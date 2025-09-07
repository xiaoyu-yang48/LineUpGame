using System;

namespace LineUp
{
    public static class ConsoleGame
    {
        public static void Start()
        {
            bool isVsComputer = false;
            while (true)
            {
                Console.WriteLine("Welcome to Line Up! Select game mode as 1 = Human vs Human or 2 = Human vs Computer.");
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

            // Game logic goes here

            var (rows, cols, winLen) = SetBoardSize();
            Console.WriteLine($"Your game board is {rows} * {cols}, WinLen = {winLen}");
            var engine = new GameEngine(rows, cols, winLen, isVsComputer);
            PrintBoard(engine);

            while (true)
            {
                var selectedType = ReadDiscType(engine);
                int colInput = 0;

                while (true)
                {
                    Console.WriteLine($"Player {engine.CurrentPlayer}, enter a column to drop your disc:");
                    try
                    {
                        colInput = int.Parse(Console.ReadLine());
                        if (colInput <= 0 || colInput > cols)
                            throw new ArgumentOutOfRangeException($"Your chosen column must be within the range: 1 to {cols}");
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
                int newRow = -1;
                int specialRow = -1;
                int opponentRow = -1;
                if (selectedType == GameEngine.DiscType.Boring)
                {
                    newRow = placedRow;
                    PrintBoard(engine);
                }
                else
                {
                    PrintBoard(engine);
                    engine.ApplyDiscEffect(placedRow, col, out newRow, out specialRow, out opponentRow);
                    PrintBoard(engine);
                }

                //check if current player win the game
                int cur = engine.CurrentPlayer;
                int opp = (cur == 1) ? 2 : 1;
                var board = engine.GetBoard();

                bool curWin = engine.WinCheck(newRow, col) || (specialRow >= 0 && board [specialRow,col] == cur && engine.WinCheck(specialRow, col));
                bool oppWin = opponentRow >= 0 && board[opponentRow, col] == opp && engine.WinCheck(opponentRow, col);

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
            }
        }

        private static GameEngine.DiscType ReadDiscType(GameEngine engine)
        {
            while (true)
            {
                var p = (engine.CurrentPlayer == 1) ? engine.Player1 : engine.Player2;
                Console.WriteLine($"Player {engine.CurrentPlayer}, your discs: boring = {p.BoringDiscs}, magnetic = {p.MagneticDiscs}, drill = {p.DrillDiscs}.");
                Console.WriteLine($"Select your disc type: (1 = boring, 2 = magnetic, 3 = drill). ");

                var typeInfo = Console.ReadLine();
                if (typeInfo == "1")
                {
                    return GameEngine.DiscType.Boring;
                }
                if (typeInfo == "2")
                {
                    return GameEngine.DiscType.Magnetic;
                }
                if (typeInfo == "3")
                {
                    return GameEngine.DiscType.Drill;
                }
                Console.WriteLine("Invalid Type.");
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
                        if (types[i, j] == GameEngine.DiscType.Drill)
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
                        if (types[i, j] == GameEngine.DiscType.Drill)
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
    }

}