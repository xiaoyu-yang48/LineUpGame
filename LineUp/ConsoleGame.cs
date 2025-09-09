using System;
using System.Collections.Generic;
using System.IO;

namespace LineUp
{
    public static class ConsoleGame
    {
        private const string SaveDirectory = "SavedGames";
        private const string DefaultSaveFile = "SavedGame.json";
        public static void Start()
        {
            //ensure save directory exists
            if (!Directory.Exists(SaveDirectory))
            { 
                Directory.CreateDirectory(SaveDirectory);
            }

            Console.WriteLine("Welcome to Line Up!");
            GameEngine engine = null;
            
            //main menu
            while (true)
            {
                Console.WriteLine("Enter 1 = start a new game; 2 = load saved game; 3 = exit");
                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    //start new game, set up board size
                    var (rows, cols, winLen) = SetBoardSize();
                    Console.WriteLine($"Your game board is {rows} * {cols}, WinLen = {winLen}");

                    //select game mode
                    bool vsComputer = ReadGameMode();

                    //create game engine
                    engine = new GameEngine(rows, cols, winLen, vsComputer);
                    break;
                }
                else if (choice == "2")
                {
                    //load saved game
                    engine = LoadGame();
                    if (engine != null)
                    {
                        Console.WriteLine("Game loaded successfully!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Failed to load game.");
                    }
                }
                else if (choice == "3")
                {
                    Console.WriteLine("Thanks for playing! See you next time");
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
                }
            }
            

            // Main game loop
            PrintBoard (engine);

            while (true)
            {
                Console.WriteLine("Type 'SAVE' to save game, 'LOAD' to load game, otherwise continue playing.");
                var selectedType = ReadDiscType(engine);
                int colInput = 0;
                bool restartTurn = false;

                while (true)
                {
                    Console.WriteLine($"Player {engine.CurrentPlayer}, enter a column to drop your disc (or 'SAVE'/'LOAD'):");
                    try
                    {
                        var input = Console.ReadLine()?.Trim();

                        //check if save or load
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
                                Console.WriteLine("Game loaded successfully!");
                                PrintBoard(engine);
                            }
                            restartTurn = true;
                            break;
                        }

                        colInput = int.Parse(input ?? "0");
                        if (colInput <= 0 || colInput > engine.Cols)
                        {
                            throw new ArgumentOutOfRangeException($"Your chosen column must be within the range: 1 to {engine.Cols}");
                        }
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
                if (restartTurn) continue;
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

            Console.WriteLine("Thanks for playing! Press any key to exit");
            Console.ReadKey();
        }

        //functions used in game loop
        private static GameEngine.DiscType ReadDiscType(GameEngine engine)
        {
            while (true)
            {
                var p = (engine.CurrentPlayer == 1) ? engine.Player1 : engine.Player2;
                Console.WriteLine($"Player {engine.CurrentPlayer}, your discs: ordinary = {p.OrdinaryDiscs}, magnetic = {p.MagneticDiscs}, boring = {p.BoringDiscs}.");
                Console.WriteLine($"Select your disc type: (O = ordinary, M = magnetic, B = boring). ");

                try
                {
                    var typeInfo = Console.ReadLine()?.Trim().ToUpperInvariant();
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

        private static bool ReadGameMode()
        {
            while (true)
            {
                Console.WriteLine("Select game mode as 1 = Human vs Human or 2 = Human vs Computer.");
                var gameMode = Console.ReadLine()?.Trim();
                if (gameMode == "1")
                {
                    return false;
                }
                else if (gameMode == "2")
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid input, please enter 1 or 2.");
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
                    rows = int.Parse(Console.ReadLine()?.Trim());
                    Console.WriteLine($"Please enter your board columns: (>= {minCols}), and rows <= columns");
                    cols = int.Parse(Console.ReadLine()?.Trim());

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
                Console.WriteLine("Saving game... \nEnter file name or press enter for default:");
                var fileName = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(fileName)) fileName = DefaultSaveFile;
                else if (!fileName.EndsWith(".json")) fileName += ".json";

                string savePath = Path.Combine(SaveDirectory, fileName);
                DataSave.Save(engine, savePath);
                Console.WriteLine($"Game saved to {savePath}");
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Failed to save game: {e.Message}");
            }
        }

        private static GameEngine LoadGame()
        {
            try
            {
                //list available saved files
                var savedFiles = Directory.GetFiles(SaveDirectory, "*.json");

                if (savedFiles.Length == 0)
                {
                    Console.WriteLine("No saved games found.");
                    return null;
                }

                Console.WriteLine("Available saved files:");
                for (int i = 0; i < savedFiles.Length; i++)
                { 
                    var fileName = new FileInfo(savedFiles[i]);
                    Console.WriteLine(($"{i + 1}, {fileName.Name}"));
                }

                Console.WriteLine("Enter the number of the saved file to load: ");
                var input = Console.ReadLine()?.Trim();
                int choice = int.Parse(input ?? "0");

                if (choice < 1 || choice > savedFiles.Length)
                {
                    Console.WriteLine("Invalid choice.");
                    return null;
                }

                string loadPath = savedFiles[choice - 1];
                Console.WriteLine($"Loading game from {Path.GetFileName(loadPath)}");

                var engine = DataSave.Load(loadPath);
                return engine;
            }
            catch (Exception e) 
            { 
                Console.WriteLine($"Failed to load game: {e.Message}");
                return null;
            }
        }
    }

}