using System;
using System.Collections.Generic;
using System.IO;

namespace LineUp
{
    public static class ConsoleGame
    {
        private const string SAVE_DIRECTORY = "SavedGames";
        private const string DEFAULT_SAVE_FILE = "game_save.json";

        public static void Start()
        {
            // Ensure save directory exists
            if (!Directory.Exists(SAVE_DIRECTORY))
            {
                Directory.CreateDirectory(SAVE_DIRECTORY);
            }

            Console.WriteLine("Welcome to Line Up!");
            Console.WriteLine("=====================================\n");
            
            GameEngine engine = null;
            bool isVsComputer = false;

            // Main menu - check for existing saves and let user decide
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1 - Start a new game");
                Console.WriteLine("2 - Load saved game");
                Console.WriteLine("3 - Exit");
                Console.Write("Your choice: ");
                
                var choice = Console.ReadLine()?.Trim();
                
                if (choice == "1")
                {
                    // Start new game - first set up board size
                    Console.WriteLine("\n=== NEW GAME SETUP ===");
                    var (rows, cols, winLen) = SetBoardSize();
                    Console.WriteLine($"\nBoard configured: {rows} x {cols}, Win Length = {winLen}");
                    
                    // Then select game mode
                    while (true)
                    {
                        Console.WriteLine("\nSelect game mode:");
                        Console.WriteLine("1 - Human vs Human (PvP)");
                        Console.WriteLine("2 - Human vs Computer (PvE)");
                        Console.Write("Your choice: ");
                        
                        var gameMode = Console.ReadLine()?.Trim();
                        if (gameMode == "1")
                        {
                            isVsComputer = false;
                            Console.WriteLine("Mode: Human vs Human selected.\n");
                            break;
                        }
                        else if (gameMode == "2")
                        {
                            isVsComputer = true;
                            Console.WriteLine("Mode: Human vs Computer selected.\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input, please enter 1 or 2.");
                        }
                    }
                    
                    // Create the game engine with the selected parameters
                    engine = new GameEngine(rows, cols, winLen, isVsComputer);
                    break;
                }
                else if (choice == "2")
                {
                    // Load saved game
                    engine = LoadGame();
                    if (engine != null)
                    {
                        isVsComputer = engine.IsVsComputer;
                        Console.WriteLine("\n=== GAME LOADED ===");
                        Console.WriteLine($"Board: {engine.Rows} x {engine.Cols}, Win Length: {engine.WinLen}");
                        Console.WriteLine($"Mode: {(isVsComputer ? "Human vs Computer" : "Human vs Human")}");
                        Console.WriteLine($"Current Player: {engine.CurrentPlayer}\n");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Failed to load game. Please try again or start a new game.\n");
                    }
                }
                else if (choice == "3")
                {
                    Console.WriteLine("\nThanks for playing! Goodbye.");
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.\n");
                }
            }

            // Check if user wants to input a sequence of moves
            Console.WriteLine("\nWould you like to:");
            Console.WriteLine("1 - Play interactively (normal mode)");
            Console.WriteLine("2 - Input a sequence of moves (e.g., 'O4,O5,M3,B6')");
            Console.Write("Your choice: ");
            
            var playMode = Console.ReadLine()?.Trim();
            
            if (playMode == "2")
            {
                // Execute sequence mode
                bool continueAfterSequence = ExecuteSequence(engine);
                
                if (!continueAfterSequence)
                {
                    // Game ended or user chose not to continue
                    Console.WriteLine("\nGame Over! Thanks for playing!");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
                // If continueAfterSequence is true, fall through to interactive play
            }
            
            // Main game loop (interactive mode)
            PrintBoard(engine);

            while (true)
            {
                // Check for save/load commands
                Console.WriteLine("\n[Type 'SAVE' to save game, 'LOAD' to load game, or continue playing]");
                
                var selectedType = ReadDiscType(engine);
                
                int colInput = 0;

                while (true)
                {
                    Console.WriteLine($"Player {engine.CurrentPlayer}, enter a column to drop your disc (or 'SAVE'/'LOAD'):");
                    try
                    {
                        var input = Console.ReadLine()?.Trim();
                        
                        // Check for save/load commands
                        if (input?.ToUpper() == "SAVE")
                        {
                            SaveGame(engine);
                            continue;
                        }
                        else if (input?.ToUpper() == "LOAD")
                        {
                            var loadedEngine = LoadGame();
                            if (loadedEngine != null)
                            {
                                engine = loadedEngine;
                                isVsComputer = engine.IsVsComputer;
                                Console.WriteLine("Game loaded successfully!\n");
                                PrintBoard(engine);
                            }
                            continue;
                        }
                        
                        colInput = int.Parse(input ?? "0");
                        if (colInput <= 0 || colInput > engine.Cols)
                            throw new ArgumentOutOfRangeException($"Your chosen column must be within the range: 1 to {engine.Cols}");
                        break;
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("Your input was null.");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Your input was not a valid integer.");
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("Your number is too big or small");
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid input");
                    }
                }
                int col = colInput - 1;

                //try to drop a disc
                if (!engine.DropDisc(col, selectedType, out int placedRow))
                {
                    Console.WriteLine("Invalid move.");
                    continue;
                }

                //apply disc special effects
                var changed = new List<(int r, int c)>();
                if (selectedType != GameEngine.DiscType.Ordinary) PrintBoard(engine);
                

                engine.ApplyDiscEffect(placedRow, col, out changed);
                PrintBoard(engine);
               

                //wincheck
                engine.WinCheck(changed, out bool curWin, out bool oppWin);
                int cur = engine.CurrentPlayer;
                int opp = (engine.CurrentPlayer == 1) ? 2 : 1;
                if (curWin && !oppWin)
                {
                    Console.WriteLine($"Player {cur} wins!");
                    break;
                }
                //check if current player's move leads to opponent winning
                else if (oppWin && !curWin) 
                {
                    Console.WriteLine($"Player {opp} wins!");
                    break;
                }
                else if (curWin && oppWin)
                {
                    Console.WriteLine($"Players {cur} and {opp} both aligned this turn. It's a draw!");
                    break;
                }

                //check if the board is all full
                if (engine.IsBoardFull())
                {
                    PrintBoard(engine);
                    Console.WriteLine("No place to drop more discs. Game Draw.");
                    break;
                }

                //switch to the other player's turn
                engine.SwitchPlayer();

                if (engine.IsVsComputer && engine.CurrentPlayer == 2)
                {
                    int botCol;
                    GameEngine.DiscType botType;

                    if (!engine.FindWinningMove(out botCol, out botType))
                    {
                        if (!engine.RandomMove(out botCol, out botType))
                        {
                            Console.WriteLine("Computer: No valid move. Game draw.");
                            break;
                        }
                    }

                    if (!engine.DropDisc(botCol, botType, out int botPlacedRow))
                    {
                        Console.WriteLine("Computer: Unexpected no valid move");
                        break;
                    }

                    if (botType != GameEngine.DiscType.Ordinary) PrintBoard(engine);
                    engine.ApplyDiscEffect(botPlacedRow, botCol, out List<(int r, int c)> botChanged);
                    PrintBoard(engine);

                    engine.WinCheck(botChanged, out bool curWin2, out bool oppWin2);
                    int cur2 = engine.CurrentPlayer;
                    int opp2 = (engine.CurrentPlayer == 1) ? 2 : 1;
                    if (curWin2 && !oppWin2)
                    {
                        Console.WriteLine($"Player {cur2} wins!");
                        break;
                    }
                    //check if current player's move leads to opponent winning
                    else if (oppWin2 && !curWin2)
                    {
                        Console.WriteLine($"Player {opp2} wins!");
                        break;
                    }
                    else if (curWin2 && oppWin2)
                    {
                        Console.WriteLine($"Players {cur2} and {opp2} both aligned this turn. It's a draw!");
                        break;
                    }

                    //check if the board is all full
                    if (engine.IsBoardFull())
                    {
                        PrintBoard(engine);
                        Console.WriteLine("No place to drop more discs. Game Draw.");
                        break;
                    }

                    //switch to the other player's turn
                    engine.SwitchPlayer();
                    continue;
                }
            }
            
            Console.WriteLine("\nGame Over! Thanks for playing!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static GameEngine.DiscType ReadDiscType(GameEngine engine)
        {
            while (true)
            {
                var p = (engine.CurrentPlayer == 1) ? engine.Player1 : engine.Player2;
                Console.WriteLine($"Player {engine.CurrentPlayer}, your discs: ordinary = {p.OrdinaryDiscs}, magnetic = {p.MagneticDiscs}, boring = {p.BoringDiscs}.");
                Console.WriteLine($"Select your disc type: (O = ordinary, M = magnetic, B = boring). ");

                try
                {
                    var typeInfo = Console.ReadLine()?.Trim()?.ToUpper();
                    
                    // Check for save/load commands
                    if (typeInfo == "SAVE")
                    {
                        SaveGame(engine);
                        continue;
                    }
                    else if (typeInfo == "LOAD")
                    {
                        Console.WriteLine("Cannot load during disc selection. Please select a disc type.");
                        continue;
                    }
                    
                    if (typeInfo == "O")
                    {
                        return GameEngine.DiscType.Ordinary;
                    }
                    if (typeInfo == "M")
                    {
                        return GameEngine.DiscType.Magnetic;
                    }
                    if (typeInfo == "B")
                    {
                        return GameEngine.DiscType.Boring;
                    }
                    Console.WriteLine("Invalid Type.");
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("Your input was null.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Your input format was not valid.");
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input");
                }
            }
        }
        
        private static (int rows, int cols, int winLen) SetBoardSize()
        {
            const int minRows = 6;
            const int minCols = 7;
            int rows = 0;
            int cols = 0;

            Console.WriteLine("\n--- Board Configuration ---");
            Console.WriteLine($"Minimum size: {minRows} rows x {minCols} columns");
            Console.WriteLine("Note: Rows cannot exceed columns\n");

            while (true)
            {
                try
                {
                    Console.Write($"Enter number of rows (>= {minRows}): ");
                    var rowInput = Console.ReadLine()?.Trim();
                    rows = int.Parse(rowInput ?? "0");
                    
                    Console.Write($"Enter number of columns (>= {minCols}): ");
                    var colInput = Console.ReadLine()?.Trim();
                    cols = int.Parse(colInput ?? "0");

                    if (rows < minRows)
                        throw new ArgumentOutOfRangeException($"Rows must be at least {minRows}");
                    if (cols < minCols)
                        throw new ArgumentOutOfRangeException($"Columns must be at least {minCols}");
                    if (rows > cols)
                        throw new ArgumentOutOfRangeException("Number of rows cannot exceed number of columns");

                    break;
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("Your input was null.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Your input was not a valid integer.");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("Your number is too big or small");
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid input");
                }
            }

            int winLen = (int)(rows * cols * 0.1);
            return (rows, cols, winLen);
        }

        private static void PrintBoard(GameEngine engine)
        { 
            int[,] board = engine.GetBoard();
            var types = engine.GetBoardType();

            for (int i = engine.Rows -1; i>=0; i--)
            {
                for (int j = 0; j < engine.Cols; j++)
                {
                    char discSymbol;

                    if (board[i, j] == 1) discSymbol = '@';
                    else if (board[i, j] == 2) discSymbol = '#';
                    else discSymbol = ' ';

                    //check if it is special disc
                    if (board[i, j] == 1)
                    {
                        if (types[i, j] == GameEngine.DiscType.Magnetic)
                        {
                            discSymbol = 'M';
                        }
                        if (types[i, j] == GameEngine.DiscType.Boring)
                        {
                            discSymbol = 'B';
                        }
                    }

                    if (board[i, j] == 2)
                    {
                        if (types[i, j] == GameEngine.DiscType.Magnetic)
                        {
                            discSymbol = 'm';
                        }
                        if (types[i, j] == GameEngine.DiscType.Boring)
                        {
                            discSymbol = 'b';
                        }
                    }

                    Console.Write($"|{discSymbol}");
                }
                Console.WriteLine("|");
            }
            for (int j = 1; j <= engine.Cols; j++)
            {
                Console.Write($" {j}");
            }
            Console.WriteLine();
        }

        private static void SaveGame(GameEngine engine)
        {
            try
            {
                Console.WriteLine("\nSaving game...");
                Console.WriteLine("Enter save file name (or press Enter for default):");
                var fileName = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = DEFAULT_SAVE_FILE;
                }
                else if (!fileName.EndsWith(".json"))
                {
                    fileName += ".json";
                }
                
                string savePath = Path.Combine(SAVE_DIRECTORY, fileName);
                DataSave.Save(engine, savePath);
                Console.WriteLine($"Game saved successfully to {savePath}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game: {ex.Message}");
            }
        }

        private static GameEngine LoadGame()
        {
            try
            {
                // List available save files
                var saveFiles = Directory.GetFiles(SAVE_DIRECTORY, "*.json");
                
                if (saveFiles.Length == 0)
                {
                    Console.WriteLine("No saved games found.");
                    return null;
                }
                
                Console.WriteLine("\nAvailable save files:");
                for (int i = 0; i < saveFiles.Length; i++)
                {
                    var fileInfo = new FileInfo(saveFiles[i]);
                    Console.WriteLine($"{i + 1}. {fileInfo.Name} (Modified: {fileInfo.LastWriteTime})");
                }
                
                Console.WriteLine("\nEnter the number of the save file to load (or 0 to cancel):");
                var input = Console.ReadLine()?.Trim();
                
                if (!int.TryParse(input, out int choice) || choice < 0 || choice > saveFiles.Length)
                {
                    Console.WriteLine("Invalid choice.");
                    return null;
                }
                
                if (choice == 0)
                {
                    return null;
                }
                
                string loadPath = saveFiles[choice - 1];
                Console.WriteLine($"Loading game from {Path.GetFileName(loadPath)}...");
                
                var engine = DataSave.Load(loadPath);
                return engine;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        private static bool ExecuteSequence(GameEngine engine)
        {
            Console.WriteLine("\n=== SEQUENCE INPUT MODE ===");
            Console.WriteLine("Enter a sequence of moves separated by commas.");
            Console.WriteLine("Format: [DiscType][Column] where:");
            Console.WriteLine("  DiscType: O (Ordinary), M (Magnetic), B (Boring)");
            Console.WriteLine("  Column: 1 to " + engine.Cols);
            Console.WriteLine("Example: O4,O5,M3,B6");
            Console.WriteLine("Enter 'SKIP' to play interactively instead.\n");
            
            while (true)
            {
                Console.Write("Enter sequence: ");
                var sequenceInput = Console.ReadLine()?.Trim();
                
                if (sequenceInput?.ToUpper() == "SKIP")
                {
                    Console.WriteLine("Starting interactive mode...\n");
                    return true; // Continue to interactive play
                }
                
                if (string.IsNullOrWhiteSpace(sequenceInput))
                {
                    Console.WriteLine("Invalid input. Please enter a sequence or 'SKIP' to play interactively.");
                    continue;
                }
                
                // Parse and validate the sequence
                var moves = ParseSequence(sequenceInput, engine);
                if (moves == null || moves.Count == 0)
                {
                    Console.WriteLine("Invalid sequence format. Please try again.");
                    continue;
                }
                
                // Execute the sequence
                Console.WriteLine($"\nExecuting {moves.Count} moves...\n");
                bool continueAfter = ExecuteMoves(engine, moves);
                
                return continueAfter;
            }
        }
        
        private static List<(GameEngine.DiscType type, int column)> ParseSequence(string sequence, GameEngine engine)
        {
            var moves = new List<(GameEngine.DiscType, int)>();
            
            try
            {
                // Split by comma and trim each move
                var moveParts = sequence.Split(',');
                
                foreach (var move in moveParts)
                {
                    var trimmedMove = move.Trim().ToUpper();
                    
                    if (trimmedMove.Length < 2)
                    {
                        Console.WriteLine($"Invalid move format: '{move}' (too short)");
                        return null;
                    }
                    
                    // Parse disc type
                    char discChar = trimmedMove[0];
                    GameEngine.DiscType discType;
                    
                    switch (discChar)
                    {
                        case 'O':
                            discType = GameEngine.DiscType.Ordinary;
                            break;
                        case 'M':
                            discType = GameEngine.DiscType.Magnetic;
                            break;
                        case 'B':
                            discType = GameEngine.DiscType.Boring;
                            break;
                        default:
                            Console.WriteLine($"Invalid disc type: '{discChar}' in move '{move}'");
                            return null;
                    }
                    
                    // Parse column number
                    string columnStr = trimmedMove.Substring(1);
                    if (!int.TryParse(columnStr, out int column))
                    {
                        Console.WriteLine($"Invalid column number: '{columnStr}' in move '{move}'");
                        return null;
                    }
                    
                    if (column < 1 || column > engine.Cols)
                    {
                        Console.WriteLine($"Column {column} out of range (1-{engine.Cols}) in move '{move}'");
                        return null;
                    }
                    
                    moves.Add((discType, column));
                }
                
                return moves;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing sequence: {ex.Message}");
                return null;
            }
        }
        
        private static bool ExecuteMoves(GameEngine engine, List<(GameEngine.DiscType type, int column)> moves)
        {
            int moveNumber = 0;
            
            foreach (var (discType, column) in moves)
            {
                moveNumber++;
                Console.WriteLine($"\n--- Move {moveNumber}: Player {engine.CurrentPlayer} plays {discType} disc in column {column} ---");
                
                // Check if player has the disc type
                var currentPlayer = (engine.CurrentPlayer == 1) ? engine.Player1 : engine.Player2;
                if (!currentPlayer.Has(discType))
                {
                    Console.WriteLine($"ERROR: Player {engine.CurrentPlayer} doesn't have any {discType} discs left!");
                    Console.WriteLine($"Available discs - Ordinary: {currentPlayer.OrdinaryDiscs}, Magnetic: {currentPlayer.MagneticDiscs}, Boring: {currentPlayer.BoringDiscs}");
                    return false;
                }
                
                // Try to drop the disc
                int col = column - 1; // Convert to 0-based index
                if (!engine.DropDisc(col, discType, out int placedRow))
                {
                    Console.WriteLine($"ERROR: Cannot place disc in column {column}. Column may be full.");
                    return false;
                }
                
                // Apply disc effects
                var changed = new List<(int r, int c)>();
                if (discType != GameEngine.DiscType.Ordinary)
                {
                    Console.WriteLine($"Applying {discType} disc effect...");
                }
                engine.ApplyDiscEffect(placedRow, col, out changed);
                
                // Print board after each move
                PrintBoard(engine);
                
                // Check for win
                engine.WinCheck(changed, out bool curWin, out bool oppWin);
                int cur = engine.CurrentPlayer;
                int opp = (engine.CurrentPlayer == 1) ? 2 : 1;
                
                if (curWin && !oppWin)
                {
                    Console.WriteLine($"\n*** Player {cur} WINS! ***");
                    return false; // Game ended, don't continue
                }
                else if (oppWin && !curWin)
                {
                    Console.WriteLine($"\n*** Player {opp} WINS! ***");
                    return false; // Game ended, don't continue
                }
                else if (curWin && oppWin)
                {
                    Console.WriteLine($"\n*** Players {cur} and {opp} both aligned this turn. It's a DRAW! ***");
                    return false; // Game ended, don't continue
                }
                
                // Check if board is full
                if (engine.IsBoardFull())
                {
                    Console.WriteLine("\n*** Board is full. Game ends in a DRAW! ***");
                    return false; // Game ended, don't continue
                }
                
                // Switch to next player
                engine.SwitchPlayer();
                
                // Add a small delay for better visualization
                System.Threading.Thread.Sleep(500);
            }
            
            Console.WriteLine($"\n=== Sequence completed successfully! ===");
            Console.WriteLine($"Current state: Player {engine.CurrentPlayer}'s turn");
            
            // Ask if user wants to continue playing interactively
            Console.WriteLine("\nWould you like to continue playing interactively? (Y/N)");
            var continueChoice = Console.ReadLine()?.Trim()?.ToUpper();
            
            return continueChoice == "Y";
        }
        
    }

}