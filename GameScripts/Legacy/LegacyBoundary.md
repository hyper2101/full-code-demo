# Legacy Boundary Rules

## Purpose
This directory acts as an architectural firewall. It serves as a temporary compatibility bridge during the Stacklands DNA purge.

## Core Rules

The Modern Runtime (e.g. `WorldManager`, `TurnBasedCombatManager`, `EnvironmentalContext`) **MUST NOT** directly reference legacy systems.

### 🚫 FORBIDDEN REFERENCES:
- `QuestManager`
- `CitiesManager`
- `DemandManager`
- `WellbeingSystem`
- `AllQuests`
- Old BoardTransition logic (hardcoded islands/greed/death)

### ✅ ALLOWED USAGE:
All legacy access **must pass through** the Hook classes defined in `GameScripts/Legacy/Hooks/`.

- `LegacyCitiesHooks`
- `LegacyQuestHooks`
- `LegacyBoardTransitionHooks`

## Hook Implementation Rules
1. **Hooks MUST be thin**: They are delegation, translation, or guard layers.
2. **NO business logic**: Do not write actual wellbeing calculations or quest logic inside a hook. If a legacy system needs to do work, delegate to it. If the feature flag is off, return early.
3. **Stateless**: Hooks must not cache state, have update loops, or hold long-living references. They are purely gateways.

## Future Plans
When the project matures enough, these Hooks and the underlying systems they wrap will be completely purged from the codebase. Do not build new features relying on them.
