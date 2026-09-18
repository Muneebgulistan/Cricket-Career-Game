import { CareerLevel } from '../career/CareerLevel';
import { TOURNAMENT_TEMPLATES, TournamentTemplate } from '../data/tournaments.data';
import { TEAMS_DATABASE, TeamData } from '../data/teams.data';
import { VENUES_DATABASE } from '../data/venues.data';
import { TournamentInstance, TournamentStatus, TournamentFixture, TeamStanding } from './TournamentModel';

export class TournamentManager {
  /**
   * Creates a live TournamentInstance for the player's current career level.
   */
  public static createTournamentForLevel(level: CareerLevel, playerNationality: string): TournamentInstance {
    const template = TOURNAMENT_TEMPLATES.find(t => t.careerLevel === level) || TOURNAMENT_TEMPLATES[0];
    return this.createFromTemplate(template, playerNationality);
  }

  public static createFromTemplate(template: TournamentTemplate, playerNationality: string): TournamentInstance {
    const id = `tourn_${template.id}_${Date.now()}`;

    // Select relevant teams based on category and level
    let availableTeams: TeamData[] = [];
    if (template.careerLevel === CareerLevel.UNDER_16) {
      availableTeams = TEAMS_DATABASE.filter(t => t.category === 'YOUTH' && t.id.startsWith('u16_'));
    } else if (template.careerLevel === CareerLevel.UNDER_19) {
      availableTeams = TEAMS_DATABASE.filter(t => t.category === 'YOUTH' && t.id.startsWith('u19_'));
    } else if (template.careerLevel === CareerLevel.DOMESTIC) {
      availableTeams = TEAMS_DATABASE.filter(t => t.category === 'DOMESTIC');
    } else if (template.careerLevel === CareerLevel.COUNTRY_LEAGUE) {
      availableTeams = TEAMS_DATABASE.filter(t => t.category === 'FRANCHISE');
    } else {
      availableTeams = TEAMS_DATABASE.filter(t => t.category === 'INTERNATIONAL');
    }

    if (availableTeams.length < 2) {
      availableTeams = TEAMS_DATABASE.slice(0, template.totalTeams);
    }

    // Determine player's team
    let playerTeam = availableTeams.find(t => t.country.toLowerCase() === playerNationality.toLowerCase());
    if (!playerTeam) {
      playerTeam = availableTeams[0];
    }

    // Select opponent teams up to template.totalTeams
    const participatingTeams: TeamData[] = [playerTeam];
    for (const t of availableTeams) {
      if (participatingTeams.length >= template.totalTeams) break;
      if (t.id !== playerTeam.id) {
        participatingTeams.push(t);
      }
    }

    // Initialize Standings
    const standings: TeamStanding[] = participatingTeams.map(team => ({
      teamId: team.id,
      teamName: team.name,
      played: 0,
      won: 0,
      lost: 0,
      tied: 0,
      points: 0,
      netRunRate: 0.0
    }));

    // Generate Fixtures involving the player's team across the rounds
    const fixtures: TournamentFixture[] = [];
    let roundNum = 1;

    if (template.type === 'BILATERAL_SERIES') {
      // 5 matches against the primary opponent
      const opponent = participatingTeams.find(t => t.id !== playerTeam!.id) || participatingTeams[1];
      for (let i = 1; i <= template.totalMatches; i++) {
        const venue = VENUES_DATABASE[(i - 1) % VENUES_DATABASE.length];
        const isHome = i % 2 !== 0;
        fixtures.push({
          id: `fix_${template.id}_${i}`,
          roundNumber: i,
          teamAId: isHome ? playerTeam.id : opponent.id,
          teamAName: isHome ? playerTeam.name : opponent.name,
          teamBId: isHome ? opponent.id : playerTeam.id,
          teamBName: isHome ? opponent.name : playerTeam.name,
          venueName: venue.name,
          isPlayed: false
        });
      }
    } else {
      // League / Cup fixtures: Player plays each rival once or twice
      const opponents = participatingTeams.filter(t => t.id !== playerTeam!.id);
      let matchCount = 0;

      while (matchCount < template.totalMatches) {
        for (const opp of opponents) {
          if (matchCount >= template.totalMatches) break;
          matchCount++;
          const venue = VENUES_DATABASE[(matchCount - 1) % VENUES_DATABASE.length];
          fixtures.push({
            id: `fix_${template.id}_${matchCount}`,
            roundNumber: matchCount,
            teamAId: playerTeam.id,
            teamAName: playerTeam.name,
            teamBId: opp.id,
            teamBName: opp.name,
            venueName: venue.name,
            isPlayed: false
          });
        }
      }
    }

    return {
      id,
      templateId: template.id,
      name: template.name,
      type: template.type,
      careerLevel: template.careerLevel,
      format: template.format,
      status: TournamentStatus.IN_PROGRESS,
      playerTeamId: playerTeam.id,
      totalMatches: fixtures.length,
      completedMatches: 0,
      currentRound: 1,
      fixtures,
      standings,
      trophyName: template.trophyName,
      minimumPerformanceRequirement: template.minimumPerformanceRequirement
    };
  }

  /**
   * Retrieves the next upcoming fixture for the player.
   */
  public static getNextFixture(tournament: TournamentInstance): TournamentFixture | null {
    return tournament.fixtures.find(f => !f.isPlayed) || null;
  }

  /**
   * Updates tournament standings and fixture completion after a match.
   */
  public static recordMatchResult(
    tournament: TournamentInstance,
    fixtureId: string,
    winnerTeamId: string,
    marginSummary: string
  ): void {
    const fixture = tournament.fixtures.find(f => f.id === fixtureId);
    if (!fixture || fixture.isPlayed) return;

    fixture.isPlayed = true;
    fixture.winnerTeamId = winnerTeamId;
    fixture.resultSummary = marginSummary;
    tournament.completedMatches++;

    // Update Standings
    const teamAStanding = tournament.standings.find(s => s.teamId === fixture.teamAId);
    const teamBStanding = tournament.standings.find(s => s.teamId === fixture.teamBId);

    if (teamAStanding) teamAStanding.played++;
    if (teamBStanding) teamBStanding.played++;

    if (winnerTeamId === fixture.teamAId && teamAStanding && teamBStanding) {
      teamAStanding.won++;
      teamAStanding.points += 2;
      teamBStanding.lost++;
    } else if (winnerTeamId === fixture.teamBId && teamAStanding && teamBStanding) {
      teamBStanding.won++;
      teamBStanding.points += 2;
      teamAStanding.lost++;
    } else if (teamAStanding && teamBStanding) {
      teamAStanding.tied++;
      teamAStanding.points += 1;
      teamBStanding.tied++;
      teamBStanding.points += 1;
    }

    // Sort standings by points descending
    tournament.standings.sort((a, b) => b.points - a.points);

    // Check completion
    if (tournament.completedMatches >= tournament.totalMatches) {
      tournament.status = TournamentStatus.COMPLETED;
      const topTeam = tournament.standings[0];
      tournament.championTeamId = topTeam?.teamId;
      tournament.championTeamName = topTeam?.teamName;
    } else {
      tournament.currentRound++;
    }
  }
}
