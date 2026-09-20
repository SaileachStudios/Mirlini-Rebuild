# Complete 50-level inventory

Coordinates are original world (x, y, z). N counts visible wall segments plus four boundary walls, excluding corner posts. I counts invisible wall objects, including duplicates. E counts unique internal unit edges; Level 40 excludes its one off-grid segment. C counts solid corner posts. Every level has one active gameplay hole and one marble. Unlock duration is 2 seconds everywhere. See inventory.json for every wall, cell index, post, instruction variant and light configuration.

| Level | Features | Start | Goal | N / I / C | E | Unlock ring | Ideal seconds¹ |
|---|---|---|---|---|---|---|---|
| 1 | Normal | (-7, 0, -7) | (6.5, -0.5, 6.5) | 4 / 0 / 0 | 0 | — | 1.5 |
| 2 | Normal | (-7, 0, -7) | (-5, -0.5, -3) | 24 / 0 / 15 | 108 | — | 8.01 |
| 3 | Normal | (-7, 0, -7) | (1, -0.5, -1) | 21 / 0 / 11 | 101 | — | 7.89 |
| 4 | Normal | (-5, 0, -7) | (-5, -0.5, 5) | 26 / 0 / 8 | 98 | — | 7.11 |
| 5 | Normal | (1, 0, -1) | (7, -0.5, 3) | 17 / 0 / 11 | 98 | — | 7.87 |
| 6 | Unlock | (-7, 0, -7) | (6.5, -0.5, 6.5) | 4 / 0 / 0 | 0 | (0, -0.49, 0) | 4.45 |
| 7 | Unlock | (-3, 0, -1) | (7, -0.5, 7) | 34 / 0 / 15 | 95 | (6, -0.49, -6) | 11.58 |
| 8 | Normal | (-1, 0, -3) | (7, -0.5, 3) | 36 / 0 / 17 | 99 | — | 8.64 |
| 9 | Unlock | (-1, 0, 3) | (7, -0.5, 7) | 34 / 0 / 14 | 93 | (6, -0.49, -6) | 8.49 |
| 10 | Dark | (-7, 0, -7) | (6.5, -0.5, 6.5) | 4 / 0 / 0 | 0 | — | 1.5 |
| 11 | Dark | (-7, 0, -3) | (7, -0.5, -3) | 34 / 0 / 14 | 98 | — | 5.52 |
| 12 | Normal | (1, 0, -3) | (7, -0.5, 7) | 32 / 0 / 11 | 98 | — | 6.77 |
| 13 | Unlock | (-5, 0, -5) | (7, -0.5, 3) | 32 / 0 / 14 | 96 | (6, -0.49, 6) | 10.53 |
| 14 | Dark | (-7, 0, -7) | (1, -0.5, 7) | 33 / 0 / 14 | 98 | — | 6.77 |
| 15 | InvisibleWalls | (-7, 0, -7) | (6.5, -0.5, 6.5) | 4 / 15 / 0 | 14 | — | 1.5 |
| 16 | InvisibleWalls | (-7, 0, -7) | (5, -0.5, 5) | 4 / 98 / 0 | 98 | — | 8.18 |
| 17 | Normal | (-3, 0, -3) | (3, -0.5, -1) | 30 / 0 / 14 | 98 | — | 6.27 |
| 18 | Unlock | (-7, 0, 1) | (7, -0.5, 5) | 33 / 0 / 11 | 96 | (-4, -0.49, -2) | 12.46 |
| 19 | Dark | (7, 0, 5) | (7, -0.5, 1) | 35 / 0 / 14 | 98 | — | 5.81 |
| 20 | InvisibleWalls | (1, 0, 5) | (7, -0.5, 7) | 4 / 98 / 0 | 98 | — | 7.61 |
| 21 | Unlock + Dark | (-7, 0, -7) | (7, -0.5, 7) | 27 / 0 / 0 | 96 | (0, -0.49, -2) | 11.61 |
| 22 | Normal | (-7, 0, 7) | (7, -0.5, 5) | 35 / 0 / 15 | 98 | — | 6.59 |
| 23 | Unlock | (3, 0, -7) | (7, -0.5, -1) | 34 / 0 / 16 | 96 | (-2, -0.49, -2) | 12.49 |
| 24 | Dark | (-7, 0, -1) | (7, -0.5, 7) | 28 / 0 / 7 | 98 | — | 6.59 |
| 25 | InvisibleWalls | (-7, 0, -1) | (7, -0.5, -5) | 4 / 98 / 0 | 98 | — | 4.95 |
| 26 | Unlock + Dark | (-5, 0, 7) | (7, -0.5, 7) | 33 / 0 / 15 | 96 | (-6, -0.49, -6) | 11.71 |
| 27 | Unlock + InvisibleWalls | (-7, 0, 3) | (7, -0.5, 7) | 4 / 94 / 0 | 94 | (4, -0.49, 6) | 13.13 |
| 28 | Normal | (-7, 0, 7) | (7, -0.5, 5) | 35 / 0 / 15 | 98 | — | 7.95 |
| 29 | Unlock | (5, 0, 5) | (7, -0.5, -1) | 32 / 0 / 16 | 96 | (-6, -0.49, -6) | 10.39 |
| 30 | Dark | (-7, 0, 7) | (7, -0.5, -1) | 29 / 0 / 15 | 98 | — | 6.51 |
| 31 | InvisibleWalls | (1, 0, 1) | (7, -0.5, 1) | 4 / 98 / 0 | 98 | — | 6.77 |
| 32 | Unlock + Dark | (-3, 0, 7) | (7, -0.5, -5) | 34 / 0 / 15 | 96 | (-6, -0.49, -6) | 16.38 |
| 33 | Unlock + InvisibleWalls | (1, 0, 7) | (7, -0.5, 1) | 4 / 94 / 0 | 94 | (6, -0.49, -6) | 11.47 |
| 34 | Normal | (7, 0, -7) | (-5, -0.5, -3) | 30 / 0 / 11 | 98 | — | 8.02 |
| 35 | Unlock | (-1, 0, -1) | (7, -0.5, 1) | 30 / 0 / 11 | 96 | (-6, -0.49, 6) | 10.18 |
| 36 | Dark | (-7, 0, 3) | (7, -0.5, -1) | 35 / 0 / 16 | 98 | — | 5.55 |
| 37 | InvisibleWalls | (7, 0, -1) | (7, -0.5, 1) | 4 / 98 / 0 | 98 | — | 6.11 |
| 38 | Unlock + Dark | (-5, 0, -1) | (7, -0.5, -5) | 30 / 0 / 13 | 96 | (-2, -0.49, 4) | 12.19 |
| 39 | Unlock + InvisibleWalls | (-7, 0, 1) | (7, -0.5, -5) | 4 / 94 / 0 | 94 | (-6, -0.49, -6) | 9.91 |
| 40 | Normal | (7, 0, 5) | (-3, -0.5, -3) | 31 / 0 / 11 | 94 | — | 6.9 |
| 41 | Unlock | (-5, 0, -1) | (7, -0.5, -1) | 27 / 0 / 10 | 96 | (-2, -0.49, 4) | 11.78 |
| 42 | Dark | (7, 0, -1) | (3, -0.5, 3) | 31 / 0 / 22 | 98 | — | 7.05 |
| 43 | InvisibleWalls | (-1, 0, -3) | (5, -0.5, 5) | 4 / 98 / 0 | 98 | — | 8.75 |
| 44 | Unlock + Dark | (-7, 0, 7) | (7, -0.5, 1) | 25 / 0 / 10 | 96 | (6, -0.49, -6) | 15.87 |
| 45 | Unlock + InvisibleWalls | (-3, 0, 7) | (7, -0.5, -3) | 4 / 94 / 0 | 94 | (1, -0.49, -2) | 9.99 |
| 46 | Normal | (-7.5, 0, -5.5) | (7.5, -0.5, 2.5) | 122 / 0 / 61 | 225 | — | 6.56 |
| 47 | Normal | (-2.5, 0, 7.5) | (-4.5, -0.5, -2.5) | 123 / 0 / 58 | 225 | — | 14.68 |
| 48 | Normal | (7.5, 0, -4.5) | (2.5, -0.5, -7.5) | 125 / 0 / 62 | 226 | — | 15.06 |
| 49 | Normal | (6.5, 0, 2.5) | (-4.5, -0.5, 2.5) | 128 / 0 / 63 | 225 | — | 13.72 |
| 50 | Normal | (-4.5, 0, 7.5) | (4.5, -0.5, -5.5) | 115 / 0 / 53 | 225 | — | 17.92 |

¹ Scene-resolved catalog anchor. Level 50 is 17.92 in its own scene, but Boot has 1.5 and the wrong build ID; do not silently import Boot’s Level 50 entry.

## Exact instructions by level

### Level 1

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 2

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 3

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 4

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 5

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 6

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 7

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 8

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 9

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 10

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 11

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 12

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 13

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 14

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 15

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 16

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 17

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole.  
  
Click to Start

### Level 18

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 19

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 20

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 21

**levelGoalGryo**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

### Level 22

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 23

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 24

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 25

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 26

**levelGoalGryo**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

### Level 27

**levelGoalGryo**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

### Level 28

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 29

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 30

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 31

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 32

**levelGoalGryo**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

### Level 33

**levelGoalGryo**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

### Level 34

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 35

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 36

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 37

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 38

**levelGoalGryo**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

### Level 39

**levelGoalGryo**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start  
Click to Start

### Level 40

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 41

**levelGoalGryo**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Stay in large ring for 2 seconds to unlock the hole.  
  
Tap to Start

### Level 42

**levelGoalGryo**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to navigate to the hole.  
  
Tap to Start

### Level 43

**levelGoalGryo**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalTouch**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Navigate your way through the invisible maze.  
  
Tap to Start

### Level 44

**levelGoalGryo**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: use the spotlight to the unlock ring before finding the hole.  
  
Tap to Start

### Level 45

**levelGoalGryo**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalTouch**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

**levelGoalKeyboard**

Goal: Find your way through the invisible maze to the large ring to unlock the hole.  
  
Tap to Start

### Level 46

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 47

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 48

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 49

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start

### Level 50

**levelGoalGryo**

Goal: Tilt the device to move the marble to the hole.  
  
Tap to Start

**levelGoalTouch**

Goal:Drag finger to move the marble to the hole  
  
Tap to Start

**levelGoalKeyboard**

Goal: Use the arrow keys or WASD keys to move the marble to the hole  
  
Click to Start
