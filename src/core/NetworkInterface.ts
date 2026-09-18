/**
 * Network abstraction interface designed to accommodate future online multiplayer,
 * cloud sync, matchmaking, and leaderboards without altering offline core logic.
 */

export interface NetworkPlayerProfile {
  id: string;
  name: string;
  rating: number;
  level: string;
  isHost: boolean;
}

export interface MatchSyncPacket {
  matchId: string;
  ballNumber: number;
  overNumber: number;
  event: string;
  scoreA: number;
  scoreB: number;
  timestamp: number;
}

export interface INetworkService {
  isConnected(): boolean;
  connect(serverUrl?: string): Promise<boolean>;
  disconnect(): void;
  createLobby(lobbyName: string): Promise<string>;
  joinLobby(lobbyCode: string): Promise<boolean>;
  sendMatchEvent(packet: MatchSyncPacket): void;
  onMatchEvent(handler: (packet: MatchSyncPacket) => void): void;
}

/**
 * Default offline implementation. Ensures all game hooks function
 * without external network requirements.
 */
export class OfflineNetworkService implements INetworkService {
  private static instance: OfflineNetworkService;

  public static getInstance(): OfflineNetworkService {
    if (!OfflineNetworkService.instance) {
      OfflineNetworkService.instance = new OfflineNetworkService();
    }
    return OfflineNetworkService.instance;
  }

  public isConnected(): boolean {
    return false; // Offline-first
  }

  public async connect(): Promise<boolean> {
    console.log('[Network] Running in Offline mode (Offline-first architecture).');
    return false;
  }

  public disconnect(): void {
    // No-op for offline
  }

  public async createLobby(): Promise<string> {
    throw new Error('Multiplayer lobbies are reserved for Phase 2 online rollout.');
  }

  public async joinLobby(): Promise<boolean> {
    throw new Error('Multiplayer lobbies are reserved for Phase 2 online rollout.');
  }

  public sendMatchEvent(): void {
    // In offline mode, match events are consumed entirely locally by MatchSimulator
  }

  public onMatchEvent(): void {
    // Local match simulation handler
  }
}
