# DEVLOG.md — Card Match Game Decision Journal

## Key Decisions

### 1. Singleton Pattern for Managers
I used a singleton pattern for `GameLogicHandler` and `AudioManager`. Since these are scene-wide systems that many objects need to access, a singleton avoids the overhead of dependency injection or event buses for a project of this scope. It keeps card scripts simple — a card just calls `GameLogicHandler.instance.cardClicked(...)` without needing a reference wired up in the Inspector.

### 2. Card Flip via Coroutine (Quaternion Slerp)
Card flipping is driven by a coroutine that Slerps the card's Y rotation from 0° to 90°, swaps the sprite at the midpoint, then continues to 180°. This gives a natural flip feel without any animation controller or Animator component. Keeping it in pure C# also makes it easy to cancel mid-flip if a new flip is triggered.

### 3. JSON Save/Load via `Application.persistentDataPath`
Game state (score, time, card positions, sprite assignments, flipped states, colors) is serialised to a single JSON file using `JsonUtility`. `persistentDataPath` is the correct cross-platform location for save data and works on both Android and iOS without extra permissions. The save is triggered on `OnApplicationQuit` so progress is never lost mid-session.

### 4. Dynamic Grid Layout in Code (No `GridLayoutGroup`)
The card grid is built entirely in `SetGamePanel()` by computing cell size and position manually from the panel's `RectTransform.sizeDelta`. This gave me direct control over spacing and scale, especially for handling odd-sized grids (e.g. 3×3 = 9 cells, so one "wildcard" centre cell is reused).

### 5. Sprite Pre-allocation with Shuffle Logic
`SpriteCardAllocation()` picks random sprite IDs for pairs and then randomly assigns them to cards. A simple linear-probe de-duplication (`(value + 1) % length`) is used instead of a Fisher-Yates shuffle to avoid pulling in extra utilities.

---

## Approach Tried and Abandoned

### GridLayoutGroup Component
Initially I placed a `GridLayoutGroup` component on the card container and let Unity handle positioning automatically. This worked fine for even grids but broke immediately for odd-sized grids (e.g. 3×3 where 9 cards don't pair evenly), because `GridLayoutGroup` has no concept of a "filler" or centre cell. Overriding it with a custom `LayoutElement` on the last cell created layout rebuild conflicts at runtime.

I abandoned this approach and switched to fully manual positioning in `SetGamePanel()`, which gave me complete control over the odd-cell edge case and made save/load of positions trivial (positions are just `Vector3` values stored in `GameState`).
