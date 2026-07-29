# Wheel Of Fortune — Unity Demo

A mobile wheel-of-fortune reward demo. The player spins for prizes, risks a bomb
on every normal zone, and chooses when to walk away with the run's rewards.

Unity **6000.5.1f1** · portrait mobile UI · reference resolution 1080×1920

---

## Revision 3 — response to the technical review

Every numbered point from the review is addressed below, with the files that
changed.

### 1. Wheel rotation is no longer off-centre

The rotator pivot was never the problem — it was already `0.5, 0.5` with a zero
anchored position. The real cause was the eight reward slots, which had been
positioned by hand:

| Slot | Old radius | Old angle | Now |
|---|---|---|---|
| 01 | 319.0 | 90.0° | 305 / 90° |
| 02 | 311.5 | 47.6° | 305 / 45° |
| 03 | 300.3 | 2.5° | 305 / 0° |
| 04 | 300.7 | −43.0° | 305 / −45° |
| 05 | 290.0 | −90.0° | 305 / −90° |
| 06 | 297.3 | −137.7° | 305 / −135° |
| 07 | 306.5 | 176.6° | 305 / 180° |
| 08 | 314.8 | 133.1° | 305 / 135° |

Radii varied by 29 units and angles by up to 3.4°, and `ui_slot_01` also carried
a stray **−4.119° rotation on the X axis**. Every icon therefore orbited a
slightly different circle, which reads as the wheel wobbling off centre.

Fixed in the scene, and enforced in code so it cannot drift again:
`Utilities/RadialLayout.cs` places slots on an exact circle, and
`UI/WheelViewMono.ApplyRadialLayout()` applies it on `Awake`.
`EnsureRotatorPivotIsCentred()` corrects and warns if the pivot is ever moved.

### 2. Bomb rule now matches the brief

`ReviveAfterBomb` — which ran for free, kept every reward and additionally
advanced the player a zone — has been removed.

| Outcome | Behaviour |
|---|---|
| **Bomb** | Every reward collected this run is destroyed, the run restarts at zone 1 |
| **Continue** | A separate paid bonus: debits the player's wallet, keeps the rewards, and **replays the same zone** |

Continuing never advances progress by itself, and the button is disabled when
the balance is short — the price is never cosmetic.
See `Core/WheelGameControllerMono.HandleBombResult / ContinueAfterBomb /
GiveUpAfterBomb` and the new `UI/BombPopupViewMono`.

### 3. Reward progression across 30 zones

Progression works two different ways, because a chest and a pile of cash should
not scale the same.

**Currencies** grow on a curve — `base × (1 + growth)^(zone − 1) × zoneBonus`,
rounded to a readable figure (`Economy/ZoneRewardScaler`):

| Zone | 1 | 5 | 15 | 25 | 29 | 30 (super) |
|---|---|---|---|---|---|---|
| Cash | 500 | 1,100 | 3,200 | 9,200 | 9,300 | 31,000 |
| Gold | 1 | 2 | 6 | 18 | 19 | 62 |

**Gifts are not multiplied — they are replaced.** `Economy/RewardTierTable` is a
ScriptableObject holding four quality tiers, and `Economy/ZoneTierProgression`
promotes one slot at a time, lowest slot first: zone 1 shows all Tier 1 gifts,
then one slot becomes Tier 2, then two, and once the whole wheel has reached a
tier the sweep continues into the next one.

| Zone | Gift slots |
|---|---|
| 1 | Small Chest ¹ · Pistol Points ¹ · Bronze Chest ¹ · Knife Points ¹ · SMG Points ¹ |
| 3 | **Standard Chest ²** · Pistol Points ¹ · Bronze Chest ¹ · Knife Points ¹ · SMG Points ¹ |
| 7 | Standard Chest ² · Rifle Points ² · **Silver Chest ²** · Knife Points ¹ · SMG Points ¹ |
| 15 | **Gold Chest ³** · **Sniper Points ³** · Silver Chest ² · Shotgun Points ² · Armor Points ² |
| 23 | **Super Chest ⁴** · Sniper Points ³ · Big Chest ³ · Vest Points ³ · T1 Shotgun ³ |
| 30 | Super Chest ⁴ · T2 Rifle ⁴ · T3 Sniper ⁴ · T2 MLE ⁴ · T1 Shotgun ³ |

At the shipped `_zonesPerTierPromotion = 2` the ladder is paced to land on
near-complete Tier 4 exactly at the super zone. Both the tiers and the pacing are
editable in `Assets/Settings/RewardTierTable.asset` and
`GameEconomySettings.asset` without touching code or the scene.

This also removes the earlier oddity where a chest count was multiplied into
"Bronze Chest x9" — item amounts now come from the tier definition.

The whole wheel is resolved in one pass (`IWheelRewardResolver`), and the spin
awards the entry that was already drawn on the face, so the displayed reward and
the granted reward cannot drift apart.

The hard-coded `labelOverride` values (`x500`, `x3000`) were cleared on all 24
slices — they would have masked the resolved amount.

### 4. Typed currency layer

`TotalReward` — a single `int` that summed cash, gold and rifle points into a
meaningless number — is gone.

- `Economy/CurrencyType` + `RewardKind` classify every slice.
- `Economy/RunRewardInventory` keeps a **separate total per currency**.
- `Economy/PlayerWallet` is the player's real, persistent inventory behind
  `IWalletStorage` (PlayerPrefs today, swappable for a server).
- Rewards enter the wallet **only** when the player leaves on a safe zone and
  collects. Continue costs are debited from that same wallet.

Adding a new currency means adding one enum value; the inventory, wallet and HUD
are all keyed by it.

### 5. Dependency inversion

- **No `OnValidate` anywhere.** Editor auto-wiring moved to `Reset()`, which is
  the sanctioned place to touch the hierarchy. This also removes the
  `AddComponent` calls that triggered Unity's *"SendMessage cannot be called
  during OnValidate"* warning.
- **No `FindObjectOfType` and no `GameObject.Find`.** All eight scene scans are
  gone; references are serialized, and a missing one is a loud error.
- The controller now consumes **nine interfaces** — `IWheelSpinner`,
  `IAudioService`, `IWheelConfigProvider`, `IZoneService`, `IRunRewardInventory`,
  `IPlayerWallet`, `IRewardScaler`, `IContinueCostPolicy` and the event bus.
  Serialized fields exist only because Unity cannot serialize an interface; no
  logic refers to a concrete type.
- `Core/GameServices` is the composition root, with an `Initialize(GameServices)`
  injection point for test doubles.

### Collect effect

The old effect scaled the reward list down to 0.88 and faded it out — one flat
tween on the whole container. It has been replaced with a proper collect
(`UI/RewardFlight` + `WinPopupViewMono.PlayCollectEffectCoroutine`):

- every reward pops to 1.18×, then arcs to the wallet HUD along a quadratic
  bézier, staggered 0.06 s apart, shrinking and spinning slightly as it goes;
- rewards are reparented to the popup root first — the scroll viewport has a
  `Mask` and the content a `HorizontalLayoutGroup`, either of which would clip
  them or snap them back mid flight;
- the balance is credited **after** the rewards land, not before, so the number
  ticks up at the moment they arrive, and the HUD plays a scale punch.

All items are driven from a single coroutine rather than one per item, so a
popup with a dozen rewards costs one update loop and nothing is left running if
the popup is destroyed mid flight. Timings are exposed on the component.

### 6. House rules

| Rule | Status |
|---|---|
| `_camelCase` private fields | all serialized fields renamed (scene updated to match) |
| `Coroutine` suffix | every coroutine method and handle |
| `SCREAMING_SNAKE_CASE` consts | `Utilities/GameConstants.cs` |
| `Mono` suffix on MonoBehaviours | all renamed, GUIDs preserved |
| `if (obj)` for Unity objects | no `== null` or `?.` left on a Unity object |
| No anonymous lambdas | `WheelViewMono.CompareByName` replaces the inline sort |
| Coroutines stopped on destroy | `OnDestroy` in every MonoBehaviour that starts one |
| `[Range]` / `[Min]` on serialized numerics | applied; `spinDuration = 0` can no longer produce NaN |
| Repository junk | 174 `_MACOSX/._*` files and 25 unused icons deleted; `.gitignore` hardened |

**One correction to the review note:** `Assets/demo_content/` is not 123
irrelevant icons — it is the project's only art folder, and **36 of its 61 files
are referenced by the scene**. Deleting it wholesale would break the wheel, the
inventory and both popups. Only the 25 genuinely unused files were removed.

---

## Architecture

```text
Assets/Scripts
├── Animation   IWheelSpinner, CoroutineWheelSpinnerMono
├── Audio       IAudioService, AudioServiceMono
├── Core        GameServices (composition root), WheelGameControllerMono,
│               IZoneService, IWheelConfigProvider, GameState
├── Data        ZoneType, WheelSlice, WheelConfig
├── Economy     CurrencyType, RewardKind, RewardDraw, RewardEntry,
│               IRunRewardInventory, IPlayerWallet, IWalletStorage,
│               IRewardScaler, IContinueCostPolicy, GameEconomySettings,
│               RewardTierTable, ITierProgression, IWheelRewardResolver
├── Events      GameEventBus, GameEvents, GameEventLoggerMono
├── UI          *ViewMono (incl. new BombPopupViewMono, WalletHudViewMono)
└── Utilities   GameConstants, RadialLayout, NumberRounding,
                RewardTextFormatter, ComponentFinder
```

The `Economy` layer is plain C# with no Unity dependency beyond `Sprite`, so the
progression curve, wallet and continue pricing can be unit tested directly.

## Gameplay rules

- The run starts at zone 1. Every 5th zone is safe; every 30th is a super zone.
- Safe and super zones contain no bomb, and are the only places the player may
  leave and bank the run.
- Landing on a bomb destroys the run's rewards and restarts from zone 1, unless
  the player pays to continue.
- Continuing costs currency from the persistent wallet, keeps the rewards, and
  replays the same zone. The cost rises with the zone and doubles with each
  continue already bought in that run.
- Rewards are grouped by type in the inventory and the win popup; cash from
  different wheels merges into one cash stack.

## Controls

| Button | Function |
|---|---|
| Start | Opens the gameplay screen |
| Spin | Spins the wheel |
| Exit | Banks the run — enabled only on safe and super zones |
| Continue | Pays currency to survive a bomb and replay the zone |
| Restart | Abandons the run and returns to zone 1 |
| Collect | Deposits the run's rewards into the wallet |

## Known follow-ups

<<<<<<< HEAD
- The wallet HUD (`ui_panel_wallet`, top right) was authored directly in the
  scene YAML and has not been checked visually in the Editor — reposition to
  taste. The collect effect flies rewards to it, so if you move it the animation
  follows automatically.
- Tier 3 borrows `UI_Icon_Renders_tier1_shotgun` and Tier 4 reuses the tier 2/3
  weapon renders. Swap in dedicated art when it exists — the tiers are data, so
  it is an inspector change.
- Assembly definitions were deliberately **not** added. They are the natural next
  step and would let the `Economy` layer be covered by EditMode tests, but they
  change assembly names and were out of scope for this revision.
- Branching: this revision is still delivered as a single snapshot. Feature
  branches per review item would make the next round easier to review.
=======
## Code Architecture

The revised version separates gameplay, UI, data, events, utilities, audio, and animation responsibilities into different folders and classes.

```text
Assets/Scripts
├── Animation
├── Audio
├── Core
├── Data
├── Events
├── UI
└── Utilities
```

## Main Scripts

### `WheelGameController.cs`

Coordinates the main gameplay flow.

Responsibilities:

- Starts and restarts the game
- Handles spin input
- Coordinates state changes
- Selects random wheel result
- Calls the wheel spinner
- Resolves rewards and bombs
- Handles revive logic
- Handles exit and collect logic
- Raises gameplay events
- Updates the current zone

The controller no longer contains all UI logic directly. UI updates are delegated to separate view classes.

---

### `WheelData.cs`

Contains wheel-related data classes.

Includes:

- `ZoneType`
- `WheelSlice`
- `WheelConfig`

`WheelSlice` includes an `InventoryKey` so rewards can be grouped consistently by category.

---

### `RewardInventory.cs`

Handles collected reward logic.

Responsibilities:

- Stores collected rewards
- Groups rewards by reward ID
- Tracks total reward amount
- Tracks item count
- Provides grouped inventory entries for UI panels

---

### `ZoneService.cs`

Handles zone rules.

Responsibilities:

- Determines whether a zone is normal, safe, or super
- Determines whether the player can exit on the current zone

---

### `WheelConfigProvider.cs`

Provides the correct wheel configuration based on the current zone type.

This makes wheel setup more scalable than hardcoding normal, safe, and super wheel references directly in the controller.

---

### `GameState.cs`

Defines the main game states.

Includes:

- `Idle`
- `Spinning`
- `BombDecision`
- `WinPopup`
- `Collecting`

This replaces the earlier bool-based state handling.

---

### `GameInputView.cs`

Handles UI button input.

Responsibilities:

- Spin button event
- Exit button event
- Restart button event
- Bomb revive/restart button events
- Collect button event
- Gameplay button interactability

---

### `WheelView.cs`

Handles wheel visual updates.

Responsibilities:

- Updates wheel base sprite
- Updates indicator sprite
- Updates wheel slots
- Provides the rotating wheel transform

---

### `WheelSlotView.cs`

Controls the visual display of a single wheel slice.

Responsibilities:

- Shows reward icon
- Shows reward amount text
- Clears unused wheel slots

---

### `ProgressBarView.cs`

Controls the top progress indicator.

Responsibilities:

- Shows nearby zone numbers
- Keeps the current indicator aligned
- Applies safe/super zone colors
- Smoothly moves and pulses the current zone marker

---

### `InventoryPanelView.cs`

Controls the left-side inventory panel.

Responsibilities:

- Builds grouped inventory entries
- Uses a scrollable inventory content area
- Hides the scrollbar visually
- Prevents inventory content from overflowing outside the panel

---

### `ZoneInfoPanelView.cs`

Controls safe/super zone information panels.

Responsibilities:

- Shows next safe zone number
- Shows next super zone number
- Smoothly scales the active safe/super panel

---

### `SingleRewardRevealView.cs`

Controls the reward reveal animation after a successful spin.

Responsibilities:

- Shows the gained reward image
- Shows the reward name
- Shows the reward amount
- Plays scale and flash animation

---

### `WinPopupView.cs`

Controls the final reward popup.

Responsibilities:

- Shows total reward and item count
- Builds grouped reward entries
- Displays collected rewards by category
- Plays collect feedback animation

---

### `WinRewardItemView.cs`

Controls each grouped reward item in the win popup.

Responsibilities:

- Shows reward icon
- Shows reward name
- Shows grouped reward amount

---

## Event Manager

The revised version includes an event bus structure.

### `GameEventBus.cs`

Centralized event publisher.

### `GameEvents.cs`

Contains gameplay event definitions.

Current events include:

- `GameStartedEvent`
- `SpinStartedEvent`
- `SpinCompletedEvent`
- `RewardCollectedEvent`
- `BombHitEvent`
- `RevivedEvent`
- `GameRestartedEvent`
- `ZoneChangedEvent`

### `GameEventLogger.cs`

Optional event listener used for debugging and verifying event flow.

The event structure makes it easier to decouple gameplay flow from systems such as logging, audio, analytics, and UI reactions.

---

## Utility Classes

### `ComponentFinder.cs`

Shared utility for finding child components by object name.

This removes duplicated helper methods from multiple UI scripts.

### `RewardTextFormatter.cs`

Centralized text formatting for reward amounts and reward totals.

Examples:

- `x500`
- `TOTAL REWARD`
- `ITEMS`

### `GameConstants.cs`

Centralized constants for commonly used game values.

Examples:

- Safe zone interval
- Super zone interval
- Default spin duration
- Default full rotations
- Visible progress step count

---

## SOLID / Refactor Notes

The revised version improves separation of responsibilities:

- `WheelGameController` coordinates gameplay flow.
- `RewardInventory` manages inventory data.
- `ZoneService` manages zone logic.
- `WheelConfigProvider` provides wheel configuration.
- UI view classes only handle their own UI sections.
- Utilities remove duplicated helper code.
- Event bus centralizes event publishing.
- Interfaces were added for important systems.

Interfaces include:

- `IZoneService`
- `IRewardInventory`
- `IWheelConfigProvider`
- `IAudioService`
- `IWheelSpinner`

---

## Wheel Spin Flicker Investigation

The wheel spin animation was reviewed because the previous version had visible flicker/shimmer during rotation.

Several approaches were tested during revision:

- Reducing TextMeshPro auto-size and raycast overhead
- Removing unnecessary layout components from the rotating wheel
- Checking duplicate wheel rendering
- Testing single-image spin visuals
- Testing shader-based UV rotation
- Testing different spin durations and rotation counts
- Reviewing sprite import settings such as compression, filter mode, mesh type, and mip maps
- Reverting to the standard wheel rotation after heavier approaches caused performance or visual tradeoffs

The current version keeps the normal rotating wheel implementation, with a cleaned hierarchy and adjusted UI/sprite settings.

Recommended Unity settings used for wheel-related assets:

```text
Texture Type: Sprite (2D and UI)
Mesh Type: Full Rect
Image Type: Simple
Use Sprite Mesh: Off
Compression: None
Filter Mode: Bilinear
Generate Mip Maps: Off
Canvas Pixel Perfect: Off
```

---

## Build Information

Recommended Unity version:

```text
Unity 2021 LTS
```

Target platform:

```text
Android
```

The APK is available from the GitHub Releases section.

---

## How to Run

1. Clone the repository.
2. Open the project with Unity 2021 LTS.
3. Open the main scene.
4. Press Play in the Unity Editor.
5. Use the Start button to enter gameplay.

---

## APK

The Android APK is available in the GitHub Releases section.
>>>>>>> 5055b6a799a4fb2b0e06e10f5b01c31f993f697b
