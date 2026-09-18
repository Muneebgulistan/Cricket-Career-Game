/**
 * Pure Web Audio API Sound Synthesizer.
 * Provides rich atmospheric game sound effects completely offline without
 * external audio file dependencies.
 */
export class AudioManager {
  private static instance: AudioManager;
  private audioCtx: AudioContext | null = null;
  private isMuted: boolean = false;
  private masterVolume: number = 0.5;

  private constructor() {}

  public static getInstance(): AudioManager {
    if (!AudioManager.instance) {
      AudioManager.instance = new AudioManager();
    }
    return AudioManager.instance;
  }

  private initContext(): void {
    if (!this.audioCtx && typeof window !== 'undefined') {
      const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext;
      if (AudioContextClass) {
        this.audioCtx = new AudioContextClass();
      }
    }
    if (this.audioCtx && this.audioCtx.state === 'suspended') {
      this.audioCtx.resume();
    }
  }

  public setMuted(muted: boolean): void {
    this.isMuted = muted;
  }

  public setVolume(volume: number): void {
    this.masterVolume = Math.max(0, Math.min(1, volume));
  }

  /**
   * Subtle UI click sound.
   */
  public playClick(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      const osc = this.audioCtx.createOscillator();
      const gain = this.audioCtx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(800, this.audioCtx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(300, this.audioCtx.currentTime + 0.04);

      gain.gain.setValueAtTime(this.masterVolume * 0.3, this.audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx.currentTime + 0.04);

      osc.connect(gain);
      gain.connect(this.audioCtx.destination);

      osc.start();
      osc.stop(this.audioCtx.currentTime + 0.04);
    } catch {
      // Audio context might be restricted before first user gesture
    }
  }

  /**
   * Resonant wooden willow 'thwack' of bat hitting leather.
   */
  public playBatHit(powerMultiplier: number = 1.0): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      const osc = this.audioCtx.createOscillator();
      const gain = this.audioCtx.createGain();

      osc.type = 'triangle';
      const baseFreq = 220 + Math.random() * 40;
      osc.frequency.setValueAtTime(baseFreq, this.audioCtx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(60, this.audioCtx.currentTime + 0.08);

      gain.gain.setValueAtTime(this.masterVolume * 0.6 * powerMultiplier, this.audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx.currentTime + 0.08);

      osc.connect(gain);
      gain.connect(this.audioCtx.destination);

      osc.start();
      osc.stop(this.audioCtx.currentTime + 0.08);
    } catch {}
  }

  /**
   * Clattering stumps sound.
   */
  public playWicket(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      // Two quick successive wooden strikes
      [0, 0.04, 0.09].forEach((delay, idx) => {
        const osc = this.audioCtx!.createOscillator();
        const gain = this.audioCtx!.createGain();

        osc.type = 'square';
        osc.frequency.setValueAtTime(450 - idx * 70, this.audioCtx!.currentTime + delay);
        osc.frequency.exponentialRampToValueAtTime(80, this.audioCtx!.currentTime + delay + 0.12);

        gain.gain.setValueAtTime(this.masterVolume * 0.4, this.audioCtx!.currentTime + delay);
        gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx!.currentTime + delay + 0.12);

        osc.connect(gain);
        gain.connect(this.audioCtx!.destination);

        osc.start(this.audioCtx!.currentTime + delay);
        osc.stop(this.audioCtx!.currentTime + delay + 0.12);
      });
    } catch {}
  }

  /**
   * Stadium crowd cheer / roar synthesized with filtered white noise.
   */
  public playCheer(isSix: boolean = false): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      const duration = isSix ? 2.5 : 1.5;
      const bufferSize = this.audioCtx.sampleRate * duration;
      const buffer = this.audioCtx.createBuffer(1, bufferSize, this.audioCtx.sampleRate);
      const data = buffer.getChannelData(0);

      // White noise with pink fade
      for (let i = 0; i < bufferSize; i++) {
        data[i] = (Math.random() * 2 - 1) * 0.5;
      }

      const noise = this.audioCtx.createBufferSource();
      noise.buffer = buffer;

      // Bandpass filter to sound like distant roar
      const filter = this.audioCtx.createBiquadFilter();
      filter.type = 'bandpass';
      filter.frequency.setValueAtTime(isSix ? 650 : 500, this.audioCtx.currentTime);
      filter.Q.setValueAtTime(1.2, this.audioCtx.currentTime);

      const gain = this.audioCtx.createGain();
      gain.gain.setValueAtTime(0.01, this.audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(this.masterVolume * (isSix ? 0.6 : 0.4), this.audioCtx.currentTime + 0.3);
      gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx.currentTime + duration);

      noise.connect(filter);
      filter.connect(gain);
      gain.connect(this.audioCtx.destination);

      noise.start();
      noise.stop(this.audioCtx.currentTime + duration);
    } catch {}
  }

  /**
   * "HOWZAT!" umpire appeal shout.
   */
  public playAppeal(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      const osc = this.audioCtx.createOscillator();
      const gain = this.audioCtx.createGain();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(280, this.audioCtx.currentTime);
      osc.frequency.exponentialRampToValueAtTime(440, this.audioCtx.currentTime + 0.25);
      osc.frequency.exponentialRampToValueAtTime(180, this.audioCtx.currentTime + 0.6);

      gain.gain.setValueAtTime(this.masterVolume * 0.4, this.audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx.currentTime + 0.6);

      osc.connect(gain);
      gain.connect(this.audioCtx.destination);

      osc.start();
      osc.stop(this.audioCtx.currentTime + 0.6);
    } catch {}
  }

  /**
   * Clean catch applause (rapid light claps).
   */
  public playCatch(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      [0, 0.08, 0.16, 0.25, 0.35].forEach(delay => {
        const osc = this.audioCtx!.createOscillator();
        const gain = this.audioCtx!.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(520, this.audioCtx!.currentTime + delay);
        gain.gain.setValueAtTime(this.masterVolume * 0.25, this.audioCtx!.currentTime + delay);
        gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx!.currentTime + delay + 0.05);

        osc.connect(gain);
        gain.connect(this.audioCtx!.destination);

        osc.start(this.audioCtx!.currentTime + delay);
        osc.stop(this.audioCtx!.currentTime + delay + 0.05);
      });
    } catch {}
  }

  /**
   * Over completion chime.
   */
  public playOverChime(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      [523.25, 659.25].forEach((freq, idx) => {
        const osc = this.audioCtx!.createOscillator();
        const gain = this.audioCtx!.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(freq, this.audioCtx!.currentTime + idx * 0.12);
        gain.gain.setValueAtTime(this.masterVolume * 0.3, this.audioCtx!.currentTime + idx * 0.12);
        gain.gain.exponentialRampToValueAtTime(0.001, this.audioCtx!.currentTime + idx * 0.12 + 0.25);

        osc.connect(gain);
        gain.connect(this.audioCtx!.destination);

        osc.start(this.audioCtx!.currentTime + idx * 0.12);
        osc.stop(this.audioCtx!.currentTime + idx * 0.12 + 0.25);
      });
    } catch {}
  }

  /**
   * Harmonious triumphal fanfare for promotion and milestone achievements.
   */
  public playFanfare(): void {
    if (this.isMuted) return;
    try {
      this.initContext();
      if (!this.audioCtx) return;

      const notes = [261.63, 329.63, 392.00, 523.25]; // C - E - G - High C
      notes.forEach((freq, idx) => {
        const startTime = this.audioCtx!.currentTime + idx * 0.12;
        const osc = this.audioCtx!.createOscillator();
        const gain = this.audioCtx!.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(freq, startTime);

        gain.gain.setValueAtTime(this.masterVolume * 0.4, startTime);
        gain.gain.exponentialRampToValueAtTime(0.001, startTime + 0.35);

        osc.connect(gain);
        gain.connect(this.audioCtx!.destination);

        osc.start(startTime);
        osc.stop(startTime + 0.35);
      });
    } catch {}
  }
}
