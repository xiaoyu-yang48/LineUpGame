# Line Up Game

A console-based line-up game with save/load functionality.

## Features

- **Game Modes**: Human vs Human or Human vs Computer
- **Customizable Board**: Choose your own board size (minimum 6x7)
- **Special Discs**: 
  - Ordinary discs: Standard game pieces
  - Magnetic discs: Attract one ordinary disc of the same player upward
  - Boring discs: Clear entire column and drop to bottom
- **Save/Load System**: Save and load games at any time

## How to Build and Run

### Prerequisites
- .NET 8.0 SDK

### Building
```bash
dotnet build
```

### Running
```bash
dotnet run --project LineUp
```

## Save/Load Functionality

### At Game Start
1. When you start the game, you'll see three options:
   - **Load saved game**: Browse and load from existing save files
   - **Start a new game**: Begin a fresh game
   - **Exit**: Quit the application

### During Gameplay
- Type `SAVE` at any prompt to save the current game state
- Type `LOAD` when prompted for column input to load a different game
- Type `SEQ` to switch to sequence input mode
- The game will prompt for a filename when saving (press Enter for default)
- When loading, you'll see a list of available save files with timestamps

### After Game Ends
- The game will ask if you want to save the final state
- This is useful for reviewing completed games later

## Sequence Input Mode

The game now supports inputting a sequence of moves in a single line, making it easy to:
- Test specific game scenarios
- Replay recorded games
- Quickly set up board positions

### How to Use Sequence Input

1. **At game start**: After setting up the game, choose option 2 for sequence input
2. **During gameplay**: Type `SEQ` at any prompt to switch to sequence mode

### Sequence Format

```
[DiscType][Column],[DiscType][Column],...
```

Where:
- **DiscType**: `O` (Ordinary), `M` (Magnetic), `B` (Boring)
- **Column**: Column number from 1 to board width

### Examples

- `O4,O5,M3,B6` - Player 1 plays ordinary disc in column 4, Player 2 plays ordinary in column 5, Player 1 plays magnetic in column 3, Player 2 plays boring in column 6
- `O1,O2,O1,O2,O1,O2,O1` - A sequence creating a vertical line in columns 1 and 2
- `M3,B4,O5,O6` - Mixed disc types across different columns

### Features

- **Automatic player switching**: The sequence alternates between players automatically
- **Visual feedback**: Each move is displayed with the board state
- **Error handling**: Invalid moves or insufficient discs will stop the sequence
- **Continue option**: After sequence completes, you can continue playing interactively
- **Win detection**: The sequence stops if a player wins during execution

## Game Rules

1. Players take turns dropping discs into columns
2. Discs fall to the lowest available position in the column
3. Win by aligning a certain number of your discs (calculated as 10% of board size)
4. Special discs have unique effects when placed
5. The game ends when someone wins or the board is full (draw)

## Save File Location

Save files are stored in the `SavedGames` directory as JSON files. The directory is created automatically when you first run the game.

## Fixed Issues

1. **Fixed infinite loop**: The game now properly handles the initial menu selection
2. **Fixed null reference errors**: Added proper null checks for user input
3. **Fixed character encoding**: Removed non-ASCII characters from comments
4. **Added complete save/load system**: 
   - Load games at startup
   - Save/load during gameplay
   - Multiple save file management
   - Automatic save directory creation

## Code Structure

- `Program.cs`: Entry point
- `ConsoleGame.cs`: Main game loop and UI, including save/load functions
- `GameEngine.cs`: Core game logic and rules
- `Player.cs`: Player state management
- `DataSave.cs`: Save/load file operations

## Testing the Save/Load Feature

1. Start a new game and make a few moves
2. Type `SAVE` when prompted for disc type or column
3. Enter a filename (e.g., "test_game")
4. Exit the game
5. Restart and choose "Load saved game"
6. Select your saved file from the list
7. Continue playing from where you left off