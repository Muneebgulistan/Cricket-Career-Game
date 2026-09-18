# Cricket Career Game Foundation

A single-player, offline-first cricket career game foundation featuring complete player progression, tournaments, performance evaluation, statistics, save/load, and match simulation systems.

> **Long-Term Target**: This repository serves as the core logic and career architecture foundation for a future 3D mobile cricket game built with Unity.

---

## Features

- **Player Creation & Progression**: Full player attribute modeling, traits, growth curves, fatigue/fitness, and career trajectory from Under-16 to International World Cups.
- **Career Systems**: Dynamic calendar, team selection manager, performance evaluator, contract management, and milestone tracking.
- **Tournament Architecture**: Multi-tier tournament systems with league tables, group stages, knockouts, fixtures, and NRR (Net Run Rate) calculation.
- **Match Simulation & Interactive Gameplay**:
  - 2.5D Canvas stadium match view at 60 FPS.
  - Deterministic 3D ball physics (trajectory, aerodynamic swing, pitch seam, bounce restitution, and spin turn).
  - Directional batting controls (Left, Center, Right) across 10 authentic cricket shots.
  - Bowling control with pace effort, line/length selection, variation presets, and an interactive accuracy/power meter.
  - Running between wickets decisions (Safe Stay vs Risky Extra Run) and reflex fielding/throwing system.
  - Contextual AI opponents adapted for T20 Powerplays, Required Run Rate pressure, and death overs.
- **Broadcast Experience**: Full television scoreboard overlay, dynamic ball-by-ball commentary engine, match milestones, and procedural Web Audio synthesized soundscape.
- **Offline Persistence**: Mid-match safe state serialization and local storage persistence.

---

## Tech Stack (Current Prototype)

- **Language**: TypeScript
- **Bundler**: Vite
- **Testing**: Vitest (62 unit & integration tests)
- **Audio**: Web Audio API (Synthesized)
- **Rendering**: HTML5 Canvas (60 FPS 2.5D field view)

---

## Getting Started

### Prerequisites

- Node.js (v18+)
- npm

### Installation

```bash
npm install
```

### Running Locally

```bash
npm run dev
```

Visit `http://localhost:5173` in your browser to launch the game.

### Running Tests

```bash
npm test
```

### Production Build

```bash
npm run build
```
