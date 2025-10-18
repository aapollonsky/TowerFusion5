# TowerFusion5 🏰⚔️

A Unity 2D tower defense game featuring intelligent enemy AI, tower traits, and dynamic wave generation.

## 🎮 Game Features

- **Tower Defense Mechanics**: Strategic tower placement to defend against enemy waves
- **Enemy Attack System**: Enemies intelligently attack towers before reaching the end point
- **Tower Traits**: Unique abilities and projectile types for different tower configurations
- **Dynamic Waves**: Automatic wave generation with balanced difficulty progression
- **Target Distribution**: Smart enemy AI distributes attacks across multiple towers (max 3 simultaneous)

## 🚀 Quick Start

1. **Open Project**: Launch Unity and open this project
2. **Check Setup**: See [`docs/UNITY_SETUP.md`](docs/UNITY_SETUP.md) for environment configuration
3. **Read Quick Start**: Follow [`docs/QUICKSTART.md`](docs/QUICKSTART.md) to get started
4. **Build**: See [`docs/BUILD.md`](docs/BUILD.md) for deployment instructions

## 📚 Documentation

All documentation is located in the **[`docs/`](docs/)** folder:

- **[📑 Documentation Index](docs/INDEX.md)** - Complete documentation navigation
- **[📖 Main README](docs/README.md)** - Project overview and architecture
- **[🎯 Quick Start](docs/QUICKSTART.md)** - Get started quickly

### Key Documentation

#### Core Systems
- [Enemy Tower Attack System](docs/ENEMY_TOWER_ATTACK_SYSTEM.md)
- [Enemy Target Distribution](docs/ENEMY_TARGET_DISTRIBUTION.md)
- [Tower Traits System](docs/TOWER_TRAITS.md)
- [Auto Wave Generation](docs/AUTO_WAVE_GENERATION.md)

#### Features
- [Max Tower Constraint](docs/MAX_TOWER_CONSTRAINT.md) - 3-tower simultaneous attack limit
- [Trait Projectile System](docs/TRAIT_PROJECTILE_SYSTEM.md)
- [Impact Effects](docs/IMPACT_EFFECTS.md)

#### Troubleshooting
- [Troubleshooting Earth Disk](docs/TROUBLESHOOTING_EARTH_DISK.md)
- [Troubleshooting Impact Effects](docs/TROUBLESHOOTING_IMPACT_EFFECTS.md)
- [Troubleshooting Trait Buttons](docs/TROUBLESHOOTING_TRAIT_BUTTON.md)

## 🏗️ Project Structure

```
TowerFusion5/
├── Assets/
│   ├── Scenes/                 # Unity scenes
│   ├── Scripts/                # C# game scripts
│   │   ├── Enemy/              # Enemy AI and behavior
│   │   ├── Tower/              # Tower systems and traits
│   │   ├── Projectile/         # Projectile types and effects
│   │   └── Managers/           # Game managers (singleton pattern)
│   ├── Data/                   # ScriptableObject assets
│   │   ├── Enemies/            # Enemy configurations
│   │   ├── Towers/             # Tower configurations
│   │   └── Waves/              # Wave configurations
│   ├── Prefabs/                # Game object prefabs
│   └── Sprites/                # 2D sprites and art
├── docs/                       # 📚 All documentation
└── .github/
    └── copilot-instructions.md # GitHub Copilot context
```

## 🎯 Key Systems

### Enemy Target Distribution
- Each enemy attacks **one tower at a time**
- Different enemies spread across **different towers** intelligently
- **Maximum 3 towers** can be under attack simultaneously per wave
- Algorithm selects tower with fewest enemies (closest if tied)

### Tower Traits
- Modular trait system for tower customization
- Different projectile types (standard, explosive, piercing, etc.)
- Visual effects and impact animations
- Earth trait with ground disk mechanics

### Wave Generation
- Automatic difficulty scaling
- Balanced enemy type distribution
- Progressive challenge increase

## 🛠️ Technologies

- **Unity 2022.3.12f1** (or later)
- **C# .NET** scripting
- **ScriptableObject** architecture pattern
- **Singleton** manager pattern
- **Event-driven** communication

## 📄 License

This project is for educational and development purposes.

## 🤝 Contributing

See [`docs/DEVELOPMENT.md`](docs/DEVELOPMENT.md) for development guidelines and contribution workflow.

---

**For complete documentation, visit the [docs folder](docs/) or start with the [Documentation Index](docs/INDEX.md).**
