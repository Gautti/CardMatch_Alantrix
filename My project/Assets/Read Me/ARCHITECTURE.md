# ARCHITECTURE.md — Card Match Game Structure & Data Flow

## Overview

The project is split into four MonoBehaviour/static classes with clear responsibilities:

```
GameLogicHandler   — game state, grid setup, match logic, save/load
Card               — individual card display, flip animation, click handling
AudioManager       — one-shot SFX playback
GameStateManager   — static save/load helper (JSON ↔ disk)
```

---

## Component Responsibilities

### `GameLogicHandler` (Singleton MonoBehaviour)
The central controller. Owns:
- Game lifecycle (`StartCardGame`, `EndGame`, `GiveUp`)
- Grid generation (`SetGamePanel`) — instantiates `Card` prefabs, positions them by computing offsets from the panel's `RectTransform`
- Sprite allocation (`SpriteCardAllocation`) — randomly assigns matched pairs to cards
- Match detection (`cardClicked`) — tracks the last-selected sprite ID and card ID; on second click, either marks both inactive (match) or flips both back (no match)
- Timer (increments `time` in `Update` while `gameStart == true`)
- Score display
- Delegating save/load to `GameStateManager`

### `Card` (MonoBehaviour, one per card prefab)
Owns only its own visual state:
- `Flip()` — starts a coroutine that Slerps Y rotation 0→90°, swaps sprite via `ChangeSprite()`, then continues 90→180°
- `Inactive()` — fades the card's `Image` color to `Color.clear` on a match
- `CardBtn()` — called by the UI Button's `OnClick`; guards against clicking a flipped or mid-turning card, then notifies `GameLogicHandler` after a short delay (so the flip animation is visible first)

### `AudioManager` (Singleton MonoBehaviour)
Wraps a single `AudioSource` and a clip array. Callers pass an integer index; `PlayOneShot` ensures overlapping SFX don't cut each other off.

### `GameStateManager` (Static class)
Pure I/O — no MonoBehaviour. Serialises a `GameState` POCO to JSON and writes it to `Application.persistentDataPath`. Keeps persistence logic completely decoupled from gameplay logic.

---

## Data Flow

```
User taps card
  └─ Card.CardBtn()
       ├─ Card.Flip()            [visual]
       └─ (0.5s delay)
            └─ GameLogicHandler.cardClicked(spriteId, cardId)
                 ├─ First card:  store spriteSelected, cardSelected
                 └─ Second card:
                      ├─ Match:   Card.Inactive() × 2, score++, CheckGameWin()
                      └─ No match: Card.Flip() × 2 (flip back)

OnApplicationQuit / manual Save
  └─ GameLogicHandler.SaveGame()
       └─ GameStateManager.SaveGame(GameState)  →  JSON file

LoadGame button
  └─ GameLogicHandler.LoadGame()
       └─ GameStateManager.LoadGame()  →  GameState
            └─ Rebuild card objects from saved positions, sprites, colors
```

---

## Trade-off I'd Revisit Given More Time

**Manual grid positioning vs. a proper layout system.**

`SetGamePanel()` computes every card's `localPosition` by hand using the panel's `sizeDelta`. It works, but the math is tightly coupled to a single fixed-size panel. If the panel ever resizes (e.g. different device aspect ratios or safe-area insets), all positions are off. A cleaner solution would be a custom `ILayoutGroup` implementation that handles the odd-cell case correctly, or a Content Size Fitter + programmatic cell sizing, so the grid adapts to any canvas size without hardcoded arithmetic. I'd make that change before shipping to multiple device form factors.
