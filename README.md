# Wheel Of Fortune - Unity Demo

A Unity mobile demo project inspired by a wheel-of-fortune reward system.  
The player spins a reward wheel, collects prizes, avoids bombs, and can choose to leave with collected rewards on safe zones.

This project was developed as a game developer demo using the provided UI assets.

---

## Revision Summary

This version includes a revised implementation based on review feedback.

| Feedback | Revision |
|---|---|
| Top bar visuals/animations were not close enough to the sample game | Added a redesigned progress/zone indicator, safe/super zone color feedback, and smoother zone info animation |
| Inventory system visual was not close enough to the sample game | Added a left-side grouped inventory panel with scrollable content |
| Spin animation had flicker | Investigated multiple spin approaches and optimized the current wheel rotation setup, asset settings, hierarchy, and spin timing |
| Code quality was not SOLID enough | Refactored the project into separated responsibilities: core, data, UI, animation, audio, events, and utilities |
| Event manager was missing | Added `GameEventBus` and gameplay event definitions |
| Namespace was missing | Added namespaces to revised scripts |
| Duplicate helper methods existed | Added shared `ComponentFinder` utility |
| Constants were scattered | Added `GameConstants` and `RewardTextFormatter` |
| State logic used multiple bools | Added a `GameState` enum |

---

## Project Overview

The game is a risk-reward wheel spinning game.

The player progresses through zones by spinning the wheel. Each spin can give a reward or trigger a bomb. Rewards collected during the run are stored temporarily. If the player hits a bomb, they can either restart or revive and continue without losing the collected rewards.

The game includes normal zones, safe zones, and super zones.

The revised version also includes:

- Start screen before gameplay
- Left-side grouped inventory
- Top progress/zone indicator
- Safe and super zone visual feedback
- Grouped win reward popup
- Event bus structure
- Utility classes
- Namespace-based script organization

---

## Screenshots and Gameplay Media

### Screenshots

| Bronze Wheel Reward | Silver Wheel Reward | Start Screen |
|---|---|---|
| ![Bronze wheel reward appearance](Media/Screenshots/bronze_wheel_reward_appearance.jpg) | ![Silver wheel reward appearance](Media/Screenshots/silver_wheel_rewardappearance.jpg) | ![Silver wheel reward appearance](Media/Screenshots/start_screen.jpg) |

| Bomb / Revive Popup | Reward Collection Popup |
|---|---|
| ![Bomb revive restart popup](Media/Screenshots/bomb_revive_restart.jpg) | ![Leave reward collection](Media/Screenshots/leave_reward_collection.jpg) |

### Gameplay Videos

- [Full 30-level gameplay video (part 1)](Media/Videos/full_fortune_wheel_30levels_part1.mp4)
- [Full 30-level gameplay video (part 1)](Media/Videos/full_fortune_wheel_30levels_part2.mp4)


---

## Aspect Ratio Screenshots

The UI was prepared to support multiple aspect ratios as required by the demo brief.

| 20:9 | 16:9 | 4:3 |
|---|---|---|
| ![20:9 aspect ratio screenshot](Media/Screenshots/game_aspect_20_9.png) | ![16:9 aspect ratio screenshot](Media/Screenshots/game_aspect_16_9.png) | ![4:3 aspect ratio screenshot](Media/Screenshots/game_aspect_4_3.png) |

---

## Gameplay Rules

- The player starts from Zone 1.
- In normal zones, the wheel contains rewards and one bomb.
- If the wheel lands on a reward, the reward is added to the player's collected rewards.
- If the wheel lands on a bomb, the bomb popup appears.
- The player can revive after a bomb and continue without losing collected rewards.
- The player can restart after a bomb, which resets the run.
- Every 5th zone is a safe zone.
- Every 30th zone is a super zone.
- Safe and super zones do not contain bombs.
- The player can leave and collect rewards only on safe or super zones.
- Rewards are grouped by reward category in the inventory and win popup.

---

## Implemented Features

### Core Gameplay

- Wheel spinning system
- Random reward selection
- Bomb slice logic
- Zone progression
- Safe zone logic
- Super zone logic
- Exit system
- Restart system
- Revive after bomb system
- Start screen before gameplay
- Game state enum for clearer flow control

### Reward System

- Rewards are editable from the Unity Inspector
- Each wheel slice can have:
  - Reward ID
  - Reward name
  - Reward icon
  - Reward amount
  - Bomb state
  - Display label
- Collected rewards are stored during the run
- Rewards are grouped by reward category
- Cash from different wheels is merged into one cash entry
- Gold from different wheels is merged into one gold entry
- Final rewards are shown as grouped entries in the win popup

### UI Features

- Mobile portrait UI layout
- Responsive Canvas setup
- Start screen
- Wheel UI with reward slices
- Top progress/zone indicator
- Safe and super zone color feedback
- Left-side grouped inventory panel
- Scrollable inventory without visible scrollbar
- Bomb popup
- Revive button
- Restart button
- Win popup
- Grouped reward display in win popup
- Collect button
- Single reward reveal animation after each successful spin
- Smooth zone info panel scaling animation

### Visual Effects

- Reward reveal effect after a successful spin
- Flash effect behind the newly won reward
- Smooth safe/super zone info scaling
- Popup overlay for better focus
- Wheel spin animation with adjusted timing and cleaned hierarchy

### Audio Support

The project supports audio clips for:

- Wheel spinning
- Reward gained
- Reward collection
- Bomb explosion

Audio clips can be assigned from the Unity Inspector on the audio service component.

---

## Controls

This is a touch/click based UI game.

| Button | Function |
|---|---|
| Start | Opens the gameplay screen |
| Spin | Spins the wheel |
| Exit | Leaves the game and opens the final reward popup on safe/super zones |
| Revive | Continues after hitting a bomb without losing rewards |
| Restart | Restarts the game from Zone 1 |
| Collect | Plays the collection effect and restarts the run |

---

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
