# Introduction
Battle boats is a turn based strategy game where players eliminate their opponents fleet of boats by ‘firing’ at a location on a grid in an attempt to sink them. The first player to sink all of their opponents’ battle boats is declared the winner.

Each player has two eight by eight grids. One grid is used for their own battle boats and the other is used to record any hits or misses placed on their opponents. At the beginning of the game, players decide where they wish to place their fleet of five battle boats.

During game play, players take it in turns to fire at a location on their opponent’s board. They do this by stating the coordinates for their target. If a player hits their opponent's boat then this is recorded as a hit. If they miss then this is recorded as a miss.

The game ends when a player's fleet of boats have been sunk. The winner is the player with boats remaining at the end of the game.
# Objectives
	- From the main menu:
		- A settings menu with formatting options:
			- Display the two grids next to each other or one above the other
			- Display gridlines or not
		- New game
			- Ask for the name
				- If the game already exists ask whether to:
					- Overwrite the save
					- Continue the save
					- Cancel and return to the main menu
				- Allow the user to place ships
				- Place ships randomly on the computer grid
				- Start playing the game
		- Continue
			- Prompt for game name
			- Load game and continue
		- Quit
	- When playing the game:
		- Display both the computer and player boards
		- Allow the player to navigate the board with arrow keys
		- When the player presses space, they should fire at the tile the cursor is on if they have not done so already, and the computer should then play
		- If there are boats longer than 1 tile, the computer should look for any partly sunk boats and fire adjacent to them
# Design
The project is split across multiple files for organisation:
- Program.cs - Main logic and sequencing
- Board.cs - Classes for managing game boards
	- Mask struct: Wrapper around a long that uses each bit to represent one cell of a board
	- Board class: Class that stores the state of a board including the layout of each ship and what tiles have been hit
- Input.cs - Functions and classes for managing input
	- Static Input class
		- IntOption function: Ask user for an option from 1 to a specified maximum
		- GridInput class: Display a grid and allow the user to move around it with arrow keys
- Game.cs - Global settings and attributes
	- string file
	- Board player, computer
	- Static class Settings
- Text.cs - Text and templates that will be displayed
# Technical Solution
See attached zip file containing code
# Testing
|                                           Objective                                            |                              Case                               |                                                       Expected                                                        |                  Result                  |
|:----------------------------------------------------------------------------------------------:|:---------------------------------------------------------------:|:---------------------------------------------------------------------------------------------------------------------:|:----------------------------------------:|
|                     Main menu asks the user to choose an item from 1 to 4                      |                   Number from 1 to 4 entered                    |                                           Program goes to the correct menu                                            |      !(main-menu-valid-input.png)[main-menu-valid-input.png]      |
|                                                                                                |                Number below 1 or above 4 entered                |                                                 Program rejects input                                                 |    !(main-menu-invalid-integer.png)[./main-menu-invalid-integer.png]    |
|                                                                                                |                       Non-number entered                        |                                                 Program rejects input                                                 |      !(main-menu-non-integer.png)[./main-menu-non-integer.png]      |
|                 Settings menu allows settings to be changed and are persistent                 |                Settings menu entered and exited                 |                                               Default values displayed                                                |          !(settings-menu.png)[./settings-menu.png]          |
|                                                                                                |                 1 or 2 entered on settings menu                 |                                           Setting changes and is displayed                                            |        !(settings-changed.png)[./settings-changed.png]         |
|                                                                                                |             Settings menu exited and entered again              |                                               Settings remain the same                                                |       !(settings-persistent.png)[./settings-persistent.png]       |
|        New game can be created and if the game exists it can be overwritten or resumed         |            Name entered which does not already exist            |                                                Ship placement launched                                                |     !(new-game-does-not-exist.png)[./new-game-does-not-exist.png]     |
|                                                                                                |              Existing name entered, then option 3               |             Program states that the game already exists, then after asking what to do, the user enters 3              |         !(new-game-exists.png)[./new-game-exists.png]         |
|                                                                                                |              Existing name entered, then option 2               |                                           Program jumps to playing the game                                           |    !(new-game-exists-continue.png)[./new-game-exists-continue.png]     |
|                                                                                                |              Existing name entered, then option 1               |                             Program deletes existing file, goes to ship placement dialog                              |    !(new-game-exists-overwrite.png)[./new-game-exists-overwrite.png]    |
|           Boats can be placed of any size but cannot overlap or extend off the edge            |           Boat placed normally starting in the center           | Marker shown on grid to indicate boat start, and cursor locked to either the same column or row as the start position |   !(ship-placement-normal-ship.gif)[./ship-placement-normal-ship.gif]    |
|                                                                                                | Boat attempted to be placed starting from over an existing boat |                                             Boat placement will not start                                             |  !(ship-placement-start-on-ship.gif)[./ship-placement-start-on-ship.gif]   |
|                                                                                                | Boat started in valid location but extended over existing boat  |                              Program will not allow cursor to move over an existing boat                              | !(ship-placement-extend-over-ship.gif)[./ship-placement-extend-over-ship.gif] |
| When playing the game, the player and computer should take turns firing at a tile of the board |           Player starts by covering one row at a time           |        Computer shoots randomly until it hits a ship, then shoots around it trying to find any adjacent ships         |      !(play-game-computer-ai.gif)[./play-game-computer-ai.gif]      |
|                                                                                                |                                                                 |                                                                                                                       |                                          |
