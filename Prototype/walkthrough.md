# Walkthrough: Offline-First Cricket Career Game (Step 1 Foundation)

We have implemented the complete foundational architecture for an offline-first single-player cricket career game focused on the individual journey of ONE player, starting from Under-16 grassroots and progressing up to the ODI World Cup.

---

## 1. Project Architecture & Folder Structure

The project has been organized into clean, decoupled domain modules:

```
c:/Users/munee/OneDrive/Desktop/My Projects/Cricket game/
├── package.json               # Vite + TypeScript + Vitest setup
├── tsconfig.json              # Strict ES2022 configuration
├── vite.config.ts             # Fast bundling configuration
├── index.html                 # Game entry HTML with Teko/Inter athletic styling
├── src/
│   ├── core/                  # EventBus, GameStateManager, NetworkInterface
│   │   ├── EventBus.ts
│   │   ├── GameState.ts
│   │   └── NetworkInterface.ts # Pre-wired for future multiplayer sync
│   ├── player/                # Player models, attributes, factory & ratings
│   │   ├── PlayerModel.ts
│   │   ├── PlayerFactory.ts
│   │   ├── RatingCalculator.ts
│   │   └── CareerStatistics.ts
│   ├── career/                # Career levels, status, progression & selection logic
│   │   ├── CareerLevel.ts
│   │   ├── CareerStatus.ts
│   │   ├── CareerProgression.ts
│   │   ├── SelectionManager.ts
│   │   └── PerformanceEvaluator.ts
│   ├── cricket/               # Rules, formats, dismissals, commentary engine
│   │   ├── CricketTypes.ts
│   │   └── CommentaryEngine.ts
│   ├── tournament/            # Tournament data models & manager
│   │   ├── TournamentModel.ts
│   │   └── TournamentManager.ts
│   ├── match/                 # Match simulation, scorecards, player turn resolution
│   │   ├── MatchModel.ts
│   │   └── MatchSimulator.ts
│   ├── save/                  # Robust offline save/load system with schema versioning
│   │   ├── SaveSystem.ts
│   │   └── SaveSchema.ts
│   ├── audio/                 # Web Audio synthesizer for offline sound effects
│   │   └── AudioManager.ts
│   ├── data/                  # Tournaments, teams, and venues databases
│   │   ├── tournaments.data.ts
│   │   ├── teams.data.ts
│   │   └── venues.data.ts
│   ├── utilities/             # Seeded PRNG, formatters, stat helpers
│   │   ├── SeededRNG.ts
│   │   └── Formatters.ts
│   ├── ui/                    # UI screens & components
│   │   ├── UIManager.ts
│   │   ├── components/
│   │   │   ├── NavigationBar.ts
│   │   │   └── DebugPanel.ts
│   │   └── screens/
│   │       ├── MainMenuScreen.ts
│   │       ├── PlayerCreationScreen.ts
│   │       ├── CareerHubScreen.ts
│   │       ├── TournamentScreen.ts
│   │       ├── MatchPrepScreen.ts
│   │       ├── MatchScreen.ts
│   │       ├── MatchResultScreen.ts
│   │       ├── CareerProgressionScreen.ts
│   │       ├── StatisticsScreen.ts
│   │       └── SettingsScreen.ts
│   ├── assets/
│   │   └── styles.css         # Dark stadium emerald UI theme
│   └── main.ts                # Application lifecycle & event orchestrator
└── tests/
    ├── player.test.ts
    ├── career.test.ts
    ├── tournament.test.ts
    ├── match.test.ts
    ├── saveload.test.ts
    └── career_loop_integration.test.ts
```

---

## 2. Key Subsystems Built

### Player Model & Ratings (`src/player/`)
- **Attributes**: Identity (ID, first/last name, age, nationality, jersey number), Role (`Batsman`, `Bowler`, `All-rounder`, `Wicketkeeper`), Batting (ability, technique, timing, power, running), Bowling (ability, pace, swing, seam, spin, accuracy, variation), Fielding (fielding, catching, throwing, reflexes), Mental (confidence, fitness, form, pressure handling), Potential & Overall rating.
- **Dynamic Rating Calculation**: Evaluates role-weighted attributes (Batsman: batting + composure; Bowler: bowling + accuracy + pace/spin; All-Rounder: 40/40/10/10 split).
- **Starting Under-16 Archetypes**: Generates realistic 15-year-old talents with overall ratings of 45–52 and high potential (75–95).

### Career Progression & Selection (`src/career/`)
- **9 Sequential Career Tiers**:
  1. `UNDER_16` (Under-16 Youth Cup)
  2. `UNDER_19` (Under-19 National Championship)
  3. `DOMESTIC` (State / Provincial List-A / First-Class)
  4. `COUNTRY_LEAGUE` (County & Franchise T20 League)
  5. `INTERNATIONAL_HOME` (Home Bilateral Series)
  6. `INTERNATIONAL_AWAY` (Overseas Away Tour)
  7. `TEST` (World Test Championship)
  8. `T20_WORLD_CUP` (ICC T20 World Cup)
  9. `ODI_WORLD_CUP` (ICC Cricket World Cup)
- **Performance-Driven Promotion**: Checks actual matches played, runs/average or wickets/economy, minimum overall rating, and current form.
- **Selection Manager**: Dynamically determines status (`Selected`, `Playing`, `Substitute`, `Dropped`, `Injured`) based on current form, fitness, confidence, and slump detection.

### Match Simulator & Performance Evaluation (`src/match/`)
- Simulates realistic 40-over, 50-over, T20, and Multi-Day cricket matches.
- Probability engine resolves deliveries based on player technique, timing, bowling accuracy, and match state.
- Tracks user's personal scorecard (runs, balls, 4s, 6s, dismissal text, overs, maidens, runs conceded, wickets, catches, run-outs).
- Evaluates individual match performance on a 1.0–10.0 scale, generating dynamic form changes, confidence updates, stamina drain, and XP.

### Career Statistics Tracker (`src/player/CareerStatistics.ts`)
- Tracks:
  - **Batting**: Matches, innings, runs, highest score, batting average, strike rate, 50s, 100s, 4s, 6s, ducks, not-outs.
  - **Bowling**: Matches, overs, maidens, runs conceded, wickets, best bowling, average, economy, 3-fors, 5-fors.
  - **Fielding**: Catches, run outs, stumpings.
- Preserves all historical numbers across:
  - All-time totals.
  - Level-by-level breakdown.
  - Tournament-by-tournament records.

### Offline-First Save / Load System (`src/save/`)
- Versioned schema (`SaveDataV1`).
- Methods: `saveGame()`, `loadGame()`, `deleteSave()`, `saveExists()`, `listSaves()`.
- Stores player profile, attributes, career level, tournament, statistics, match history, and game settings.
- Memory fallback for automated test environments and IndexedDB/LocalStorage persistence for browsers.

### Web Audio API Synthesizer (`src/audio/`)
- 100% offline audio synthesis: bat-ball willow strike, rattling stumps, milestone fanfare, and UI clicks without external asset downloads.

### Future Multiplayer Abstraction (`src/core/NetworkInterface.ts`)
- Pre-wired `INetworkService` interface with `OfflineNetworkService` default implementation, allowing Phase 2 online multiplayer lobbies and match synchronization to plug in seamlessly.

---

## 3. Test & Verification Results

### Automated Vitest Suite
All 17 tests across 6 test suites passed:

```
 ✓ tests/player.test.ts (3 tests)
 ✓ tests/tournament.test.ts (3 tests)
 ✓ tests/career.test.ts (6 tests)
 ✓ tests/match.test.ts (2 tests)
 ✓ tests/saveload.test.ts (2 tests)
 ✓ tests/career_loop_integration.test.ts (1 test)

 Test Files  6 passed (6)
      Tests  17 passed (17)
```

### Production Build
```
> cricket-career-game@0.1.0 build
> tsc && vite build

✓ 40 modules transformed.
dist/index.html                   0.85 kB │ gzip:  0.49 kB
dist/assets/index-9srGKI2P.css    7.32 kB │ gzip:  2.12 kB
dist/assets/index-BrhjvilI.js   135.10 kB │ gzip: 30.64 kB │ map: 294.68 kB
✓ built in 865ms
```
Zero errors, zero warnings.
