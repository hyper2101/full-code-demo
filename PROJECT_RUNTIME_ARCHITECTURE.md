# 1. Project Identity & Future Direction

**Genre:** Sandbox Card Management / Survival Auto-Battler
**Gameplay Structure:** Players manage a settlement of mutant cats struggling against oppressive "Dog" factions. The game revolves around dragging and dropping cards to gather resources, craft equipment, manage stamina, and survive encounters.
**Inspirations:** Stacklands (base framework), Cult of the Lamb (ideological survival).

**Current Architectural Direction:** Moving away from Stacklands' "cozy automation" and infinite scaling. The new direction emphasizes scarcity, territorial pressure, individual character progression (cats with stamina/scars), and tactical turn-based combat over real-time automation.

**Intended Future Direction:**
The long-term direction, or "North Star", is:
- Smaller but deeper simulation.
- Stronger individual cat identity over generic pawns.
- Less infinite scaling, more economic and psychological pressure.
- Data-driven narrative and event authoring.
- Runtime-composable systems over deep inheritance trees.
- Modal tactical gameplay definitively replacing passive automation.

---

# 2. Emotional Design Direction

The intended emotional tone is:
- pressured
- unstable
- survival-focused
- psychologically oppressive
- scarcity-driven

The game should absolutely **avoid**:
- infinite comfort scaling
- passive idle optimization fantasies
- frictionless automation abundance

Players should feel:
- attachment to individual cats
- fear of loss
- economic tension
- moral compromise
- fragile survival stability

*All architectural choices should support this emotional direction (e.g., preventing easy automation logic).*

---

# 3. Architectural Vocabulary

To align terminology across development:
* **Authority:** The system currently allowed to mutate gameplay truth.
* **Modal System:** A temporary isolated runtime state that pauses the board and seizes authority.
* **Physical Card:** A board object used for interaction and spatial gameplay.
* **Data Object:** A non-physical runtime structure containing persistent gameplay data (e.g., `EquipmentInstance`).
* **Orchestration:** Systems coordinating interactions between authorities without owning deep state.
* **Simulation:** The autonomous progression of board systems over time.
* **Legacy:** Code inherited from Stacklands that is considered deprecated and dead.
* **Compatibility Layer:** Hooks existing solely to bridge new architecture (like CombatV2) with the physical board.

---

# 4. Core Runtime Philosophy

* **Card-as-Entity Architecture:** Everything in the game—cats, enemies, resources, food, and structures—is fundamentally a card. The physical representation on the board is the primary interface.
* **Board-First Gameplay:** The 2D board is the main interaction space. The board operates in real-time, but pauses (`WorldManager.WorldSimulationPaused`) when transitioning into deep sub-systems like combat or expeditions.
* **Mèo giữ Data, Bàn cờ giữ Object (Data Authority Principle):** To prevent reference leaks and board clutter, items like equipment are destroyed physically when equipped, transforming into deep-copied data objects (`EquipmentInstance`) inside the cat. When unequipped, they are instantiated back as physical cards.
* **Containers/Stacks Matter:** Stacking logic dictates interaction (e.g., Cat on Resource = Gather). The relationships (`Parent.SetChild`) define runtime state rather than loose variables.
* **Anti-Overengineering & Scarcity:** Infinite automation chains are intentionally removed to enforce micro-management and resource scarcity. The architecture rejects deep inheritance trees for generic workers in favor of specialized, data-driven encounters.

---

# 5. Architectural Priorities

When making technical tradeoffs, follow this **Priority Order**:
1. Runtime integrity
2. Gameplay clarity
3. Deterministic authority
4. Save/load stability
5. Solo developer maintainability
6. Content iteration speed
7. Performance optimization
8. Visual polish

---

# 6. Performance Philosophy

The project prioritizes:
- clarity over abstraction
- deterministic runtime behavior
- low cognitive load
- manageable solo-indie iteration speed

**Avoid:**
- premature optimization
- excessive generic systems
- deep abstraction layers
- ECS-style fragmentation unless clearly justified by extreme bottlenecks

*Simple, localized runtime logic is always preferred over enterprise-scale architecture.*

---

# 7. Failure Philosophy

The architecture prioritizes:
- deterministic reconstruction
- graceful degradation
- containment of corruption
- prevention of cascading authority failure

If corruption or fatal logic errors are detected:
- Invalid stack topology is ejected instantly.
- Modal authority is aborted safely and control returned to the board.
- Transient state may be discarded safely.
- Persistence integrity always takes priority over visual continuity.

*The game prefers losing temporary combat state over corrupting persistent board state.*

---

# 8. Content Authoring Philosophy

Gameplay content should increasingly become:
- TSV-driven
- data-authored
- event-composed
- runtime-configurable

**Hardcoded linear content is strongly discouraged.**

**Preferred:**
- declarative conditions
- reusable event templates
- modular reward pipelines
- composable encounter generation

---

# 9. Runtime Tick Philosophy

The game is **NOT** fully turn-based.
The game is **NOT** fully realtime.

The board operates as a hybrid:
* localized action timers
* independent card processing
* event-driven orchestration
* modal interruption

**Independent Processing:** Cards process contextual actions independently (e.g., gather, craft, cultivate, recover stamina).
**Global Clock:** The global simulation clock only governs day/month progression, stamina decay, scheduled threats, and systemic escalation.
**Modal Breaks:** Complex interactions transition into modal authority systems.

---

# 10. Runtime Layer Hierarchy

To prevent architectural corruption, the game is strictly structured into layers. Lower layers must never depend on higher layers.

**Layer 1 — Physical Runtime Layer**
* `GameCard`, `CardData`, `Containers`, Board interactions, Parent/Child hierarchy.
* *The physical building blocks of the board.*

**Layer 2 — Simulation Layer**
* Time progression, Resource processing, Stamina degradation, Cultivation (`SpiritField`).
* *The rules governing the physical blocks over time.*

**Layer 3 — Orchestration Layer**
* `NarrativeEventSystem`, `ThreatManager`, `ExpeditionManager`, Combat encounter setup.
* *The directors that inject events and encounters into the simulation.*

**Layer 4 — Modal Systems**
* `TurnBasedCombatManager`, Ritual resolution.
* *Systems that temporarily seize total authority from the board to resolve complex interactions.*

**Layer 5 — Persistence Layer**
* `SaveSystem`, Serialization, Runtime reconstruction.
* *Handles saving and loading. Never drives gameplay except during load reconstruction.*

**Layer 6 — Presentation Layer**
* UI panels, Tooltip systems, Visual effects, Animations.
* *Strictly read-only observers. UI never owns gameplay truth.*

---

# 11. Dependency Direction Rules

**Allowed Dependencies:**
* Presentation -> Orchestration
* Orchestration -> Simulation
* Simulation -> Physical Runtime

**Allowed strictly through Events (GameplayEventBus):**
* Simulation -> Presentation
* Combat -> UI
* Threat -> Narrative

**FORBIDDEN Dependencies:**
* UI -> SaveSystem authority mutation
* UI -> Direct combat calculations
* Legacy -> Active runtime ownership
* Combat -> Direct board mutation (use snapshots/events)
* Persistence -> Gameplay orchestration during runtime

---

# 12. Gameplay Modal State Model

The game does not run as a single monolithic loop. It operates through mutually exclusive runtime modes. During modal transitions, the simulation pauses and runtime authority transfers temporarily. Only one modal authority may mutate progression state at a time.

**Modes:**
* **Board Simulation Mode:** Real-time drag-and-drop, timers ticking. Authority: `WorldManager`.
* **Combat Mode:** Board pauses. Tactical resolution. Authority: `TurnBasedCombatManager`.
* **Expedition/Narrative Mode:** Board pauses. Text, choices, and mapping. Authority: `NarrativeEventSystem` / `ExpeditionManager`.
* **Reward Resolution Mode:** Board pauses. User selects loot. Authority: Reward UIs routing to storage.

*Example Transition Flow:*
`WorldManager` (Simulation) -> Encounter triggers -> `WorldManager` pauses -> `TurnBasedCombatManager` gains authority -> Combat resolves -> Reward pipeline executes -> Authority returns to `WorldManager`.

---

# 13. Board Integrity Rules

The board is the canonical interaction space.

**Rules:**
* A card may only have **one** Parent.
* Stack hierarchy must remain **acyclic**.
* Invalid stack relationships are auto-ejected.
* Destroyed cards must unregister from runtime registries immediately.
* Modal systems must not leave orphaned references when returning authority.
* Save reconstruction explicitly validates all stack topology.

*Board corruption is treated as a critical runtime failure.*

---

# 14. High-Level Runtime Flow

1. **Game Boot:** Managers initialize, localization loads via TSV.
2. **Board Initialization:** `WorldManager` and `SaveSystem` spawn cards and reconstruct physical stacks based on persistence data.
3. **Card Spawning & Interaction:** Cards are created and manipulated via user drag-and-drop. Interactions trigger localized action timers.
4. **Stacking/Container Interactions:** Parent/child links are formed. Cards process rules based on their stack context.
5. **System Updates:** Stamina degrades, resources yield, and the day/month cycle ticks forward.
6. **Narrative/Threat Escalation:** Systems like `DogTax` or `ThreatManager` evaluate triggers (e.g., high blasphemy in rituals) and spawn Debt Notes or Threat encounters directly onto the board.
7. **Combat Transition:** When an encounter triggers, the `PreCombatScreen` intercepts. The `TurnBasedCombatManager` takes over, freezing the main board. Players drag active cats into a 3x3 grid.
8. **Combat Resolution:** Automated turn-based combat executes via reaction chains. Winners receive drops generated via `LootProfile`.
9. **Reward Resolution:** Reward screens allow dragging cards into `Ordering Storage` (Insured vs Uninsured slots).
10. **Save/Load:** `SaveSystem` serializes the board's physical layout and internal data states.

---

# 15. Primary Gameplay Systems

### Cards
* **Purpose:** The universal physical entity.
* **Authority:** `WorldManager`, `CardData`, `GameCard`.
* **Important Flow:** Cards own their action timers. They transition from data definitions to physical board objects via `RuntimeCardRegistry`.

### Containers (Ordering Storage & Expedition Chests)
* **Purpose:** Manage off-board limits and loot extraction.
* **Authority:** Storage managers. Implements "Insured Slots" to prevent total wipeout upon retreat.

### Combat
* **Purpose:** Tactical resolution of conflict.
* **Authority:** `TurnBasedCombatManager`, `CombatEncounter`.
* **Important Models:** `EquipmentInstance` (stats), `MewtationsCombatStructs`.
* **Flow:** Pre-combat grid -> Turn initialization -> Stat resolution -> Reaction chains. Replaces Stacklands' realtime bump logic.

### Threat / Dog Tax
* **Purpose:** Economic pressure and punishment.
* **Authority:** `ThreatManager`.
* **Flow:** Generates `Debt Note` cards on the board that must be paid. Failure triggers retaliation encounters.

### Cultivation (Spirit Field & Cat God Mouth)
* **Purpose:** Resource conversion and ritual sacrifice.
* **Authority:** Internal `PlantRuntimeState` (for fields).
* **Flow:** Cat God Mouth uses a 2-slot system (Ritual + Sacrifice). Evaluates `Devotion` vs `Blasphemy` to yield `GodCatPackCard` or trigger `GodCatThreat`.

### Narrative & Events
* **Purpose:** Lore delivery and decision-making.
* **Authority:** `NarrativeEventSystem`, TSV Tables.
* **Flow:** Data-driven popups that yield conditional rewards or combat based on user choice.

### Knowledge & Codex System (Recipe Book)
* **Purpose:** Player-facing documentation of progression and craftable recipes without breaking immersion or relying on external wikis.
* **Authority:** `RecipeBookController`, `Blueprint` metadata.
* **Core Philosophy (Explanation vs. Simulation):** The UI layer explicitly separates the "Simulation Truth" from the "Explanation Truth". 
  * *Simulation Truth:* `RequiredCards` dictate physical stack merging and consumption during real-time board play.
  * *Explanation Truth:* Metadata like `RequiredStructures` and `WorkerRequirementType` are explicitly defined for the Codex to clearly explain to the player *how* a recipe works (e.g., requires a Cat Crafter near a Furnace) without polluting the consumption simulation logic.
* **Flow:** Scalable dynamic tabs populate based on unlocked knowledge in `SaveSystem`. Strict adherence to the `MewtationsLoc` pipeline ensures 100% localization without hardcoded strings.

---

# 16. Card Lifecycle

1. **Creation:** Defined as `CardData` prefabs.
2. **Spawning:** `WorldManager` or specialized systems spawn a `GameCard` onto the physical board.
3. **Runtime Registration:** Registered in `RuntimeCardRegistry`.
4. **Stacking/Attachment:** User drags the card. It sets its `Parent` and `Child` references, initiating contextual action loops.
5. **Data Transformation:** If it's equipment, it is destroyed upon equip and copied as an `EquipmentInstance` into the character's data block.
6. **Combat Usage:** If it's a character, it is serialized into the 3x3 grid, engaging in turn-based logic.
7. **Destruction/Revival:** Exhausted cards become Paralyzed/Corpses. They can be revived at the Dog Hospital (creating debt). Otherwise, they are completely destroyed (`Destroy(gameObject)`).
8. **Save/Load Reconstruction:** The unique ID, position, stack hierarchy, and inner data (like stamina or stored equipment) are serialized.

---

# 17. Runtime Authority Map

* **`WorldManager` (State Owner):** The absolute source of truth for the physical board, time progression, and card presence.
* **`TurnBasedCombatManager` (Transient Owner):** Seizes authority during encounters. The board is paused. It has total ownership over combat calculations and stat evaluation.
* **`SaveSystem` (Persistence Authority):** Overrides `WorldManager` during boot to enforce saved state.
* **`EquipmentInstance` (Data Authority):** Owns equipment stats. Physical gear cards DO NOT own their stats once equipped—they are visual/interaction wrappers.
* **UI Panels (Visual Observers):** `CharacterPanelUI`, `PreCombatScreen`, etc., are strictly read-only observers. They trigger commands but do not store authoritative state.
* **Legacy Managers (Deprecated Authorities):** `CitiesManager`, `QuestManager`, `WeatherManager` still exist but have been stripped of runtime authority via execution flags. They must never be reactivated as truth sources.

---

# 18. Event Flow Architecture

* **GameplayEventBus:** The central nervous system for decoupled events (e.g., OnDayEnded, OnCombatStarted).
* **Update Flow:** Most logic is driven by Unity's `Update` ticking localized action timers on `CardData`, rather than a monolithic loop.
* **Combat Pipeline:** Highly procedural. Initiative calculation -> Target resolution -> Skill execution -> Consequence application (Damage/Debuff) -> Cleanup.
* **Narrative Pipeline:** Trigger -> Load TSV data -> Pause Board -> Present UI -> Apply Consequence (Loot/Combat) -> Unpause.
* **Direct Ownership vs Observers:** Systems directly manipulate their owned data but broadcast events for UI or achievements to passively listen to.

---

# 19. Data Lifetime Categories

**Persistent Data**
* Cat progression, `EquipmentInstance`s, Debt state, Scar data, World progression.
* *Lives on disk. Reconstructed on load.*

**Session Data**
* Current board physical layout, Temporary buffs, Runtime stack topology.
* *Serialized alongside Persistent Data, but highly volatile in-game.*

**Transient Modal Data**
* Combat snapshots, Reward selection state, Expedition routing state.
* *Lives ONLY during the modal interruption. Destroyed upon return to simulation. Not serialized.*

**Visual Data**
* Animations, Hover state, UI selections, Tooltip caches.
* *Strictly presentation-only. Recomputed on the fly. Not serialized.*

*Only Persistent and Session Data are serialized.*

---

# 20. Gameplay Data Flow

* **State Movement:** Physical cards interact -> Action timer completes -> Consequence generated.
* **Data Transformation:** Physical Equipment Card -> Dragged onto UI -> Card Destroyed -> `EquipmentInstance` injected into Cat -> Cat un-equips -> `EquipmentInstance` purged -> New Physical Card spawned.
* **Communication:** Components communicate via direct reference for high-frequency actions (stacks) and via `GameplayEventBus` for system-wide milestones.
* **State Location:** 
  * *Board State* lives in `WorldManager` and physical `GameCard` hierarchies.
  * *Transient State* lives in `TurnBasedCombatManager` during fights.
  * *Persistent State* lives in `SaveSystem` data structures waiting for disk IO.

---

# 21. Save/Load Philosophy

* **Serialization Boundaries:** The system saves the logical ID, position, and specific volatile inner data (e.g., stamina levels, deep-copied equipment) of cards.
* **Reconstruction Flow:** Read JSON -> Instantiate base prefabs -> Apply saved volatile data -> Reconstruct Parent/Child stack links -> Validate logic (e.g., ejecting invalid children).
* **Authority Restoration:** Upon loading, `SaveSystem` dictates truth. If a card exists in the save but not the registry, it is forced in. If the save file structure breaks, the `BoardIntegrityValidator` drops corrupted sub-stacks.
* **Temporary Data:** Combat snapshots and active expedition paths are generally volatile. If a crash occurs, combat is either reset or considered a retreat based on context.

---

# 22. Legacy Architecture & Current Technical Debt

**Active Replacements:** `TurnBasedCombatManager` replaces the old bump-combat. `EquipmentInstance` replaces physical cards for equipped gear. `NarrativeEventSystem` (TSV-driven) is replacing hardcoded linear quests. `MewtationsLocTable` replaces old localization.

**SAFE TO EXPAND:** 
* `TurnBasedCombatManager` and `CombatV2` systems.
* Cat-specific systems (Stamina, Equipments as Instances).
* TSV-driven Localization and Narrative systems.

**Current Technical Debt (IN TRANSITION):** 
* `WorldManager.cs` is still overloaded with legacy Stacklands orchestration.
* `Cards.cs` contains mixed legacy constants and active runtime logic.
* Some save/load flows still assume old Stacklands hierarchy behavior.
* Combat bridging through `Combatable.cs` still exists purely for compatibility.
* Certain UI flows still depend on legacy `DialogueSystem.cs` popup assumptions.

**COMPATIBILITY ONLY:**
* Base `Combatable` hooks used only to bridge physical cards into the new Turn-Based system.

**LEGACY (DO NOT USE/EXTEND):**
* `GameScripts/Legacy/Stacklands/Automation/` (Conveyors, Batteries).
* `GameScripts/Legacy/Stacklands/Cities/` (Happiness, Demands).
* `GameScripts/Legacy/Stacklands/Quests/` (Linear achievement checklists).
* `Worker.cs`, `Villager.cs` hierarchies.

---

# 23. Forbidden Architectural Patterns

**WHAT MUST NEVER HAPPEN:**
* NEVER store authoritative gameplay state in UI classes.
* NEVER extend legacy `Villager` or `Worker` automation systems.
* NEVER reintroduce infinite passive automation loops (Factorio-style).
* NEVER make equipment cards own equipped stats while equipped (use `EquipmentInstance`).
* NEVER create hidden singleton state outside `SaveSystem` authority.
* NEVER let combat directly mutate live board cards during encounters (use snapshots/events).
* NEVER couple Narrative logic directly into UI classes.
* NEVER use static globals for runtime gameplay state.

---

# 24. Feature Integration Rules

When adding a new gameplay feature, adhere to the following constitution:

1. **Prefer dedicated System managers.** Do not bloat `WorldManager` further.
2. **Keep UI strictly read-only.**
3. **Use `GameplayEventBus`** for cross-system communication rather than tight coupling.
4. **Keep save data reconstruction deterministic.**
5. **Prefer runtime composition** (components/data blocks) over deep inheritance trees.
6. **Avoid adding new permanent singleton authorities.**
7. **Treat physical cards as interaction surfaces,** not deep data containers. Offload complex data to specific state objects.
8. **Respect the modal architecture.** Pause the board if you need isolated authority.

---

# 25. AI Reading Guide

When touching this codebase, follow this reading order to avoid Stacklands traps:

1. **Read `gdd.md` & `legacy_cleanup.md` first.** They define what this game *is* and what it *is not*.
2. **Review the Architectural Hierarchy & Philosophy (Sections 2-11).** Understand the emotional tension, the modal nature of the game, and how failures are treated.
3. **Read `WorldManager.cs`** to understand how the board ticks, but ignore any code wrapped in `LegacyRuntimeFlags`.
4. **Read `GameScripts/Combat/Core/TurnBasedCombatManager.cs`** to understand the true combat authority. DO NOT read old bump-combat logic in `Combatable.cs`.
5. **Read Cat Equipment logic.** Understand `EquipmentInstance` to grasp the "Data vs Object" philosophy.
6. **Beware of Traps:** Do not extend `Villager` or `Worker`. Do not hook into `CitiesManager` or `QuestManager`. They are dead systems. Use `NarrativeEventSystem` for story and Threat managers for pressure.
7. **Orchestration:** Managers in `Systems/` orchestrate logic. If adding a new feature, decouple it into a new Manager rather than bloating `WorldManager` further.

# 12. Economy Architecture

**12.1. Denomination Authority**
- Economy Core relies on ICurrency, CurrencyTier, and CurrencyUtility to strictly enforce the denomination hierarchy.
- Cards opt into the economy via virtual CanBeSold, SellTier, and SellValue properties. Defaults are strictly alse and   to prevent accidental wealth generation.

**12.2. Debt & Hook Systems**
- The DebtNotice in Active Collection State relies on the OnCurrencySpawned event hook rather than continuous board scanning. This adheres to the Orchestration vs. Simulation philosophy.
- Forced Liquidation behavior completely ignores standard CurrencyUtility denomination rules and consumes raw value, ensuring mechanical asymmetry between player rules and institutional punishment.
