# Space League
Group Members:
Noah Bakayou
Hiệp An Trương
Teague Sangster
Jacon Nouri

## Gameplay Video

[Watch Gameplay Video](https://youtu.be/1wX5wFGBjOw)

## Game Concept

This will be a space-themed game. The game mechanics will revolve around ball play, similar to Rocket League — except the players will control spacecrafts that is fully traversable in a 3D plane. There are 2 teams of 3 players and there are 2 goalposts at the opposite end of the map. To spice things up, objects such as asteroid fields or shipwrecks can be added in the middle of the map, which makes getting the ball from one side to another more difficult. If a player crashes into an asteroid, they will be spawned back at the starting position, potentially losing control of the ball to the other team.

## Weapon Loadout System

Our team would like to implement is the ability for the players to change their weapon loadout before entering the battle, similar to what I did in my 2D project. However, I don't think having players shoot and destroy each other while also having to play ball would work very well. Therefore, I would like to propose some alternative weapons:

- **Grappling hook**: the player have to aim and shoot the hook at the ball. If hit, it will grab and pull the ball toward the player.
- **EMP missile**: aim and shoot at the enemy player. If hit, immobilized the player for a short amount of time.
- **Smoke launcher**: create a large area of smoke that completely obstructs the player's vision inside. Perfect to use in the asteroid field to blind and cause the enemy player to crash.

## Card System

Additionally, we can implement Teague's idea of card system into the game. When a team scores a goal, both teams return to their starting positions. The winning team will receive 1 point to their score counter. The losing team will be able to choose a card to improve their stats. The cards can be one of the followings:

- Increase acceleration & deceleration
- Increase maneuverability
- Increase firing rate of weapons
- Grant a shield in front of goalpost that will stop and deflect ball once

## Core Features (Build First)

Jacob:
- Ball physics (gravity / space-gravity tuning)
- Goal detection + scoring system
- Sound effects (ball hit, scoring, collisions)
- Asteroids placed around the arena (static obstacles)

Noah:
- Networking (sync ships, ball, scoring)
- Menus for joining games/lobbies
- Respawning system/game state management

## Enhancements (After Core Works)
An:
- Improved movement (inertia tuning, drift, braking thrusters)
- Weapon management (switching, cooldowns)
- Blaster mechanics (damage system)
- Shield & health pickups around the arena

Teague:
- Weapon Card System
- New weapons granted after each goal
- Arena cage/barriers to keep ships & ball inside bounds
- Boost pickup around the arena

