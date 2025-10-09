using System;
using System.Collections.Generic;
using System.IO;

namespace LineUp
{
    public sealed class LineUpClassic : Game
    {
        public override string Name => "LineUpClassic";

        // --- Moved from GameEngine ---
        private int[,] Board;
        private DiscType[,] BoardType;
        public int Rows { get; }
        public int Cols { get; }
        public int WinLen { get; }

        public Player Player1 { get; }
        public Player Player2 { get; }
        public int CurrentPlayer { get; private set; } = 1;

        // rules and AI strategy
        private WinRule winRule;

        // computer mode
        public bool IsVsComputer { get; }
        private readonly Random rand = new Random();
        private AIStrategy aiStrategy;

        public LineUpClassic(int rows, int cols, int winLen, bool isVsComputer = false)
        {
            Rows = rows;
            Cols = cols;
            WinLen = winLen;
            IsVsComputer = isVsComputer;
            Board = new int[Rows, Cols];
            BoardType = new DiscType[Rows, Cols];
            Player1 = new HumanPlayer(1, rows * cols);
            Player2 = isVsComputer ? new ComputerPlayer(2, rows * cols) : new HumanPlayer(2, rows * cols);
            winRule = new WinRule(WinLen);
            if (IsVsComputer)
            {
                aiStrategy = new SimpleAIStrategy();
            }
        }

        public int[,] GetBoard() => Board;
        public DiscType[,] GetBoardType() => BoardType;

        public enum DiscType
        {
            Ordinary, Magnetic, Boring
        }

        private Player GetCurrent() => (CurrentPlayer == 1) ? Player1 : Player2;

        // change and restore (memento hooks)
        private bool recording = false;
        private int[,] backupBoard;
        private DiscType[,] backupBoardType;
        private int bakCP;
        private int p1O, p1M, p1B, p2O, p2M, p2B;

        public void BeginRecord()
        {
            recording = true;
            backupBoard = (int[,])Board.Clone();
            backupBoardType = (DiscType[,])BoardType.Clone();
            bakCP = CurrentPlayer;
            p1O = Player1.OrdinaryDiscs; p1M = Player1.MagneticDiscs; p1B = Player1.BoringDiscs;
            p2O = Player2.OrdinaryDiscs; p2M = Player2.MagneticDiscs; p2B = Player2.BoringDiscs;
        }

        public void RollBack()
        {
            if (!recording) return;
            Board = (int[,])backupBoard.Clone();
            BoardType = (DiscType[,])backupBoardType.Clone();
            CurrentPlayer = bakCP;
            Player1.SetStock(p1O, p1M, p1B);
            Player2.SetStock(p2O, p2M, p2B);
            recording = false;
            backupBoard = null;
            backupBoardType = null;
        }

        // basic game funcs
        public bool DropDisc(int col, DiscType type, out int placedRow)
        {
            placedRow = -1;
            if (col < 0 || col >= Cols) return false;

            int targetRow = -1;
            for (int i = 0; i < Rows; i++)
            {
                if (Board[i, col] == 0)
                {
                    targetRow = i;
                    break;
                }
            }
            if (targetRow == -1) return false;

            var p = GetCurrent();
            if (!p.Has(type)) return false;
            p.Consume(type);

            Board[targetRow, col] = CurrentPlayer;
            BoardType[targetRow, col] = type;
            placedRow = targetRow;
            return true;
        }

        public void ApplyDiscEffect(int row, int col, out List<CellChange> changedDisc)
        {
            changedDisc = new List<CellChange>();
            var type = BoardType[row, col];
            int owner = Board[row, col];

            if (owner != 0)
            {
                // ordinary disc
                if (type == DiscType.Ordinary)
                {
                    changedDisc.Add(new CellChange(row, col));
                    return;
                }
                // boring disc effect
                if (type == DiscType.Boring)
                {
                    int countP1 = 0, countP2 = 0;
                    for (int i = 0; i < Rows; i++)
                    {
                        if (i != row && Board[i, col] != 0)
                        {
                            if (Board[i, col] == 1) countP1++;
                            else if (Board[i, col] == 2) countP2++;
                        }

                        Board[i, col] = 0;
                        BoardType[i, col] = DiscType.Ordinary;
                    }

                    Player1.ReturnDisc(countP1);
                    Player2.ReturnDisc(countP2);

                    Board[0, col] = owner;
                    BoardType[0, col] = DiscType.Ordinary;
                    changedDisc.Add(new CellChange(0, col));
                    return;
                }

                // magnetic disc effect
                if ((type == DiscType.Magnetic))
                {
                    changedDisc.Add(new CellChange(row, col));

                    if (row == 0 || (row > 0 && Board[row - 1, col] == owner))
                    {
                        BoardType[row, col] = DiscType.Ordinary;
                        return;
                    }
                    for (int i = row - 2; i >= 0; i--)
                    {
                        if (Board[i, col] == owner && BoardType[i, col] == DiscType.Ordinary)
                        {
                            (Board[i + 1, col], Board[i, col]) = (Board[i, col], Board[i + 1, col]);
                            (BoardType[i + 1, col], BoardType[i, col]) = (BoardType[i, col], BoardType[i + 1, col]);

                            changedDisc.Add(new CellChange(i + 1, col));
                            changedDisc.Add(new CellChange(i, col));
                            break;
                        }
                    }
                    BoardType[row, col] = DiscType.Ordinary;
                    return;
                }
            }
        }

        public bool CheckCellWin(int row, int col) => winRule.CheckCellWin(Board, Rows, Cols, row, col);

        public void WinCheck(List<CellChange> changedDisc, out bool curWin, out bool oppWin)
        {
            winRule.WinCheck(Board, Rows, Cols, CurrentPlayer, changedDisc, out curWin, out oppWin);
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = (CurrentPlayer == 1) ? 2 : 1;
        }

        public bool IsBoardFull() => winRule.IsBoardFull(Board, Rows, Cols);

        private bool IsColumnPlayable(int col)
        {
            if (col < 0 || col >= Cols) return false;
            return Board[Rows - 1, col] == 0;
        }

        private bool IsDisctypePlayable(DiscType type)
        {
            return GetCurrent().Has(type);
        }

        public bool TryMoveWins(int col, DiscType type)
        {
            if (!IsColumnPlayable(col)) return false;
            if (!GetCurrent().Has(type)) return false;

            int targetRow = -1;
            for (int i = 0; i < Rows; i++)
            {
                if (Board[i, col] == 0)
                {
                    targetRow = i;
                    break;
                }
            }
            if (targetRow == -1) return false;

            BeginRecord();
            try
            {
                if (!DropDisc(col, type, out int placedRow)) return false;
                ApplyDiscEffect(placedRow, col, out List<(int r, int c)> changedDisc);
                if (changedDisc == null) return false;
                WinCheck(changedDisc, out bool curWin, out bool oppWin);
                return curWin && !oppWin;
            }
            finally
            {
                RollBack();
            }
        }

        // AI logic moved to AIStrategy. Keep no-ops or wrappers if needed later.

        // save/load restore state
        public void RestoreState(int[,] board, DiscType[,] boardType, int currentPlayer, (int p1O, int p1M, int p1B) p1, (int p2O, int p2M, int p2B) p2)
        {
            if (board == null || boardType == null) throw new ArgumentNullException("board/boardtype is null");
            if (board.GetLength(0) != Rows || board.GetLength(1) != Cols) throw new ArgumentException("board dimensions mismatch");
            if (boardType.GetLength(0) != Rows || boardType.GetLength(1) != Cols) throw new ArgumentException("boardtype dimensions mismatch");

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Board[r, c] = board[r, c];
                    BoardType[r, c] = boardType[r, c];
                }
            }

            Player1.SetStock(p1.p1O, p1.p1M, p1.p1B);
            Player2.SetStock(p2.p2O, p2.p2M, p2.p2B);

            CurrentPlayer = (currentPlayer == 1) ? 1 : 2;
        }

        // --- Moved from ConsoleGame ---
        private const string SaveDirectory = "SavedGames";
        private const string DefaultSaveFile = "SavedGame.json";

        public static void Start()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            Console.WriteLine("Welcome to Line Up!");
            LineUpClassic engine = null;

            while (true)
            {
                Console.WriteLine("Enter 1 = start a new game; 2 = load saved game; 3 = exit");
                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    var (rows, cols, winLen) = SetBoardSize();
                    Console.WriteLine($"Your game board is {rows} * {cols}, WinLen = {winLen}");
                    bool vsComputer = ReadGameMode();
                    engine = new LineUpClassic(rows, cols, winLen, vsComputer);
                    break;
                }
                else if (choice == "2")
                {
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

            PrintBoard(engine);

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

                if (!engine.DropDisc(col, selectedType, out int placedRow))
                {
                    Console.WriteLine("Invalid move.");
                    continue;
                }

                var changed = new List<CellChange>();
                if (selectedType != LineUpClassic.DiscType.Ordinary) PrintBoard(engine);

                engine.ApplyDiscEffect(placedRow, col, out changed);
                PrintBoard(engine);

                engine.WinCheck(changed, out bool curWin, out bool oppWin);
                int cur = engine.CurrentPlayer;
                int opp = (engine.CurrentPlayer == 1) ? 2 : 1;
                if (curWin && !oppWin)
                {
                    Console.WriteLine($"Player {cur} wins!");
                    break;
                }
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

                if (engine.IsBoardFull())
                {
                    PrintBoard(engine);
                    Console.WriteLine("No place to drop more discs. Game Draw.");
                    break;
                }

                engine.SwitchPlayer();

                if (engine.IsVsComputer && engine.CurrentPlayer == 2)
                {
                    int botCol;
                    LineUpClassic.DiscType botType;
                    if (engine.aiStrategy == null || !engine.aiStrategy.FindMove(engine, out botCol, out botType))
                    {
                        Console.WriteLine("Computer: No valid move. Game draw.");
                        break;
                    }

                    if (!engine.DropDisc(botCol, botType, out int botPlacedRow))
                    {
                        Console.WriteLine("Computer: Unexpected no valid move");
                        break;
                    }

                    if (botType != LineUpClassic.DiscType.Ordinary) PrintBoard(engine);
                    engine.ApplyDiscEffect(botPlacedRow, botCol, out List<CellChange> botChanged);
                    PrintBoard(engine);

                    engine.WinCheck(botChanged, out bool curWin2, out bool oppWin2);
                    int cur2 = engine.CurrentPlayer;
                    int opp2 = (engine.CurrentPlayer == 1) ? 2 : 1;
                    if (curWin2 && !oppWin2)
                    {
                        Console.WriteLine($"Player {cur2} wins!");
                        break;
                    }
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

                    if (engine.IsBoardFull())
                    {
                        PrintBoard(engine);
                        Console.WriteLine("No place to drop more discs. Game Draw.");
                        break;
                    }

                    engine.SwitchPlayer();
                    continue;
                }
            }

            Console.WriteLine("Thanks for playing! Press any key to exit");
            Console.ReadKey();
        }

        // functions used in game loop (moved from ConsoleGame)
        private static LineUpClassic.DiscType ReadDiscType(LineUpClassic engine)
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
                        return LineUpClassic.DiscType.Ordinary;
                    }
                    if (typeInfo == "M")
                    {
                        return LineUpClassic.DiscType.Magnetic;
                    }
                    if (typeInfo == "B")
                    {
                        return LineUpClassic.DiscType.Boring;
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

        private static void PrintBoard(LineUpClassic engine)
        {
            int[,] board = engine.GetBoard();
            var types = engine.GetBoardType();

            for (int i = engine.Rows - 1; i >= 0; i--)
            {
                for (int j = 0; j < engine.Cols; j++)
                {
                    char discSymbol;

                    if (board[i, j] == 1) discSymbol = '@';
                    else if (board[i, j] == 2) discSymbol = '#';
                    else discSymbol = ' ';

                    if (board[i, j] == 1)
                    {
                        if (types[i, j] == LineUpClassic.DiscType.Magnetic)
                        {
                            discSymbol = 'M';
                        }
                        if (types[i, j] == LineUpClassic.DiscType.Boring)
                        {
                            discSymbol = 'B';
                        }
                    }

                    if (board[i, j] == 2)
                    {
                        if (types[i, j] == LineUpClassic.DiscType.Magnetic)
                        {
                            discSymbol = 'm';
                        }
                        if (types[i, j] == LineUpClassic.DiscType.Boring)
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

        private static void SaveGame(LineUpClassic engine)
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

        private static LineUpClassic LoadGame()
        {
            try
            {
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
