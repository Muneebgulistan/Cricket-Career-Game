import { DeliveryLine, DeliveryLength } from './BattingEngine';
import { BowlingVariation } from './BowlingEngine';
import { BowlingStyle } from '../player/PlayerModel';

export type PitchCondition = 'BALANCED' | 'HARD_BOUNCY' | 'DUSTY_SPIN' | 'GREEN_SEAM';

export interface TrajectoryPoint {
  timeMs: number;
  x: number; // Lateral position in meters (-2.5m to +2.5m, 0 = middle stump)
  y: number; // Pitch distance in meters (0 = bowler release, 20.12 = batsman crease)
  z: number; // Height in meters (0 = pitch surface)
  isBounced: boolean;
}

export interface DeliveryTrajectory {
  points: TrajectoryPoint[];
  totalDurationMs: number;
  bounceTimeMs: number;
  bouncePoint: { x: number; y: number; z: number };
  arrivalPoint: { x: number; y: number; z: number };
  releasePaceKph: number;
  arrivalPaceKph: number;
  swingAngleDeg: number;
  seamDeviationDeg: number;
  spinTurnDeg: number;
}

export class BallPhysics {
  public static readonly PITCH_LENGTH_METERS = 20.12;
  public static readonly STUMPS_HEIGHT_METERS = 0.71;
  public static readonly STUMP_WIDTH_METERS = 0.23;

  /**
   * Translates DeliveryLine enum to pitch target X coordinate (in meters).
   */
  public static lineToMeters(line: DeliveryLine): number {
    switch (line) {
      case DeliveryLine.WIDE_OUTSIDE_OFF:
        return -0.85;
      case DeliveryLine.OUTSIDE_OFF:
        return -0.40;
      case DeliveryLine.MIDDLE:
        return 0.0;
      case DeliveryLine.LEG_STUMP:
        return 0.25;
      case DeliveryLine.DOWN_LEG:
        return 0.75;
      default:
        return 0.0;
    }
  }

  /**
   * Translates DeliveryLength enum to ideal pitch bounce distance Y (in meters from bowler).
   */
  public static lengthToMeters(length: DeliveryLength): number {
    switch (length) {
      case DeliveryLength.YORKER:
        return 18.5; // Very full, right at the popping crease
      case DeliveryLength.FULL:
        return 16.0; // Driving length
      case DeliveryLength.GOOD_LENGTH:
        return 13.5; // Top of off stump zone
      case DeliveryLength.SHORT:
        return 9.5;  // Pullable length
      case DeliveryLength.BOUNCER:
        return 7.0;  // Digging it in short
      default:
        return 13.5;
    }
  }

  /**
   * Determines broader bowling category from player's specific bowling style.
   */
  public static categorizeStyle(style: BowlingStyle): 'FAST' | 'MEDIUM' | 'SPIN' {
    if (style === BowlingStyle.RIGHT_ARM_FAST || style === BowlingStyle.LEFT_ARM_FAST) {
      return 'FAST';
    }
    if (style === BowlingStyle.RIGHT_ARM_MEDIUM || style === BowlingStyle.LEFT_ARM_MEDIUM) {
      return 'MEDIUM';
    }
    if (
      style === BowlingStyle.RIGHT_ARM_OFF_SPIN ||
      style === BowlingStyle.RIGHT_ARM_LEG_SPIN ||
      style === BowlingStyle.LEFT_ARM_ORTHODOX ||
      style === BowlingStyle.LEFT_ARM_UNORTHODOX
    ) {
      return 'SPIN';
    }
    return 'MEDIUM';
  }

  /**
   * Calculates a complete deterministic 3D delivery trajectory from bowler release to batsman contact.
   */
  public static calculateTrajectory(
    line: DeliveryLine,
    length: DeliveryLength,
    variation: BowlingVariation,
    style: BowlingStyle,
    pitchCondition: PitchCondition = 'BALANCED',
    basePaceKph: number = 130
  ): DeliveryTrajectory {
    const category = this.categorizeStyle(style);

    // Pace scaling by category if not explicitly provided
    let paceKph = basePaceKph;
    if (category === 'FAST') paceKph = Math.max(130, Math.min(152, basePaceKph));
    if (category === 'MEDIUM') paceKph = Math.max(110, Math.min(132, basePaceKph));
    if (category === 'SPIN') paceKph = Math.max(75, Math.min(102, basePaceKph));

    // Release characteristics
    const releaseX = category === 'SPIN' ? -0.4 : -0.6; // Bowler jumps slightly wide of stumps
    const releaseY = 0.0;
    const releaseZ = category === 'SPIN' ? 1.85 : 2.15; // Higher release point for fast bowlers

    const targetBounceY = this.lengthToMeters(length);
    const targetBounceX = this.lineToMeters(line);

    // Initial forward speed in m/s
    const vY = (paceKph * 1000) / 3600; // e.g. 135 km/h = 37.5 m/s

    // Flight time to bounce point
    const timeToBounceSec = Math.max(0.2, targetBounceY / vY);

    // Gravity effect: g = 9.81 m/s^2
    const g = 9.81;

    // Initial vertical velocity required to hit targetBounceY at z=0:
    // 0 = releaseZ + vZ * t - 0.5 * g * t^2 => vZ = (0.5 * g * t^2 - releaseZ) / t
    const vZ = (0.5 * g * Math.pow(timeToBounceSec, 2) - releaseZ) / timeToBounceSec;

    // Lateral velocity to reach bounceX from releaseX:
    const vX = (targetBounceX - releaseX) / timeToBounceSec;

    // Swing / Seam / Spin effects
    let swingAccelX = 0; // m/s^2 aerodynamic lateral acceleration
    let seamDeviationM = 0; // Lateral deflection upon bounce
    let spinTurnM = 0; // Lateral break upon bounce
    let bounceRestitution = 0.62; // z-velocity restitution

    // Adjust for pitch condition
    if (pitchCondition === 'HARD_BOUNCY') {
      bounceRestitution = 0.70;
    } else if (pitchCondition === 'DUSTY_SPIN') {
      bounceRestitution = 0.55;
    } else if (pitchCondition === 'GREEN_SEAM') {
      bounceRestitution = 0.64;
    }

    // Variation-specific dynamics
    if (variation === BowlingVariation.OUTSWING) {
      swingAccelX = -1.6; // Drifts outward (off-side)
    } else if (variation === BowlingVariation.INSWING) {
      swingAccelX = 1.6;  // Swings in toward stumps
    } else if (variation === BowlingVariation.REVERSE_SWING) {
      swingAccelX = 2.4;  // Late sharp in-swing
    } else if (variation === BowlingVariation.OFF_CUTTER) {
      seamDeviationM = 0.18; // Grips and cuts in
    } else if (variation === BowlingVariation.LEG_CUTTER) {
      seamDeviationM = -0.18; // Cuts away
    } else if (variation === BowlingVariation.SHARP_TURN) {
      spinTurnM = (style === BowlingStyle.RIGHT_ARM_OFF_SPIN || style === BowlingStyle.LEFT_ARM_ORTHODOX) ? 0.35 : -0.35;
      if (pitchCondition === 'DUSTY_SPIN') spinTurnM *= 1.4;
    } else if (variation === BowlingVariation.ARM_BALL || variation === BowlingVariation.QUICKER_BALL) {
      paceKph += 6;
      spinTurnM = 0.05; // Skids straight on
    } else if (variation === BowlingVariation.WRIST_VARIATION) {
      spinTurnM = 0.28; // Googly spins contrary to stock delivery
    } else if (variation === BowlingVariation.FLIGHTED) {
      paceKph -= 6;
      bounceRestitution += 0.05;
    }

    // Step simulation through time
    const points: TrajectoryPoint[] = [];
    const stepMs = 20; // 50 updates per second
    const totalSimDurationSec = this.PITCH_LENGTH_METERS / (vY * 0.88); // Slight deceleration from air resistance
    const totalSteps = Math.ceil((totalSimDurationSec * 1000) / stepMs);

    let curX = releaseX;
    let curY = releaseY;
    let curZ = releaseZ;
    let curVX = vX;
    let curVY = vY;
    let curVZ = vZ;
    let hasBounced = false;
    let bounceTimeMs = 0;
    let bouncePoint = { x: 0, y: 0, z: 0 };

    points.push({
      timeMs: 0,
      x: Math.round(curX * 1000) / 1000,
      y: 0,
      z: Math.round(curZ * 1000) / 1000,
      isBounced: false
    });

    for (let step = 1; step <= totalSteps; step++) {
      const timeMs = step * stepMs;
      const dt = stepMs / 1000;

      // Air resistance and lateral swing before bounce
      if (!hasBounced) {
        curVX += swingAccelX * dt;
        curVZ -= g * dt;
      } else {
        curVZ -= g * dt;
      }

      curX += curVX * dt;
      curY += curVY * dt;
      curZ += curVZ * dt;

      // Check for pitch bounce
      if (curZ <= 0 && !hasBounced && curY >= 4.0) {
        curZ = 0;
        hasBounced = true;
        bounceTimeMs = timeMs;
        bouncePoint = { x: curX, y: curY, z: 0 };

        // Post-bounce dynamics: vertical rebound with energy loss
        curVZ = Math.abs(curVZ) * bounceRestitution;

        // Friction on turf slows forward velocity slightly
        curVY *= 0.90;

        // Apply seam nip or spin turn to lateral velocity
        const lateralImpulse = (seamDeviationM + spinTurnM) / 0.15;
        curVX += lateralImpulse;
      }

      points.push({
        timeMs,
        x: Math.round(curX * 1000) / 1000,
        y: Math.round(curY * 1000) / 1000,
        z: Math.max(0, Math.round(curZ * 1000) / 1000),
        isBounced: hasBounced
      });

      // Stop once ball reaches or crosses batsman crease (20.12m)
      if (curY >= this.PITCH_LENGTH_METERS) {
        break;
      }
    }

    const lastPoint = points[points.length - 1];
    const arrivalPoint = {
      x: lastPoint.x,
      y: this.PITCH_LENGTH_METERS,
      z: lastPoint.z
    };

    const totalDurationMs = lastPoint.timeMs;
    const arrivalPaceKph = paceKph * 0.88; // Ball typically loses ~12% pace off pitch

    return {
      points,
      totalDurationMs,
      bounceTimeMs,
      bouncePoint,
      arrivalPoint,
      releasePaceKph: paceKph,
      arrivalPaceKph: Math.round(arrivalPaceKph),
      swingAngleDeg: Math.round(Math.atan2(swingAccelX, 9.81) * (180 / Math.PI) * 10) / 10,
      seamDeviationDeg: seamDeviationM !== 0 ? Math.round(seamDeviationM * 45 * 10) / 10 : 0,
      spinTurnDeg: spinTurnM !== 0 ? Math.round(spinTurnM * 35 * 10) / 10 : 0
    };
  }

  /**
   * Helper to check if an arrival point $(x, z)$ would hit standard stumps.
   * Stumps are 23cm wide (x from -0.115 to +0.115) and 71cm high (z from 0 to 0.71).
   */
  public static isHittingStumps(arrivalPoint: { x: number; y: number; z: number }): boolean {
    const halfWidth = this.STUMP_WIDTH_METERS / 2;
    const isLineTarget = arrivalPoint.x >= -halfWidth && arrivalPoint.x <= halfWidth;
    const isHeightTarget = arrivalPoint.z >= 0.05 && arrivalPoint.z <= this.STUMPS_HEIGHT_METERS;
    return isLineTarget && isHeightTarget;
  }
}
