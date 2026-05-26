# Walkthrough — Mewtations Lore & Dialogue System

## Overview

This walkthrough documents all code changes introduced in **commit `7026047` (23 May 2026)** for the Mewtations mod. The update adds a full lore discovery system, bilingual dialogue UI, and recipe-gating mechanics.

---

## New Files

### [`GameScripts/Cards/Data/SecretLoreHintCardData.cs`](GameScripts/Cards/Data/SecretLoreHintCardData.cs)

A new `CardData` subclass representing collectible **Secret Lore Hint** scroll fragments found during Expeditions.

**Key behaviours:**
- **Unstackable**: `CanHaveCard` returns `false` — no cards can be stacked on it, and it cannot act as a root stack either.
- **Auto-unlock**: On `OnInitialCreate()` and every `UpdateCard()`, calls `ChronicleManager.UnlockHint(this.Id)` to register the discovery.
- **Visual highlight**: Sets `MyGameCard.HighlightActive = true` for a shimmer effect.
- **Double-click to read**: Tracks `_lastClickTime`; if two clicks occur within 350ms, opens a lore dialogue via `DialogueSystem.Instance.StartDialogue(...)` with `MewtationsLoc` bilingual strings.
- **Fallback dialogue**: If the card ID doesn't match `hint_1/2/3`, shows a generic "Lost Scroll Fragment" message.

---

### [`GameScripts/Core/Systems/ChronicleManager.cs`](GameScripts/Core/Systems/ChronicleManager.cs)

Static singleton managing which lore hints have been found this session.

| Method | Purpose |
|--------|---------|
| `UnlockHint(string id)` | Mark a hint as discovered |
| `IsHintUnlocked(string id)` | Check if a hint was found |

Used by both `SecretLoreHintCardData` (to register) and `Blueprint`/`DialogueSystem` (to gate content).

---

### [`GameScripts/Core/Systems/DialogueSystem.cs`](GameScripts/Core/Systems/DialogueSystem.cs)

Full IMGUI-based dialogue and Chronicle UI. Attached to a persistent `MonoBehaviour` (`DialogueSystem.Instance`).

**Dialogue API:**
```csharp
// Simple list of string choices
DialogueSystem.Instance.StartDialogue(title, text, choices, (idx) => { });

// Branching choices with requirements and callbacks
DialogueSystem.Instance.StartDialogue(title, text, new List<DialogueChoice> { ... });
```

**`DialogueChoice`** class supports:
- `Text` — displayed button label
- `RequirementText` — shown grayed if unavailable
- `IsAvailable` — `Func<bool>` checked at render time
- `OnSelected` — `Action` fired on click

**Chronicle Window ("📖 Chronicle of Truth"):**
- Persistent button top-right when no dialogue is active
- Scrollable vault showing 3 lore fragments
- Locked fragments show `🔒 Lost Scroll Fragment` in grey
- Unlocked fragments show title + "Read Fragment" button
- Shows recipe status (locked/unlocked) and recipe details
- Clicking "Read" opens a dialogue; when closed, returns to Chronicle
- `Time.timeScale = 0` while Chronicle is open; restored intelligently (stays frozen if Expedition/Combat active)

**Visual styling:** Dark glass panel (`#1A1A24, α=0.95`), warm gold title (`#F2D999`), soft white body text, dark button backgrounds.

---

### [`GameScripts/Core/Systems/MewtationsLoc.cs`](GameScripts/Core/Systems/MewtationsLoc.cs)

Static bilingual localization for all Mewtations-specific strings. Auto-detects English vs Vietnamese from `SokLoc.instance.CurrentLanguage`.

**Usage:**
```csharp
string label = MewtationsLoc.Translate("btn_chronicle", "📖 Chronicle of Truth");
```

**String table covers:**
- Chronicle UI labels (`btn_chronicle`, `win_chronicle_title`, `win_chronicle_desc`, `btn_close`, `btn_read`, `lbl_recipe`, `lbl_unlocked`, `lbl_locked`, `lbl_lost_fragment`)
- Recipe details for fragments 1–3 (`recipe_1_details`, `recipe_2_details`, `recipe_3_details`)
- Hint lore titles, descriptions, and full body text (3 fragments, EN + VI)
- Talent strings (`talent_true_harmony_name`, `talent_true_harmony_desc`)
- Weary Dog Patrol encounter dialogue (6 dialogue states, EN + VI)

---

## Modified Files

### [`GameScripts/Crafting/Base/Blueprint.cs`](GameScripts/Crafting/Base/Blueprint.cs)

Added `CanCurrentlyBeMade` property to gate recipes behind Chronicle unlock state.

**Logic:**
- Talisman recipes → require `item_secret_lore_hint_1` unlocked
- `item_breakthrough_pill` recipe → require `item_secret_lore_hint_2` unlocked
- All other recipes → always craftable

**Tooltip integration:** `GetText()` now appends a red locked warning if `!CanCurrentlyBeMade`:
```
✗ LOCKED: This recipe is sealed. Uncover the corresponding Secret Lore Hint in Expedition to decode it!
✗ KHÓA: Công thức này đang bị phong ấn. Hãy tìm Cổ Bản Kí Sự tương ứng trong Viễn Chinh để giải mã!
```

### [`GameScripts/Cards/Cats/CatCardData.cs`](GameScripts/Cards/Cats/CatCardData.cs)

Minor updates to integrate with the lore/talent system.

### [`GameScripts/Cards/Data/BreakthroughArrayCardData.cs`](GameScripts/Cards/Data/BreakthroughArrayCardData.cs)

New card supporting the **True Harmony Covenant** ritual — stacking a Nascent Soul Cat (Breakthrough 4) and 3 Hint Fragments to trigger the final win condition.

### [`GameScripts/Expedition/ExpeditionManager.cs`](GameScripts/Expedition/ExpeditionManager.cs)

Extended with `IsExpeditionActive` property used by `DialogueSystem` to decide whether to restore `Time.timeScale` when closing dialogue/Chronicle.

---

## Game Flow Summary

```
Player finds item_secret_lore_hint_1 in Expedition
  → SecretLoreHintCardData.OnInitialCreate()
  → ChronicleManager.UnlockHint("item_secret_lore_hint_1")
  → Player double-clicks card → Lore dialogue opens (MewtationsLoc text)
  → Player opens Chronicle (📖 button) → sees Fragment I unlocked
  → Talisman blueprint tooltip shows as craftable (CanCurrentlyBeMade = true)

Player collects all 3 fragments
  → Chronicle shows all 3 unlocked
  → Place 3 fragments + Nascent Soul Cat in Breakthrough Array
  → True Harmony Covenant talent granted → Game ending achieved
```

---

## Verification

- All 4 new/modified files confirmed present in git index and on disk:
  - `GameScripts/Cards/Data/SecretLoreHintCardData.cs` ✅
  - `GameScripts/Core/Systems/ChronicleManager.cs` ✅
  - `GameScripts/Core/Systems/DialogueSystem.cs` ✅
  - `GameScripts/Core/Systems/MewtationsLoc.cs` ✅
  - `GameScripts/Crafting/Base/Blueprint.cs` ✅
- Commit `7026047` pushed to `origin/main` (branch `main`) ✅
- `git status`: clean working tree ✅

---

## Lore Atmosphere & Social Infiltration Update (26 May 2026)

To fully differentiate the mod from a simple "Stacklands clone", we have added thick mainland atmosphere, social class tension, dynamic NPC reaction, and infiltration danger.

### 1. Dynamic NPC Dialogue & Social Stratification Check
- Modified the **Weary Dog Patrol** encounter in [`GameScripts/Expedition/ExpeditionManager.cs`](GameScripts/Expedition/ExpeditionManager.cs):
  - Dynamically queries `ActiveCats.Max(c => c.BreakthroughLevel)` to evaluate the player's current cultivation tier.
  - **Low-Rank Dialogue** (Breakthrough < 2): Guard glares at you with absolute disdain: *"Có giấy phép chưa? Loại tạp mèo như ngươi mà cũng muốn mò vào đây sao?..."*
  - **High-Rank Dialogue** (Breakthrough $\ge$ 2): Guard immediately straightens up and bows respectfully: *"À, ngài đã đột phá rồi sao. Xin lỗi vì lúc trước tiểu nhân thất lễ... Khu vực giới nghiêm này giờ đã mở cho những người cấp cao của ngài."*

### 2. Immersive Card Descriptions (Dogma Propaganda & Mainland Life)
Overrode several generic card descriptions in `MewtationsLocTable.tsv` with rich, world-building lore:
- **`card_house_description` (House):** Crowded lower districts where cats gather to share warmth and whisper about rebellion.
- **`card_quarry_description` (Quarry):** State-regulated mines where Overseers closely monitor and tax spiritual energy extraction.
- **`card_garden_description` (Garden):** Tiny patch where growing simple food requires a localized permit from the Dogma.
- **`card_sawmill_description` (Sawmill):** Heavy industrial mill filled with smoke and screaming saws that echo the Dogma's harsh order.

### 3. Expedition Rebranded to "Infiltration / Trespassing"
- **Breach Portals:** Rebranded portals to **Unstable Breach** (*Kẽ Nứt Vô Chủ*) and **Stabilized Breach** (*Cổng Xâm Nhập Ổn Định*) with descriptions emphasizing planned incursion into Dogma restricted areas.
- **Recipe Status:** Changed recipe status UI to **Classification Status** (*Trạng Thái Cấp Phép*) and unlock display to **Authorized** (*Đã Cấp Phép*) / **Sealed** (*Bị Niêm Phong*).

### 4. Raw & Refined Item Flavor Text
Mapped hyper-detailed lore strings to resource cards in `MewtationsLocTable.tsv`:
- **Spirit Ore:** *"Quặng linh thạch cấp thấp. Tầng dưới thường phải đổi cả tuần lương để mua một mảnh."*
- **Spirit Fuel:** *"Nhiên liệu tinh luyện bị kiểm soát chặt bởi Dogma."*
- **Black Market Refiner:** *"Máy luyện lậu tự chế. Không được cấp phép sử dụng trong khu dân cư."*

These updates ground the game strongly in survival, class struggle, and atmosphere, completely separating its identity from Stacklands.

### 5. Ambient Pressure Warnings & System Alerts
We have integrated high-tension **Dogma security notifications** directly into card tooltips using colored rich text (`<color>` tags):
- **Class-C Operator (`card_villager_description`):** Appends `[CITIZEN DOSSIER: Low-grade resident. Restricted movement. Unauthorized travel outside residential zones is strictly prohibited.]` in dark red.
- **Overseer (`card_militia_description`):** Appends `[ENFORCEMENT MANDATE: Constant surveillance of the Spirit Quota. Report any signs of mental deviation or physical mutation immediately.]` in gold.
- **Sovereign (`card_swordsman_description`):** Appends `[PROPAGANDA BROADCAST: Absolute obedience to the Dogma is the only path to safety. Resistance will result in immediate cleansing.]` in dark red.
- **Spirit Ore (`card_iron_ore_description`):** Appends `[SECURITY NOTICE: Unauthorized mining or possession of Spirit Ore is illegal. Violators will be detained by the Security Enforcers.]` in dark red.
- **Spirit Fuel (`card_refined_spirit_fuel_description`):** Appends `[SAFETY REGULATION: Authorized personnel only. Unauthorized consumption of Spirit Fuel detected will trigger an immediate local audit.]` in gold.
- **Black Market Refiner (`card_black_market_refiner_description`):** Appends `[SYSTEM ALERT: Unauthorized Consumption Detected. Location flagged. Inspection Incoming. Restricted zone audit scheduled.]` in dark red.

These alerts constantly pressure the player and reinforce the near-modern industrial-spirituality setting, without bloating the game with unnecessary cosmic lore.

### 6. Dynamic Narrative Events & Loading Screen Tips
To fully deepen the **"social-industrial spirituality"** vibe, we implemented highly immersive local events and administrative propaganda:
- **Propaganda Loading Tips:** Overrode `label_death_intro_1` through `label_death_intro_5` in `MewtationsLocTable.tsv` with cold, rigid Dogma laws (e.g., *"Dogma reminds all citizens: instability is the root of suffering."*, *"Spirit Quota violations are punishable by confiscation."*, *"Unregistered Mewtations must report to the nearest Overseer."*).
- **Dynamic Merchant Encounter (`exp_merchant_encounter_title`):**
  - **Low-Rank:** Merchant sneers at your primitive cats: *"Biến đi! Loại tạp mèo thấp kém như các ngươi không đủ cấp để xem hàng này..."* and only offers cheap, basic Spirit Ore for 3 Gold.
  - **High-Rank:** Merchant bows and whispers: *"Nhìn ngài có vẻ là một Hộ Pháp cao cấp... Tiểu nhân có vài món bảo vật giấu riêng, hoàn toàn không ghi trong sổ sách..."* and offers rare pills.
- **License Check & Confiscation event (`exp_license_check_title`):** Enforcers scan your bags, giving the choice to bribe with gold, accept resource confiscation, or attempt an agile escape.
- **Low-Grade Beggar event (`exp_beggar_title`):** A mutated scavenger begs for a fragment of Spirit Ore to save their starving child, offering a path to purge accumulated Corruption or gain Greed if rejected.

This tightly couples character rank (cultivation) with tangible social privilege and daily survival in a highly unique, memorable setting.

## Turn-Based RPG Combat Architecture Transition (Phase 5 — Purge & Standardization) — 26 May 2026

We have successfully completed **Phase 5 (Purge & Standardization)** of our combat engine upgrade, completely obsoleting the legacy Stacklands real-time system and establishing a unified, deterministic, state-driven turn-based tactical combat system (**CombatV2**).

### 1. Transitional Serialized Migration (Stats Transformation)
- **File Modified:** [`GameScripts/Combat/Base/CombatStats.cs`](GameScripts/Combat/Base/CombatStats.cs)
  - Applied **Transitional Serialized Migration** to preserve existing data in serialized assets (Prefabs, ScriptableObjects, Save files).
  - Renamed the raw private serialized variables utilizing `[SerializeField]` and `[FormerlySerializedAs]` to maintain Unity's serialization state perfectly.
  - Wrapped these fields under new clean, tactical RPG public properties:
    - `AttackSpeed` $\rightarrow$ `Initiative`
    - `HitChance` $\rightarrow$ `Accuracy`
    - `AttackSpeedIncrement` $\rightarrow$ `InitiativeIncrement`
    - `HitChanceIncrement` $\rightarrow$ `AccuracyIncrement`
  - Kept old properties marked with `[Obsolete]` to guarantee zero compile issues across other game files.

### 2. Purge of Legacy Real-Time Combat Logic
- **File Modified:** [`GameScripts/Combat/Base/Combatable.cs`](GameScripts/Combat/Base/Combatable.cs)
  - Added new clean score getters `GetAccuracyScore()` and `GetInitiativeScore()` based on turn-based principles (incorporating status effects like frenzy/drunk/slow percent).
  - Hard-purged all legacy auto-attack loop updates, real-time timer calculations, and real-time combat execution methods (e.g. `StartAttack()`, `CompleteAttack()`, `PerformAttack()`, `UpdateAttackAnimations()`).
  - Marked obsolete variables (`AttackTimer`, `InAttack`, `AttackTargets`) safely as obsolete.

### 3. Renamed Class Types and Compatibility Cleanup
- **File Modified:** [`GameScripts/Combat/Base/BattlefieldContext.cs`](GameScripts/Combat/Base/BattlefieldContext.cs)
  - Renamed all remaining return types, parameters, and local instance initializers from `Conflict` to `BattlefieldContext`.
  - Ensured all 5 main files ([`GameCard.cs`](GameScripts/Cards/Base/GameCard.cs), [`Combatable.cs`](GameScripts/Combat/Base/Combatable.cs), [`WorldManager.cs`](GameScripts/Core/WorldManager.cs), [`SaveSystem.cs`](GameScripts/Core/Systems/SaveSystem.cs), [`ForestCombatManager.cs`](GameScripts/Combat/Base/ForestCombatManager.cs)) compile seamlessly with the renamed type.

### 4. Battlefield Tactical Rulespace & Threat Projection
- **File Modified:** [`GameScripts/Combat/Base/BattlefieldContext.cs`](GameScripts/Combat/Base/BattlefieldContext.cs)
  - Upgraded `BattlefieldContext` to serve as the **Tactical Rulespace** for turn-based battles.
  - Implemented **Spatial Occupancy Rules** (`IsCellOccupied`) where cells (slots 0-5) are blocked by active units or dying corpses (until `DeathResolution` finishes) to prevent overlap bugs.
  - Implemented **Threat Projection Helpers**:
    - `IsProtectedCell` — Backline (3-5) is protected from direct melee damage if the frontline (0-2) is occupied by an ally.
    - `IsControlZone` — Active frontlines representing guard threat.
    - `GetThreatZone` — Visual/logical adjacent cell checking on the 2x3 combat grid.

### 5. Event Stream & Reaction Chain Controls
- **File Modified:** [`GameScripts/CombatV2/Core/CombatEncounter.cs`](GameScripts/CombatV2/Core/CombatEncounter.cs)
  - Declared `CombatEventType` and `CombatEvent` structures to support **Event Stream Architecture** (tracking `ActionDeclared`, `SnapshotCreated`, `GuardTriggered`, `DodgeSucceeded`, `HPCommitted`, `UnitDied`).
  - Integrated deterministic **Reaction Window Priority** limits:
    - Set up a lock (`ReactionDepth`) to guarantee `Maximum reaction depth = 1` preventing infinite action loops (Reaction Chain Rules).
  - Implemented **Death Authority Rule**: units are set to `dying` immediately when HP hits 0 during `CheckDeaths`, but physically removed only during `DeathResolution`.

---

## Verification & Compilation Status

- **Code Integrity:** All wrappers, obsoleted methods, and transitional attributes compile flawlessly with Unity's assembly layout.
- **Save Compatibility:** Verified that the save system still parses combat boundaries cleanly using the upgraded `BattlefieldContext` definitions.
- **Turn Determinism:** Verified that the stable initiative tie-breaker coupled with the event-driven transition loop guarantees deterministic simulation behavior.


