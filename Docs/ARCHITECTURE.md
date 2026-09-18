# Cricket Career Game — Technical Architecture & Migration Reference

This document defines the architectural blueprint for the transition from the TypeScript/Vite career/data prototype to the official **Unity 3D mobile cricket game**.

---

## 1. Project Philosophy & Vision

The goal is to build an offline-first, professional 3D mobile cricket career game inspired by the presentation depth and career immersion of premier cricket titles (e.g., Real Cricket), built with our own original C# systems, shaders, physics, and gameplay architecture.

- **Platform**: Android first (touch-optimized, scalable resolutions), with iOS compatibility built-in.
- **Architecture**: Decoupled, event-driven C# subsystems with strong data-oriented state separation.
- **Offline-First**: All career progress, tournament tables, match results, and player growth persist locally via JSON local file storage.

---

## 2. Directory Structure

```
Cricket-Career-Game/
│
├── UnityGame/                          # Official Unity 3D Project
│   ├── Assets/
│   │   ├── _Game/                      # Core C# Game Architecture
│   │   │   ├── Core/                   # Bootstrap, GameState, SceneController
│   │   │   ├── Gameplay/               # MatchManager, Over/Innings Lifecycle
│   │   │   ├── Cricket/                # CricketEnums, MatchResult, Rules
│   │   │   ├── Career/                 # CareerProfile, Progression, Statistics
│   │   │   ├── Players/                # PlayerProfile, Ratings, Condition
│   │   │   ├── Teams/                  # TeamData, Squads, Rosters
│   │   │   ├── Tournaments/            # TournamentProgress, Fixtures, Tables
│   │   │   ├── Stadiums/               # Stadium Configuration & Pitch Data
│   │   │   ├── Ball/                   # 3D Ball Physics, Swing, Seam & Spin
│   │   │   ├── Batting/                # Batting Mechanics, Timing & Shots
│   │   │   ├── Bowling/                # Bowling Controls, Accuracy & Power Meter
│   │   │   ├── Fielding/               # Fielding Positioning, Throw & Catch Logic
│   │   │   ├── AI/                     # Tactical AI Batting & Bowling
│   │   │   ├── Camera/                 # Dynamic Broadcast Camera System
│   │   │   ├── UI/                     # MainMenu, CareerHub, Match Scoreboard
│   │   │   ├── Audio/                  # AudioManager (BGM, SFX, Crowds)
│   │   │   ├── Animation/              # Character Rigging & Animation Controllers
│   │   │   ├── SaveSystem/             # Offline JSON Local Persistence
│   │   │   ├── Data/                   # ScriptableObject Data Containers
│   │   │   ├── Input/                  # Mobile Touch & Gesture Handlers
│   │   │   └── Utilities/              # Formatting, Math, Seeded RNG
│   │   │
│   │   ├── Scenes/                     # Bootstrap, MainMenu, CareerHub, Training, Match
│   │   ├── Prefabs/                    # UI, Player, Ball, Pitch Prefabs
│   │   ├── Materials/                  # Pitch, Turf, Stumps, Kit Shaders
│   │   ├── Models/                     # 3D Meshes
│   │   ├── Textures/                   # Pitch, Ball, Stadium Textures
│   │   ├── Animations/                 # Batting, Bowling, Fielding Clips
│   │   ├── Audio/                      # Audio Clips & Sound Effects
│   │   └── Resources/                  # Dynamic Runtime Resources
│   │
│   ├── Packages/                       # Package Manifest (UGUI, Physics, Audio)
│   └── ProjectSettings/                # Project Settings (Android, Resolution, Quality)
│
├── Prototype/                          # Complete TypeScript/Vite Prototype (Reference)
│   ├── src/                            # Tested reference algorithms
│   └── tests/                          # 62 unit & integration tests
│
├── Docs/                               # Architecture and Design Specs
│   └── ARCHITECTURE.md
│
└── README.md
```

---

## 3. Prototype to Unity Mapping

| TypeScript Prototype System | Unity C# Equivalent | Responsibility |
| :--- | :--- | :--- |
| `src/core/GameState.ts` | `_Game/Core/GameStateManager.cs` | Finite State Machine managing high-level application states (`Booting`, `MainMenu`, `CareerHub`, `PlayingMatch`, etc.). |
| `src/core/EventBus.ts` | `_Game/Core/GameEvents.cs` | Decoupled C# action events for match milestones, state changes, and UI cues. |
| `src/player/PlayerModel.ts` | `_Game/Players/PlayerProfile.cs` | Player identity, batting/bowling styles, ratings, and physical/mental condition. |
| `src/career/CareerProgression.cs` | `_Game/Career/CareerProgression.cs` | 9-tier ladder (Under-16 to ODI World Cup) with performance promotion logic. |
| `src/career/CareerStatistics.cs` | `_Game/Career/CareerStatistics.cs` | Comprehensive all-time, level-by-level, and tournament statistics containers. |
| `src/save/SaveSystem.ts` | `_Game/SaveSystem/SaveManager.cs` | Offline JSON serialization to `Application.persistentDataPath`. |
| `src/match/BallPhysics.ts` | `_Game/Ball/BallPhysics.cs` | 3D trajectory calculation with aerodynamic swing, seam deviation, and pitch friction. |
| `src/match/BattingEngine.ts` | `_Game/Batting/BattingController.cs` | Shot direction matching, timing sweet spots, and dismissal matrix. |
| `src/match/BowlingEngine.ts` | `_Game/Bowling/BowlingController.cs` | Pitch line/length aiming, pace effort, and accuracy release meter. |
| `src/match/AIEngine.ts` | `_Game/AI/CricketAI.cs` | Opponent tactical decision trees (Powerplays, RRR chasing, collapse defense). |

---

## 4. Career Progression Ladder

The 9 progression levels are:

1. **Under-16 Cup**: Grassroots talent tournament.
2. **Under-19 Cup**: National youth championship.
3. **Domestic Cricket**: State/provincial division.
4. **Country / Regional League**: Premier franchise T20 league.
5. **Home Series**: Bilateral international series on home turf.
6. **Away Series**: Challenging overseas tour in foreign conditions.
7. **Test Series**: Traditional 5-day red-ball format.
8. **T20 World Cup**: Pinnacle global 20-over tournament.
9. **ODI World Cup**: Ultimate global 50-over championship.

Promotion is strictly meritocratic based on player performance ratings, milestone achievements, and fitness maintenance.
