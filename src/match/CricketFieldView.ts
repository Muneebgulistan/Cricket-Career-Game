import { DeliveryTrajectory } from './BallPhysics';
import { BattingShot, ShotDirection } from './BattingEngine';
import { DismissalType } from '../cricket/CricketTypes';

export type FieldPreset = 'ATTACKING' | 'BALANCED' | 'DEFENSIVE';

export interface FielderPosition {
  name: string;
  x: number; // 0 to 1 normalized field coords (0.5 = center)
  y: number;
  isInsideCircle: boolean;
}

export class CricketFieldView {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private animFrameId: number | null = null;
  private isDestroyed: boolean = false;

  // Camera & view offsets
  private cameraX: number = 0;
  private cameraY: number = 0;
  private cameraZoom: number = 1.0;
  private targetZoom: number = 1.0;

  // Active delivery animation state
  private activeTrajectory: DeliveryTrajectory | null = null;
  private animStartTime: number = 0;
  private ballCurrentPos: { x: number; y: number; z: number } | null = null;
  private hasBouncedYet: boolean = false;
  private onBounceCallback?: () => void;
  private onArrivalCallback?: () => void;
  private onCompleteCallback?: () => void;

  // Shot follow-through animation
  private shotAnim: {
    active: boolean;
    shot: BattingShot;
    direction: ShotDirection;
    runs: number;
    ballX: number;
    ballY: number;
    velX: number;
    velY: number;
    startTime: number;
  } | null = null;

  // Wicket animation state
  private wicketAnim: {
    active: boolean;
    dismissalType: DismissalType;
    bailsOffset: number;
    stumpAngle: number;
    startTime: number;
  } | null = null;

  // Tactical Field Setup
  private currentPreset: FieldPreset = 'BALANCED';
  private isPowerplay: boolean = false;
  private fielders: FielderPosition[] = [];

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.updateFielders();
    this.startLoop();
  }

  public setFieldPreset(preset: FieldPreset, isPowerplay: boolean = false): void {
    this.currentPreset = preset;
    this.isPowerplay = isPowerplay;
    this.updateFielders();
  }

  private updateFielders(): void {
    // 10 fielders (excluding bowler and keeper who are at fixed pitch spots)
    if (this.currentPreset === 'ATTACKING') {
      this.fielders = [
        { name: '1st Slip', x: 0.44, y: 0.72, isInsideCircle: true },
        { name: '2nd Slip', x: 0.41, y: 0.73, isInsideCircle: true },
        { name: 'Gully', x: 0.35, y: 0.69, isInsideCircle: true },
        { name: 'Point', x: 0.28, y: 0.64, isInsideCircle: true },
        { name: 'Cover', x: 0.32, y: 0.52, isInsideCircle: true },
        { name: 'Mid-Off', x: 0.42, y: 0.44, isInsideCircle: true },
        { name: 'Mid-On', x: 0.58, y: 0.44, isInsideCircle: true },
        { name: 'Mid-Wicket', x: 0.66, y: 0.54, isInsideCircle: true },
        { name: 'Square Leg', x: 0.70, y: 0.64, isInsideCircle: true },
        { name: 'Deep Fine Leg', x: 0.78, y: 0.82, isInsideCircle: false }
      ];
    } else if (this.currentPreset === 'DEFENSIVE') {
      this.fielders = [
        { name: 'Deep Point', x: 0.18, y: 0.62, isInsideCircle: false },
        { name: 'Deep Cover', x: 0.20, y: 0.45, isInsideCircle: false },
        { name: 'Long-Off', x: 0.38, y: 0.18, isInsideCircle: false },
        { name: 'Long-On', x: 0.62, y: 0.18, isInsideCircle: false },
        { name: 'Deep Mid-Wicket', x: 0.80, y: 0.45, isInsideCircle: false },
        { name: 'Deep Square Leg', x: 0.82, y: 0.65, isInsideCircle: false },
        { name: 'Third Man', x: 0.22, y: 0.82, isInsideCircle: false },
        { name: 'Cover Point', x: 0.33, y: 0.60, isInsideCircle: true },
        { name: 'Mid-Wicket Ring', x: 0.66, y: 0.56, isInsideCircle: true },
        { name: 'Short Fine Leg', x: 0.64, y: 0.72, isInsideCircle: true }
      ];
    } else {
      // Balanced
      this.fielders = [
        { name: 'Slip', x: 0.43, y: 0.72, isInsideCircle: true },
        { name: 'Point', x: 0.28, y: 0.63, isInsideCircle: true },
        { name: 'Cover', x: 0.30, y: 0.50, isInsideCircle: true },
        { name: 'Mid-Off', x: 0.42, y: 0.42, isInsideCircle: true },
        { name: 'Mid-On', x: 0.58, y: 0.42, isInsideCircle: true },
        { name: 'Mid-Wicket', x: 0.68, y: 0.52, isInsideCircle: true },
        { name: 'Square Leg', x: 0.72, y: 0.64, isInsideCircle: true },
        { name: 'Third Man', x: 0.24, y: 0.84, isInsideCircle: false },
        { name: 'Deep Cover', x: 0.18, y: 0.42, isInsideCircle: false },
        { name: 'Deep Square Leg', x: 0.80, y: 0.68, isInsideCircle: false }
      ];
    }

    // Powerplay enforcement (max 2 outside 30-yard ring)
    if (this.isPowerplay) {
      let outsideCount = 0;
      this.fielders.forEach(f => {
        if (!f.isInsideCircle) {
          outsideCount++;
          if (outsideCount > 2) {
            // Pull into ring
            f.isInsideCircle = true;
            f.x = 0.5 + (f.x - 0.5) * 0.6;
            f.y = 0.6 + (f.y - 0.6) * 0.6;
          }
        }
      });
    }
  }

  public startDeliveryAnimation(
    trajectory: DeliveryTrajectory,
    onBounce?: () => void,
    onArrival?: () => void,
    onComplete?: () => void
  ): void {
    this.activeTrajectory = trajectory;
    this.animStartTime = performance.now();
    this.hasBouncedYet = false;
    this.onBounceCallback = onBounce;
    this.onArrivalCallback = onArrival;
    this.onCompleteCallback = onComplete;
    this.ballCurrentPos = null;
    this.shotAnim = null;
    this.wicketAnim = null;
    this.targetZoom = 1.15; // Subtle camera zoom into batsman
  }

  public triggerShotAnimation(shot: BattingShot, direction: ShotDirection, runs: number): void {
    let velX = 0;
    let velY = -4;

    if (direction === ShotDirection.LEFT) {
      velX = -3.5;
      velY = runs >= 4 ? -2.5 : -1.0;
    } else if (direction === ShotDirection.RIGHT) {
      velX = 3.5;
      velY = runs >= 4 ? -2.5 : -1.0;
    } else {
      velX = 0;
      velY = -5.0;
    }

    if (runs === 6) {
      velX *= 1.3;
      velY *= 1.4;
      this.targetZoom = 0.90; // Pull camera out to view high maximum
    } else if (runs === 4) {
      velX *= 1.15;
      velY *= 1.15;
    }

    this.shotAnim = {
      active: true,
      shot,
      direction,
      runs,
      ballX: this.canvas.width / 2,
      ballY: this.canvas.height * 0.70,
      velX,
      velY,
      startTime: performance.now()
    };
  }

  public triggerWicketAnimation(dismissalType: DismissalType): void {
    this.wicketAnim = {
      active: true,
      dismissalType,
      bailsOffset: 0,
      stumpAngle: 0,
      startTime: performance.now()
    };
    this.targetZoom = 1.35; // Dramatic close-up on shattered stumps
  }

  private startLoop(): void {
    const loop = () => {
      if (this.isDestroyed) return;
      this.update();
      this.render();
      this.animFrameId = requestAnimationFrame(loop);
    };
    this.animFrameId = requestAnimationFrame(loop);
  }

  private update(): void {
    // Smooth camera lerp
    this.cameraZoom += (this.targetZoom - this.cameraZoom) * 0.08;

    // Delivery progress
    if (this.activeTrajectory) {
      const elapsed = performance.now() - this.animStartTime;
      const duration = this.activeTrajectory.totalDurationMs;

      // Find current trajectory point via time
      const points = this.activeTrajectory.points;
      let matched = points[0];

      for (let i = 0; i < points.length; i++) {
        if (points[i].timeMs >= elapsed) {
          matched = points[i];
          break;
        }
        matched = points[points.length - 1];
      }

      this.ballCurrentPos = { x: matched.x, y: matched.y, z: matched.z };

      // Bounce check
      if (!this.hasBouncedYet && matched.isBounced) {
        this.hasBouncedYet = true;
        if (this.onBounceCallback) this.onBounceCallback();
      }

      // Arrival check at batsman
      if (elapsed >= duration * 0.92 && this.onArrivalCallback) {
        this.onArrivalCallback();
        this.onArrivalCallback = undefined;
      }

      // Complete check
      if (elapsed >= duration) {
        const cb = this.onCompleteCallback;
        this.activeTrajectory = null;
        this.onCompleteCallback = undefined;
        if (cb) cb();
      }
    }

    // Shot flight progress
    if (this.shotAnim && this.shotAnim.active) {
      const elapsed = performance.now() - this.shotAnim.startTime;
      this.shotAnim.ballX += this.shotAnim.velX;
      this.shotAnim.ballY += this.shotAnim.velY;

      // Decelerate friction
      this.shotAnim.velX *= 0.985;
      this.shotAnim.velY *= 0.985;

      if (elapsed > 2000) {
        this.shotAnim.active = false;
        this.targetZoom = 1.0;
      }
    }

    // Wicket progress
    if (this.wicketAnim && this.wicketAnim.active) {
      const elapsed = performance.now() - this.wicketAnim.startTime;
      this.wicketAnim.bailsOffset = Math.min(30, elapsed * 0.08);
      this.wicketAnim.stumpAngle = Math.min(0.4, elapsed * 0.001);

      if (elapsed > 2200) {
        this.wicketAnim.active = false;
        this.targetZoom = 1.0;
      }
    }
  }

  private render(): void {
    const ctx = this.ctx;
    const w = this.canvas.width;
    const h = this.canvas.height;

    ctx.save();
    ctx.clearRect(0, 0, w, h);

    // Apply Camera Zoom centered on pitch
    ctx.translate(w / 2, h * 0.55);
    ctx.scale(this.cameraZoom, this.cameraZoom);
    ctx.translate(-w / 2, -h * 0.55);

    // 1. Stadium Grass Field (Oval)
    const fieldGrad = ctx.createRadialGradient(w / 2, h * 0.55, 40, w / 2, h * 0.55, w * 0.55);
    fieldGrad.addColorStop(0, '#1d3e23');
    fieldGrad.addColorStop(0.7, '#142c19');
    fieldGrad.addColorStop(1, '#0c1a0f');

    ctx.fillStyle = fieldGrad;
    ctx.beginPath();
    ctx.ellipse(w / 2, h * 0.55, w * 0.46, h * 0.44, 0, 0, Math.PI * 2);
    ctx.fill();

    // Boundary Rope
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 3;
    ctx.setLineDash([8, 6]);
    ctx.stroke();
    ctx.setLineDash([]);

    // 30-Yard Infield Circle
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.25)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.ellipse(w / 2, h * 0.57, w * 0.26, h * 0.24, 0, 0, Math.PI * 2);
    ctx.stroke();

    // 2. Pitch Strip (Perspective rectangular strip)
    const pitchW_Top = 32;
    const pitchW_Bot = 48;
    const pitchY_Top = h * 0.28;
    const pitchY_Bot = h * 0.72;

    const pitchGrad = ctx.createLinearGradient(0, pitchY_Top, 0, pitchY_Bot);
    pitchGrad.addColorStop(0, '#c2a675');
    pitchGrad.addColorStop(1, '#d8bc88');

    ctx.fillStyle = pitchGrad;
    ctx.beginPath();
    ctx.moveTo(w / 2 - pitchW_Top / 2, pitchY_Top);
    ctx.lineTo(w / 2 + pitchW_Top / 2, pitchY_Top);
    ctx.lineTo(w / 2 + pitchW_Bot / 2, pitchY_Bot);
    ctx.lineTo(w / 2 - pitchW_Bot / 2, pitchY_Bot);
    ctx.closePath();
    ctx.fill();

    // Pitch Crease Lines
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;

    // Bowler's Crease (Top)
    ctx.beginPath();
    ctx.moveTo(w / 2 - pitchW_Top / 2 - 6, pitchY_Top + 8);
    ctx.lineTo(w / 2 + pitchW_Top / 2 + 6, pitchY_Top + 8);
    ctx.stroke();

    // Batsman's Popping Crease (Bottom)
    ctx.beginPath();
    ctx.moveTo(w / 2 - pitchW_Bot / 2 - 8, pitchY_Bot - 12);
    ctx.lineTo(w / 2 + pitchW_Bot / 2 + 8, pitchY_Bot - 12);
    ctx.stroke();

    // 3. Stumps & Bails
    this.renderStumps(ctx, w / 2, pitchY_Top + 6, 8, false); // Bowler's end
    this.renderStumps(ctx, w / 2, pitchY_Bot - 6, 14, true);  // Batsman's end

    // 4. Batsman Character
    this.renderBatter(ctx, w / 2, pitchY_Bot - 8);

    // 5. Wicketkeeper
    this.renderKeeper(ctx, w / 2, pitchY_Bot + 14);

    // 6. Bowler Character
    this.renderBowler(ctx, w / 2 - 14, pitchY_Top - 4);

    // 7. Fielders
    this.renderFielders(ctx, w, h);

    // 8. Ball in Delivery Flight
    if (this.ballCurrentPos) {
      this.renderDeliveryBall(ctx, w, pitchY_Top, pitchY_Bot, pitchW_Bot);
    }

    // 9. Ball in Shot Flight
    if (this.shotAnim && this.shotAnim.active) {
      this.renderShotBall(ctx);
    }

    ctx.restore();
  }

  private renderStumps(ctx: CanvasRenderingContext2D, x: number, y: number, height: number, isBatsmanEnd: boolean): void {
    ctx.save();
    ctx.translate(x, y);

    let angle = 0;
    let bailLift = 0;

    if (isBatsmanEnd && this.wicketAnim && this.wicketAnim.active) {
      angle = this.wicketAnim.stumpAngle;
      bailLift = this.wicketAnim.bailsOffset;
    }

    // Stumps (Off, Middle, Leg)
    ctx.fillStyle = '#f8fafc';
    [-4, 0, 4].forEach(offset => {
      ctx.save();
      ctx.translate(offset, 0);
      ctx.rotate(offset === 0 ? angle : angle * 1.3);
      ctx.fillRect(-1.5, -height, 3, height);
      ctx.restore();
    });

    // Bails
    ctx.fillStyle = '#f59e0b';
    ctx.fillRect(-6, -height - 2 - bailLift, 12, 2.5);

    ctx.restore();
  }

  private renderBatter(ctx: CanvasRenderingContext2D, x: number, y: number): void {
    ctx.save();
    ctx.translate(x - 8, y);

    // Shadow
    ctx.fillStyle = 'rgba(0, 0, 0, 0.4)';
    ctx.beginPath();
    ctx.ellipse(0, 0, 8, 4, 0, 0, Math.PI * 2);
    ctx.fill();

    // Legs / Pads
    ctx.fillStyle = '#f8fafc';
    ctx.fillRect(-4, -14, 3, 14);
    ctx.fillRect(1, -14, 3, 14);

    // Torso (Jersey)
    ctx.fillStyle = '#10b981';
    ctx.fillRect(-5, -28, 10, 14);

    // Helmet
    ctx.fillStyle = '#065f46';
    ctx.beginPath();
    ctx.arc(0, -32, 5, 0, Math.PI * 2);
    ctx.fill();

    // Bat
    ctx.fillStyle = '#eab308';
    ctx.save();
    ctx.translate(4, -10);
    ctx.rotate(-0.35);
    ctx.fillRect(0, -18, 3, 20);
    ctx.restore();

    ctx.restore();
  }

  private renderKeeper(ctx: CanvasRenderingContext2D, x: number, y: number): void {
    ctx.save();
    ctx.translate(x, y);

    // Shadow
    ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
    ctx.beginPath();
    ctx.ellipse(0, 0, 6, 3, 0, 0, Math.PI * 2);
    ctx.fill();

    // Crouched Torso
    ctx.fillStyle = '#0284c7';
    ctx.fillRect(-4, -14, 8, 10);

    // Keeper Helmet
    ctx.fillStyle = '#0369a1';
    ctx.beginPath();
    ctx.arc(0, -17, 4, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
  }

  private renderBowler(ctx: CanvasRenderingContext2D, x: number, y: number): void {
    ctx.save();
    ctx.translate(x, y);

    // Shadow
    ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
    ctx.beginPath();
    ctx.ellipse(0, 0, 7, 3, 0, 0, Math.PI * 2);
    ctx.fill();

    // Bowler Torso
    ctx.fillStyle = '#0284c7';
    ctx.fillRect(-4, -20, 8, 12);

    // Head
    ctx.fillStyle = '#fde047';
    ctx.beginPath();
    ctx.arc(0, -24, 4, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();
  }

  private renderFielders(ctx: CanvasRenderingContext2D, w: number, h: number): void {
    this.fielders.forEach(f => {
      const fx = f.x * w;
      const fy = f.y * h;

      ctx.save();
      ctx.translate(fx, fy);

      // Shadow
      ctx.fillStyle = 'rgba(0, 0, 0, 0.3)';
      ctx.beginPath();
      ctx.ellipse(0, 0, 5, 2.5, 0, 0, Math.PI * 2);
      ctx.fill();

      // Fielder Body
      ctx.fillStyle = '#0284c7';
      ctx.fillRect(-3, -12, 6, 9);

      // Head
      ctx.fillStyle = '#fde047';
      ctx.beginPath();
      ctx.arc(0, -15, 3, 0, Math.PI * 2);
      ctx.fill();

      // Tiny Name Label
      ctx.fillStyle = 'rgba(255, 255, 255, 0.6)';
      ctx.font = '8px Inter, sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText(f.name, 0, 10);

      ctx.restore();
    });
  }

  private renderDeliveryBall(
    ctx: CanvasRenderingContext2D,
    w: number,
    pitchY_Top: number,
    pitchY_Bot: number,
    pitchW_Bot: number
  ): void {
    if (!this.ballCurrentPos) return;

    // Map 3D coords (x: -1m to 1m, y: 0m to 20.12m, z: 0m to 2.5m) to 2D Canvas
    const progressY = Math.min(1, Math.max(0, this.ballCurrentPos.y / 20.12));
    const screenY = pitchY_Top + progressY * (pitchY_Bot - pitchY_Top);

    // Perspective pitch width interpolation
    const currentPitchW = 32 + progressY * (pitchW_Bot - 32);
    const screenX = (w / 2) + (this.ballCurrentPos.x * (currentPitchW / 1.5));

    // Height above pitch lifts ball visually upwards
    const heightOffset = this.ballCurrentPos.z * 16;
    const ballRadius = Math.max(3, 4 + progressY * 3);

    // Ground Shadow
    ctx.fillStyle = 'rgba(0, 0, 0, 0.4)';
    ctx.beginPath();
    ctx.ellipse(screenX, screenY, ballRadius * 0.9, ballRadius * 0.45, 0, 0, Math.PI * 2);
    ctx.fill();

    // Glowing Red Leather Ball
    ctx.fillStyle = '#dc2626';
    ctx.beginPath();
    ctx.arc(screenX, screenY - heightOffset, ballRadius, 0, Math.PI * 2);
    ctx.fill();

    // Ball Specular Highlight & White Seam
    ctx.fillStyle = '#fca5a5';
    ctx.beginPath();
    ctx.arc(screenX - ballRadius * 0.3, screenY - heightOffset - ballRadius * 0.3, ballRadius * 0.35, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.arc(screenX, screenY - heightOffset, ballRadius * 0.9, 0, Math.PI);
    ctx.stroke();
  }

  private renderShotBall(ctx: CanvasRenderingContext2D): void {
    if (!this.shotAnim) return;

    ctx.fillStyle = '#dc2626';
    ctx.beginPath();
    ctx.arc(this.shotAnim.ballX, this.shotAnim.ballY, 5, 0, Math.PI * 2);
    ctx.fill();

    // Motion trail
    ctx.strokeStyle = 'rgba(239, 68, 68, 0.4)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(this.shotAnim.ballX, this.shotAnim.ballY);
    ctx.lineTo(this.shotAnim.ballX - this.shotAnim.velX * 3, this.shotAnim.ballY - this.shotAnim.velY * 3);
    ctx.stroke();
  }

  public destroy(): void {
    this.isDestroyed = true;
    if (this.animFrameId !== null) {
      cancelAnimationFrame(this.animFrameId);
    }
  }
}
