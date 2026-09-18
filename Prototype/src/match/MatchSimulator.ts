import { MatchInstance, InningsScorecard, PlayerMatchPerformance, BatsmanInningsEntry, BowlerInningsEntry } from './MatchModel';
import { PlayerData, PlayerRole } from '../player/PlayerModel';
import { TournamentInstance, TournamentFixture } from '../tournament/TournamentModel';
import { MATCH_FORMAT_RULES, DismissalType } from '../cricket/CricketTypes';
import { CommentaryEngine } from '../cricket/CommentaryEngine';
import { PerformanceEvaluator } from '../career/PerformanceEvaluator';

export class MatchSimulator {
  /**
   * Initializes a new MatchInstance ready for simulation.
   */
  public static initializeMatch(
    player: PlayerData,
    tournament: TournamentInstance,
    fixture: TournamentFixture
  ): MatchInstance {
    const rules = MATCH_FORMAT_RULES[tournament.format];
    const oversPerSide = rules.totalOvers;

    const teamA = { id: fixture.teamAId, name: fixture.teamAName, shortName: fixture.teamAName.substring(0, 3).toUpperCase() };
    const teamB = { id: fixture.teamBId, name: fixture.teamBName, shortName: fixture.teamBName.substring(0, 3).toUpperCase() };

    const innings1: InningsScorecard = {
      battingTeamId: teamA.id,
      battingTeamName: teamA.name,
      bowlingTeamId: teamB.id,
      bowlingTeamName: teamB.name,
      totalRuns: 0,
      totalWickets: 0,
      oversCompleted: 0,
      ballsInCurrentOver: 0,
      isCompleted: false,
      batsmen: [],
      bowlers: [],
      extras: { wides: 0, noBalls: 0, byes: 0, legByes: 0, total: 0 }
    };

    const innings2: InningsScorecard = {
      battingTeamId: teamB.id,
      battingTeamName: teamB.name,
      bowlingTeamId: teamA.id,
      bowlingTeamName: teamA.name,
      totalRuns: 0,
      totalWickets: 0,
      oversCompleted: 0,
      ballsInCurrentOver: 0,
      isCompleted: false,
      batsmen: [],
      bowlers: [],
      extras: { wides: 0, noBalls: 0, byes: 0, legByes: 0, total: 0 }
    };

    const playerPerformance: PlayerMatchPerformance = {
      playerId: player.id,
      playerName: `${player.firstName} ${player.lastName}`,
      isPlayerTeamWinner: false,
      didBat: false,
      runs: 0,
      balls: 0,
      fours: 0,
      sixes: 0,
      isOut: false,
      dismissal: 'Not Out',
      didBowl: false,
      overs: 0,
      maidens: 0,
      runsConceded: 0,
      wickets: 0,
      catches: 0,
      runOuts: 0,
      stumpings: 0
    };

    return {
      id: `match_${Date.now()}_${Math.floor(Math.random() * 1000)}`,
      tournamentId: tournament.id,
      tournamentName: tournament.name,
      fixtureId: fixture.id,
      teamA,
      teamB,
      venue: fixture.venueName,
      date: new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
      format: tournament.format,
      oversPerSide,
      currentInningsIndex: 0,
      innings: [innings1, innings2],
      isCompleted: false,
      playerPerformance,
      commentaryLog: []
    };
  }

  /**
   * Simulates the entire match from start to finish in one pass.
   */
  public static simulateFullMatch(match: MatchInstance, player: PlayerData): MatchInstance {
    // Simulate First Innings
    this.simulateInnings(match, 0, player);

    // Target for Second Innings
    match.targetRuns = match.innings[0].totalRuns + 1;
    match.currentInningsIndex = 1;

    // Simulate Second Innings
    this.simulateInnings(match, 1, player);

    // Conclude Match
    this.concludeMatch(match, player);

    return match;
  }

  /**
   * Simulates a single innings up to target or overs completion.
   */
  private static simulateInnings(match: MatchInstance, inningsIndex: number, player: PlayerData): void {
    const innings = match.innings[inningsIndex];
    const isPlayerBattingTeam = innings.battingTeamId === player.career.currentTeam || innings.battingTeamName.includes(player.nationality);
    const isPlayerBowlingTeam = !isPlayerBattingTeam;

    const target = match.targetRuns;
    const maxOvers = match.oversPerSide;

    // Roster names
    const battingLineup = this.generateLineup(innings.battingTeamName, isPlayerBattingTeam, player);
    const bowlingAttack = this.generateBowlers(innings.bowlingTeamName, isPlayerBowlingTeam, player);

    // Populate scorecard entries
    innings.batsmen = battingLineup.map(p => ({
      playerId: p.id,
      name: p.name,
      runs: 0,
      balls: 0,
      fours: 0,
      sixes: 0,
      isOut: false,
      dismissalText: 'Not Out'
    }));

    innings.bowlers = bowlingAttack.map(b => ({
      playerId: b.id,
      name: b.name,
      overs: 0,
      maidens: 0,
      runs: 0,
      wickets: 0,
      economy: 0.0
    }));

    let strikerIdx = 0;
    let nonStrikerIdx = 1;
    let nextBatsmanIdx = 2;

    for (let over = 1; over <= maxOvers; over++) {
      if (innings.totalWickets >= 10) break;
      if (target && innings.totalRuns >= target) break;

      const bowlerIdx = (over - 1) % bowlingAttack.length;
      const bowler = bowlingAttack[bowlerIdx];
      const bowlerScorecard = innings.bowlers[bowlerIdx];

      let runsInThisOver = 0;
      let isUserBowlingThisOver = bowler.isUser;

      for (let ball = 1; ball <= 6; ball++) {
        if (innings.totalWickets >= 10) break;
        if (target && innings.totalRuns >= target) break;

        const striker = battingLineup[strikerIdx];
        const strikerScorecard = innings.batsmen[strikerIdx];
        const isUserBatting = striker.isUser;

        // Calculate ball outcome based on player skills
        const outcome = this.resolveDelivery(isUserBatting, isUserBowlingThisOver, player);

        strikerScorecard.balls++;

        if (outcome.isWicket) {
          innings.totalWickets++;
          strikerScorecard.isOut = true;
          strikerScorecard.dismissalText = outcome.dismissalText;
          strikerScorecard.bowlerName = bowler.name;
          bowlerScorecard.wickets++;

          const comm = CommentaryEngine.getBallCommentary(striker.name, bowler.name, 0, true, outcome.dismissalType);
          match.commentaryLog.push(`[Ov ${over}.${ball}] ${comm}`);

          // Track user wickets
          if (isUserBowlingThisOver) {
            match.playerPerformance.didBowl = true;
            match.playerPerformance.wickets++;
          }

          // User fielding catch check
          if (!isUserBowlingThisOver && isPlayerBowlingTeam && Math.random() < 0.25) {
            match.playerPerformance.catches++;
          }

          if (isUserBatting) {
            match.playerPerformance.didBat = true;
            match.playerPerformance.isOut = true;
            match.playerPerformance.dismissal = outcome.dismissalText;
          }

          // Next batsman comes in
          if (nextBatsmanIdx < battingLineup.length) {
            strikerIdx = nextBatsmanIdx;
            nextBatsmanIdx++;
          }
        } else {
          innings.totalRuns += outcome.runs;
          runsInThisOver += outcome.runs;
          bowlerScorecard.runs += outcome.runs;
          strikerScorecard.runs += outcome.runs;

          if (outcome.runs === 4) strikerScorecard.fours++;
          if (outcome.runs === 6) strikerScorecard.sixes++;

          if (isUserBatting) {
            match.playerPerformance.didBat = true;
            match.playerPerformance.runs += outcome.runs;
            match.playerPerformance.balls++;
            if (outcome.runs === 4) match.playerPerformance.fours++;
            if (outcome.runs === 6) match.playerPerformance.sixes++;
          }

          if (isUserBowlingThisOver) {
            match.playerPerformance.didBowl = true;
            match.playerPerformance.runsConceded += outcome.runs;
          }

          // Rotate strike on odd runs
          if (outcome.runs % 2 !== 0) {
            const temp = strikerIdx;
            strikerIdx = nonStrikerIdx;
            nonStrikerIdx = temp;
          }

          if (outcome.runs >= 4 || Math.random() < 0.25) {
            const comm = CommentaryEngine.getBallCommentary(striker.name, bowler.name, outcome.runs, false);
            match.commentaryLog.push(`[Ov ${over}.${ball}] ${comm}`);
          }
        }
      }

      bowlerScorecard.overs++;
      if (runsInThisOver === 0) bowlerScorecard.maidens++;
      bowlerScorecard.economy = Math.round((bowlerScorecard.runs / bowlerScorecard.overs) * 100) / 100;

      if (isUserBowlingThisOver) {
        match.playerPerformance.overs++;
        if (runsInThisOver === 0) match.playerPerformance.maidens++;
      }

      innings.oversCompleted = over;

      // Rotate strike at end of over
      const temp = strikerIdx;
      strikerIdx = nonStrikerIdx;
      nonStrikerIdx = temp;
    }

    innings.isCompleted = true;
  }

  /**
   * Probability logic resolving a delivery between batsman and bowler.
   */
  private static resolveDelivery(
    isUserBatting: boolean,
    isUserBowling: boolean,
    player: PlayerData
  ): { runs: number; isWicket: boolean; dismissalType?: DismissalType; dismissalText: string } {
    let wicketChance = 0.045; // ~4.5% base chance of wicket per ball
    let dotChance = 0.45;
    let boundaryChance = 0.12;

    if (isUserBatting) {
      // User's technique and timing reduce wicket chance and increase boundaries
      const skillMod = (player.batting.battingTechnique + player.batting.timing) / 200; // 0.2 to 0.5
      const formMod = (player.mental.form - 50) / 200; // -0.2 to +0.2
      wicketChance = Math.max(0.015, wicketChance - (skillMod * 0.03) - (formMod * 0.02));
      boundaryChance = Math.min(0.28, boundaryChance + (player.batting.power / 400) + (formMod * 0.05));
    }

    if (isUserBowling) {
      // User's bowling ability and accuracy increase wicket chance and dot balls
      const skillMod = (player.bowling.bowlingAbility + player.bowling.accuracy) / 200;
      const formMod = (player.mental.form - 50) / 200;
      wicketChance = Math.min(0.085, wicketChance + (skillMod * 0.03) + (formMod * 0.02));
      dotChance = Math.min(0.65, dotChance + (skillMod * 0.10));
    }

    const roll = Math.random();

    if (roll < wicketChance) {
      const dismissalRoll = Math.random();
      let dismissalType = DismissalType.CAUGHT;
      let text = 'c Keeper b Bowler';

      if (dismissalRoll < 0.35) {
        dismissalType = DismissalType.CAUGHT;
        text = 'c Fielder b Bowler';
      } else if (dismissalRoll < 0.65) {
        dismissalType = DismissalType.BOWLED;
        text = 'b Bowler';
      } else if (dismissalRoll < 0.85) {
        dismissalType = DismissalType.LBW;
        text = 'lbw b Bowler';
      } else {
        dismissalType = DismissalType.RUN_OUT;
        text = 'run out';
      }

      return { runs: 0, isWicket: true, dismissalType, dismissalText: text };
    }

    // Runs roll
    const runRoll = Math.random();
    if (runRoll < dotChance) {
      return { runs: 0, isWicket: false, dismissalText: 'Not Out' };
    } else if (runRoll < dotChance + 0.30) {
      return { runs: 1, isWicket: false, dismissalText: 'Not Out' };
    } else if (runRoll < dotChance + 0.40) {
      return { runs: 2, isWicket: false, dismissalText: 'Not Out' };
    } else if (runRoll < dotChance + 0.43) {
      return { runs: 3, isWicket: false, dismissalText: 'Not Out' };
    } else if (runRoll < dotChance + 0.43 + boundaryChance) {
      return { runs: 4, isWicket: false, dismissalText: 'Not Out' };
    } else {
      return { runs: 6, isWicket: false, dismissalText: 'Not Out' };
    }
  }

  /**
   * Concludes the match, determines the winner, margin, and triggers performance evaluation.
   */
  private static concludeMatch(match: MatchInstance, player: PlayerData): void {
    match.isCompleted = true;
    const inn1 = match.innings[0];
    const inn2 = match.innings[1];

    let winnerId = '';
    let summary = '';
    let playerWon = false;

    const playerTeamId = match.teamA.name.includes(player.nationality) || inn1.battingTeamName.includes(player.nationality)
      ? match.teamA.id
      : match.teamB.id;

    if (inn2.totalRuns > inn1.totalRuns) {
      winnerId = inn2.battingTeamId;
      const wicketsLeft = 10 - inn2.totalWickets;
      summary = `${inn2.battingTeamName} won by ${wicketsLeft} wicket${wicketsLeft > 1 ? 's' : ''}`;
    } else if (inn1.totalRuns > inn2.totalRuns) {
      winnerId = inn1.battingTeamId;
      const runMargin = inn1.totalRuns - inn2.totalRuns;
      summary = `${inn1.battingTeamName} won by ${runMargin} run${runMargin > 1 ? 's' : ''}`;
    } else {
      winnerId = 'TIE';
      summary = 'Match Tied in thrilling finish!';
    }

    match.winnerTeamId = winnerId;
    match.resultSummary = summary;
    playerWon = (winnerId === playerTeamId);
    match.playerPerformance.isPlayerTeamWinner = playerWon;

    // Evaluate Player Performance
    const evalInput = {
      runsScored: match.playerPerformance.runs,
      ballsFaced: match.playerPerformance.balls,
      fours: match.playerPerformance.fours,
      sixes: match.playerPerformance.sixes,
      isOut: match.playerPerformance.isOut,

      oversBowled: match.playerPerformance.overs,
      maidens: match.playerPerformance.maidens,
      runsConceded: match.playerPerformance.runsConceded,
      wicketsTaken: match.playerPerformance.wickets,

      catches: match.playerPerformance.catches,
      runOuts: match.playerPerformance.runOuts,
      stumpings: match.playerPerformance.stumpings,

      matchResult: (playerWon ? 'WIN' : (winnerId === 'TIE' ? 'DRAW' : 'LOSS')) as 'WIN' | 'LOSS' | 'DRAW',
      isManOfTheMatch: (match.playerPerformance.runs >= 75 || match.playerPerformance.wickets >= 4)
    };

    const evaluation = PerformanceEvaluator.evaluateMatch(player, evalInput);
    match.playerPerformance.evaluation = evaluation;

    if (evalInput.isManOfTheMatch) {
      match.manOfTheMatchPlayerName = `${player.firstName} ${player.lastName}`;
    } else {
      match.manOfTheMatchPlayerName = playerWon ? `${inn1.battingTeamName} Captain` : 'Opponent Star';
    }
  }

  private static generateLineup(teamName: string, isPlayerTeam: boolean, player: PlayerData) {
    const surnames = ['Sharma', 'Smith', 'Root', 'Babar', 'Warner', 'Kohli', 'Stokes', 'Cummins', 'Bumrah', 'Shaheen', 'Rashid'];
    const lineup: Array<{ id: string; name: string; isUser: boolean }> = [];

    for (let i = 1; i <= 11; i++) {
      if (isPlayerTeam && i === 4) {
        // Player bats at #4 (ideal career showcase)
        lineup.push({ id: player.id, name: `${player.firstName} ${player.lastName}`, isUser: true });
      } else {
        const surname = surnames[(i + teamName.length) % surnames.length];
        lineup.push({ id: `p_${i}`, name: `Player ${surname}`, isUser: false });
      }
    }
    return lineup;
  }

  private static generateBowlers(teamName: string, isPlayerTeam: boolean, player: PlayerData) {
    const bowlers: Array<{ id: string; name: string; isUser: boolean }> = [];
    const surnames = ['Pacer', 'Spinner', 'Quick', 'Strike', 'Swing', 'Seamer'];

    for (let i = 0; i < 5; i++) {
      if (isPlayerTeam && i === 0 && (player.role === PlayerRole.BOWLER || player.role === PlayerRole.ALL_ROUNDER)) {
        bowlers.push({ id: player.id, name: `${player.firstName} ${player.lastName}`, isUser: true });
      } else {
        bowlers.push({ id: `b_${i}`, name: `${surnames[i % surnames.length]} (${teamName.substring(0, 3)})`, isUser: false });
      }
    }
    return bowlers;
  }
}
