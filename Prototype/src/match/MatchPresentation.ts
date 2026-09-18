import { MatchInstance, InningsScorecard } from './MatchModel';
import { PlayerData } from '../player/PlayerModel';
import { PitchCondition } from './BallPhysics';

export interface TossResult {
  winnerTeamId: string;
  winnerTeamName: string;
  electedTo: 'BAT' | 'BOWL';
  callChoice: 'HEADS' | 'TAILS';
  coinLanded: 'HEADS' | 'TAILS';
  summaryText: string;
}

export interface MatchConditions {
  pitchType: PitchCondition;
  pitchDescription: string;
  weather: string;
  outfieldSpeed: 'LIGHTNING_FAST' | 'STANDARD' | 'SLOW';
  temperatureC: number;
}

export class MatchPresentation {
  /**
   * Generates realistic atmospheric match conditions based on venue and random variance.
   */
  public static generateMatchConditions(venueName: string): MatchConditions {
    const isSubcontinent = venueName.toLowerCase().includes('lahore') ||
      venueName.toLowerCase().includes('karachi') ||
      venueName.toLowerCase().includes('rawalpindi') ||
      venueName.toLowerCase().includes('mumbai') ||
      venueName.toLowerCase().includes('dhaka') ||
      venueName.toLowerCase().includes('youth');

    const isEnglishOrOceania = venueName.toLowerCase().includes('melbourne') ||
      venueName.toLowerCase().includes('sydney') ||
      venueName.toLowerCase().includes('lords') ||
      venueName.toLowerCase().includes('oval');

    let pitchType: PitchCondition = 'BALANCED';
    let pitchDescription = 'Even surface with consistent carry. Fair contest between bat and ball.';
    let weather = 'Clear sunny skies with a light breeze';
    let outfieldSpeed: 'LIGHTNING_FAST' | 'STANDARD' | 'SLOW' = 'STANDARD';
    let temperatureC = 26;

    if (isSubcontinent) {
      if (Math.random() < 0.5) {
        pitchType = 'DUSTY_SPIN';
        pitchDescription = 'Dry, abrasive turf. Spinners will find sharp turn and variable bounce as the match wears on.';
      } else {
        pitchType = 'HARD_BOUNCY';
        pitchDescription = 'Hard-rolled surface offering high bounce and value for shots through the line.';
      }
      weather = 'Warm, dry afternoon with haze overhead';
      temperatureC = 31;
      outfieldSpeed = 'LIGHTNING_FAST';
    } else if (isEnglishOrOceania) {
      pitchType = 'GREEN_SEAM';
      pitchDescription = 'Lush green tinge with morning moisture. Expect lively movement off the seam with the new ball.';
      weather = 'Overcast conditions with brisk wind assisting conventional swing';
      temperatureC = 19;
      outfieldSpeed = 'STANDARD';
    }

    return {
      pitchType,
      pitchDescription,
      weather,
      outfieldSpeed,
      temperatureC
    };
  }

  /**
   * Resolves an interactive coin toss.
   */
  public static conductToss(
    playerTeamId: string,
    playerTeamName: string,
    opponentTeamId: string,
    opponentTeamName: string,
    playerCall: 'HEADS' | 'TAILS',
    conditions: MatchConditions
  ): TossResult {
    const coinLanded: 'HEADS' | 'TAILS' = Math.random() < 0.5 ? 'HEADS' : 'TAILS';
    const isPlayerWon = coinLanded === playerCall;

    let winnerTeamId = isPlayerWon ? playerTeamId : opponentTeamId;
    let winnerTeamName = isPlayerWon ? playerTeamName : opponentTeamName;

    // AI or default choice
    let electedTo: 'BAT' | 'BOWL' = 'BAT';
    if (conditions.pitchType === 'GREEN_SEAM') {
      electedTo = 'BOWL'; // Bowl first to exploit early moisture
    } else if (conditions.pitchType === 'DUSTY_SPIN') {
      electedTo = 'BAT';  // Bat first before wicket deteriorates
    } else {
      electedTo = Math.random() < 0.55 ? 'BAT' : 'BOWL';
    }

    const summaryText = `${winnerTeamName} won the toss and elected to ${electedTo} first.`;

    return {
      winnerTeamId,
      winnerTeamName,
      electedTo,
      callChoice: playerCall,
      coinLanded,
      summaryText
    };
  }

  /**
   * Formats the presentation intro text.
   */
  public static getIntroSummary(match: MatchInstance, conditions: MatchConditions): string {
    return `Welcome to ${match.venue} for this ${match.format} clash between ${match.teamA.name} and ${match.teamB.name}! Pitch: ${conditions.pitchType} - ${conditions.pitchDescription}. Weather: ${conditions.weather} (${conditions.temperatureC}°C).`;
  }
}
