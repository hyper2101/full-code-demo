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
