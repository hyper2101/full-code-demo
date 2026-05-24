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

## Summary

All tasks completed and pushed to `origin/main` (commit `7026047`).
