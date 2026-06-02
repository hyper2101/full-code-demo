# Task List — Mewtations Lore & Dialogue System

## Completed Tasks

- [x] **SecretLoreHintCardData.cs** — New card type for secret lore hint items
  - Unstackable (cannot have cards stacked on top)
  - Auto-registers unlock in `ChronicleManager` on create/update
  - Double-click detection (350ms window) to open dialogue
  - Shows lore hint dialogue via `DialogueSystem` using `MewtationsLoc` strings
  - Visual shimmer highlight when active

- [x] **ChronicleManager.cs** — Static manager for tracking unlocked lore hints
  - `UnlockHint(string id)` — registers a hint as found
  - `IsHintUnlocked(string id)` — checks unlock state
  - Persistent across session via static dictionary

- [x] **DialogueSystem.cs** — Full in-game dialogue UI system (IMGUI-based)
  - `StartDialogue(title, text, choices, callback)` — simple choice dialogue
  - `StartDialogue(title, text, branchingChoices)` — branching choice dialogue with requirements
  - `HideWindow()` — close dialogue
  - "📖 Chronicle of Truth" persistent button (top-right, visible when no dialogue active)
  - Chronicle window: scrollable vault of 3 lore fragments with lock/unlock display
  - Recipe status shown per fragment (locked/unlocked)
  - Re-read dialogue from Chronicle window
  - Dark glassmorphism UI style (warm gold title, soft white body)
  - Time freeze (`Time.timeScale = 0`) during dialogue and Chronicle view
  - Smart time restore (stays frozen if Expedition/Combat is active)

- [x] **MewtationsLoc.cs** — Bilingual localization system (English / Vietnamese)
  - Auto-detects language from `SokLoc.instance.CurrentLanguage`
  - `Translate(key, defaultText)` — lookup with fallback
  - Full string table: Chronicle UI, recipe details, hint lore (3 fragments), dialogue event strings

- [x] **Blueprint.cs** — Modified to support lore-gated recipe locking
  - `CanCurrentlyBeMade` property checks `ChronicleManager` unlock state
  - Talisman recipes gated behind Hint 1 (`item_secret_lore_hint_1`)
  - Breakthrough Pill recipe gated behind Hint 2 (`item_secret_lore_hint_2`)
  - Tooltip shows red "✗ LOCKED / ✗ KHÓA" message when recipe is sealed
  - Bilingual lock message via `MewtationsLoc.CurrentLang`

- [x] **CatCardData.cs** — Minor update (integrated with new systems)

- [x] **BreakthroughArrayCardData.cs** — New card supporting advanced breakthrough mechanics

- [x] **ExpeditionManager.cs** — Extended to support expedition-based lore discovery
  - `IsExpeditionActive` property exposed for time-scale management in DialogueSystem

## Current Phase: Lore Terminology Refinement & Localization Interceptor
- [x] **Localization Interceptor Pattern** in `CardData.cs`
  - Intercepted `Name` and `Description` getters to query `MewtationsLoc.Translate()` first
  - Dynamic fallback to `SokLoc.Translate()` for non-overridden terms
- [x] **Refined Thematic Terminology** in `MewtationsLocTable.tsv`
  - Applied the **70% gameplay clarity / 30% lore flavor** balance rule
  - Cleaned up overly long "hard sci-fi bureaucracy" terms into punchy marketable titles (e.g., *Class-C Operator*, *Overseer*, *Sovereign*, *Spirit Quota*, *Spirit Fuel*)
  - Replaced cyberpunk drone terminology with *Security Enforcer* (avoided sci-fi automation/holograms)
  - Balanced boss types by mixing Dogma officials with *Corrupted Beasts*, *Rogue Cultivators*, and *Void Guardians* (rather than rewriting all bosses to bureaucrats)
  - Added *Black Market Refiner* for crystal clear gameplay recognition

## Summary

All core features, dialog systems, and the Localization Interceptor with refined gameplay-clarity-first terminology are successfully implemented!

## Recent Updates (Today)
- [x] **ExpeditionManager.cs Refactor**
  - Completely removed hardcoded English placeholder strings ("Translated Log", "exp_generic_log").
  - Dynamically mapped legacy immersive strings (from Expeditionlegacy.cs) into event nodes (e.g. Kiếp Lôi, Lò Đan, Ma Huyệt, Dogma Hành Pháp).
  - Maintained 100% of the newly corrected logic (gold deduction, rewards, HP changes, mutations).
- [x] **MewtationsLocTable.tsv Consolidation**
  - Synced all newly mapped legacy strings into the central .tsv file.
  - Merged redundant root-level .tsv file into the official GameScripts\Core\Systems\MewtationsLocTable.tsv.
  - Deduped keys and verified 100% localization coverage for ExpeditionManager.cs.
- [x] **Workspace Cleanup**
  - Removed temporary generator scripts (Generator.cs, Generator3.cs, FixRemnants.cs).
  - Removed redundant Expeditionlegacy.cs after extracting its text.
  - Merged GDD texts into a master gdd.md.
