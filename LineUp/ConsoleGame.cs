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

            // Check for existing saves and let user decide
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1 - Load saved game");
                Console.WriteLine("2 - Start a new game");
                Console.WriteLine("3 - Exit");
                Console.Write("Your choice: ");
                
                var choice = Console.ReadLine()?.Trim();
                
                if (choice == "1")
                {
                    engine = LoadGame();
                    if (engine != null)
                    {
                        isVsComputer = engine.IsVsComputer;
                        Console.WriteLine("\nGame loaded successfully!");
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
                else if (choice == "2")
                {
                    // Start new game - get game mode
                    while (true)
                    {
                        Console.WriteLine("\nSelect game mode:");
                        Console.WriteLine("1 - Human vs Human");
                        Console.WriteLine("2 - Human vs Computer");
                        Console.Write("Your choice: ");
                        
                        var gameMode = Console.ReadLine()?.Trim();
                        if (gameMode == "1")
                        {
                            isVsComputer = false;
                            break;
                        }
                        else if (gameMode == "2")
                        {
                            isVsComputer = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input, please enter 1 or 2.");
                        }
                    }
                    
                    // Set up board size
                    var (rows, cols, winLen) = SetBoardSize();
                    Console.WriteLine($"\nYour game board is {rows} x {cols}, Win Length = {winLen}\n");
                    engine = new GameEngine(rows, cols, winLen, isVsComputer);
                    break;
                }
                else if (choice == "3")
                {
                    Console.WriteLine("Thanks for playing! Goodbye.");
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.\n");
                }
            }

            // Main game loop
            PrintBoard(engine);
            bool gameEnded = false;

            while (!gameEnded)
            {
                // Check for save/load commands
                Console.WriteLine("\n[Type 'SAVE' to save game, 'LOAD' to load game, or continue playing]");
                
                var selectedType = ReadDiscType(engine, ref gameEnded);
                if (gameEnded) break;
                
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
                    gameEnded = true;
                    break;
                }
                //check if current player's move leads to opponent winning
                else if (oppWin && !curWin) 
                {
                    Console.WriteLine($"Player {opp} wins!");
                    gameEnded = true;
                    break;
                }
                else if (curWin && oppWin)
                {
                    Console.WriteLine($"Players {cur} and {opp} both aligned this turn. It's a draw!");
                    gameEnded = true;
                    break;
                }

                //check if the board is all full
                if (engine.IsBoardFull())
                {
                    PrintBoard(engine);
                    Console.WriteLine("No place to drop more discs. Game Draw.");
                    gameEnded = true;
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
                            gameEnded = true;
                            break;
                        }
                    }

                    if (!engine.DropDisc(botCol, botType, out int botPlacedRow))
                    {
                        Console.WriteLine("Computer: Unexpected no valid move");
                        gameEnded = true;
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
                        gameEnded = true;
                        break;
                    }
                    //check if current player's move leads to opponent winning
                    else if (oppWin2 && !curWin2)
                    {
                        Console.WriteLine($"Player {opp2} wins!");
                        gameEnded = true;
                        break;
                    }
                    else if (curWin2 && oppWin2)
                    {
                        Console.WriteLine($"Players {cur2} and {opp2} both aligned this turn. It's a draw!");
                        gameEnded = true;
                        break;
                    }

                    //check if the board is all full
                    if (engine.IsBoardFull())
                    {
                        PrintBoard(engine);
                        Console.WriteLine("No place to drop more discs. Game Draw.");
                        gameEnded = true;
                        break;
                    }

                    //switch to the other player's turn
                    engine.SwitchPlayer();
                    continue;
                }
            }
            
            // Ask if player wants to save before exiting
            Console.WriteLine("\nGame ended. Would you like to save the final state? (Y/N)");
            var saveChoice = Console.ReadLine()?.Trim()?.ToUpper();
            if (saveChoice == "Y")
            {
                SaveGame(engine);
            }
            
            Console.WriteLine("\nThanks for playing! Press any key to exit...");
            Console.ReadKey();
        }

        private static GameEngine.DiscType ReadDiscType(GameEngine engine, ref bool gameEnded)
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

            while (true)
            {
                try
                {
                    Console.WriteLine($"Please enter your board rows: (>= {minRows})");
                    var rowInput = Console.ReadLine()?.Trim();
                    rows = int.Parse(rowInput ?? "0");
                    
                    Console.WriteLine($"Please enter your board columns: (>= {minCols}), and rows <= columns");
                    var colInput = Console.ReadLine()?.Trim();
                    cols = int.Parse(colInput ?? "0");

                    if (rows < minRows)
                        throw new ArgumentOutOfRangeException($"Rows must be >= {minRows}");
                    if (cols < minCols)
                        throw new ArgumentOutOfRangeException($"Columns must be >= {minCols}");
                    if (rows > cols)
                        throw new ArgumentOutOfRangeException("Rows cannot exceed columns.");

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
    }

}