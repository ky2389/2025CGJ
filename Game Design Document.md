# Museum Night Game Design Document

## Core Concept
A 2D top-down grid-based puzzle game inspired by "Night at the Museum". Players act as museum curators who must return all exhibits to their original positions before dawn.

## Basic Mechanics

### Grid System
- Game played on a grid-based map
- All movement is tile-based (one tile per turn)
- Both player and exhibits move simultaneously each turn, and cannot collide with each other, or the wall

### Exhibit Behavior
- At game start, all exhibits are in their correct positions
- Each exhibit has a fixed movement pattern (e.g., move right 3 tiles, then left 3 tiles, repeat)
- When player pushes an exhibit, it resumes its original pattern from the new position next turn, continuing from its current step in the pattern (not resetting)
- Exhibits's collision - if exhibits will be colliding next round whether both due to behavior pattern or caused by one being pushed by player, they will collide and the game will be lost! This includes any chain reaction collisions caused by player actions.
- Wall collision - if exhibit's moving pattern is blocked by the border wall of the game or any other built-in wall bricks, it will stop there for as much turns as needed, but still keeps its behavior pattern and continues trying the same step until it can move

### Player Mechanics
- Player can move up/down/left/right one tile per turn with WASD
- Player can push the one exhibit in the direction he moves
- Player push actions occur simultaneously with exhibit movements each turn
- Exhibits block player movement - if exhibit is adjacent, player can only move in remaining directions. But if player pushing an exhibit will make other exhibits hit the player next turn, the player will have priority while other exhibits will stay at the same place for that turn, just like when facing a wall
- Player acts as museum curator trying to restore order

### Win Condition
- All exhibits must return to their starting positions (as set at the beginning of each level)
- Must be completed within a fixed number of turns per level
- Victory achieved only when all exhibits are in correct positions at the end of the final turn

### Lose Conditions
- Any exhibit-to-exhibit collision occurs (including chain reaction collisions caused by player actions)
- Time runs out before all exhibits return to starting positions

### Power-ups/Tools (Pending, don't do for now)
1. **Lights On**: All exhibits freeze for several turns, only player can move
2. **Freeze Spell**: Target one specific exhibit to freeze for several turns
3. **Flashlight**: Exhibits within player's radius cannot move
4. **Barriers**: Block exhibit movement in specific directions, altering their movement patterns

## Technical Requirements
- Unity 6.0 implementation
- Turn-based movement system (both the player and exhibit move simultaneously each turn, after player choose direction)
- Grid-based collision detection
- Exhibit AI with pattern-based movement (maintaining step counters)
- Player interaction system (push mechanics)
- Turn counter and win/lose conditions
- Collision detection for all possible scenarios including chain reactions

## Theme
"Everything is alive" - exhibits come to life at night and move according to their nature, player must restore museum order before dawn.