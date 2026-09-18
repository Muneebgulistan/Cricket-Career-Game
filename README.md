# Cricket Career Game

A professional 3D mobile cricket career simulation featuring authentic player progression, tournaments, performance evaluation, statistics, and offline persistence.

---

## Project Overview

- **Project Name**: Cricket Career Game
- **Genre**: 3D Cricket Career Simulation
- **Platform**: Android / iOS
- **Engine**: Unity (3D Mobile)
- **Primary Mode**: Offline Career
- **Future Roadmap**: Online Multiplayer

---

## Career Progression Ladder

The single-player career follows a meritocratic progression pathway:

1. **Under-16 Cup**
2. **Under-19 Cup**
3. **Domestic Cricket**
4. **Country / Regional League**
5. **International Home Series**
6. **International Away Series**
7. **Test Match Series**
8. **T20 World Cup**
9. **ODI World Cup**

*Note: Promotion to higher tiers depends strictly on player performance, match ratings, milestone accomplishments, and physical condition.*

---

## Repository Structure

```
Cricket-Career-Game/
│
├── UnityGame/                          # Official Unity 3D mobile game project
│   ├── Assets/
│   │   ├── _Game/                      # Core C# Architecture (Core, Career, UI, Gameplay)
│   │   ├── Scenes/                     # Bootstrap, MainMenu, CareerHub, Training, Match
│   │   ├── Prefabs/
│   │   ├── Materials/
│   │   ├── Models/
│   │   ├── Textures/
│   │   ├── Animations/
│   │   ├── Audio/
│   │   └── Resources/
│   ├── Packages/                       # Package Manager manifest
│   └── ProjectSettings/                # Project & platform settings (Android / iOS)
│
├── Prototype/                          # Complete TypeScript/Vite prototype (Design Reference)
│   ├── src/                            # Tested game logic & simulation algorithms
│   └── tests/                          # 62 passing unit & integration tests
│
├── Docs/                               # Technical specifications & architecture blueprints
│   └── ARCHITECTURE.md
│
├── .gitignore                          # Official Unity + Web ignore rules
└── README.md
```

---

## Prototype Verification

The design reference prototype in `Prototype/` can be tested and run with:

```bash
cd Prototype
npm install
npm test
npm run dev
```

---

## Disclaimer

This is an independent, original project developed from the ground up with custom C# systems, architecture, and logic. It is **not affiliated with, endorsed by, or associated with Real Cricket, Nautilus Mobile, or any other commercial cricket game**.
