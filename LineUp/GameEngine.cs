using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace LineUp
{
    public class GameEngine
    {
        private int[,] Board;
        private DiscType[,] BoardType;
        public int Rows { get; }
        public int Cols { get; }
        public int WinLen { get; }

        public Player Player1 { get; }
        public Player Player2 { get; }
        public int CurrentPlayer { get; private set; } = 1;

        //computer mode
        public bool IsVsComputer { get; }
        //random - computer randoms drop discs
        private readonly Random rand = new Random();

        public GameEngine(int rows, int cols, int winLen, bool isVsComputer = false)
        {
            Rows = rows;
            Cols = cols;
            WinLen = winLen;
            IsVsComputer = isVsComputer;
            Board = new int[Rows, Cols];
            BoardType = new DiscType[Rows, Cols];
            Player1 = new Player(1, rows * cols);
            Player2 = new Player(2, rows * cols);
        }

        public int[,] GetBoard() => Board;

        public DiscType[,] GetBoardType() => BoardType;
        

        public enum DiscType
        {
            Ordinary, Magnetic, Boring
        }

        private Player GetCurrent() => (CurrentPlayer == 1) ? Player1 : Player2;



        //change and restore
        private bool recording = false;
        private int[,] backupBoard;
        private DiscType[,] backupBoardType;
        private int bakCP;
        private int p1O, p1M, p1B, p2O, p2M, p2B;

        public void BeginRecord()
        {
            recording = true;

            //clone current board
            backupBoard = (int[,])Board.Clone();
            backupBoardType = (DiscType[,])BoardType.Clone();

            //backup disc stock and current player
            bakCP = CurrentPlayer;
            p1O = Player1.OrdinaryDiscs; p1M = Player1.MagneticDiscs; p1B = Player1.BoringDiscs;
            p2O = Player2.OrdinaryDiscs; p2M = Player2.MagneticDiscs; p2B = Player2.BoringDiscs;
        }

        public void RollBack()
        {
            if (!recording) return;

            //restore board
            Board = (int[,])backupBoard.Clone();
            BoardType = (DiscType[,])backupBoardType.Clone();

            //restore player and disc stock
            CurrentPlayer = bakCP;
            Player1.SetStock(p1O, p1M, p1B);
            Player2.SetStock(p2O, p2M, p2B);

            recording = false;
            backupBoard = null;
            backupBoardType = null;
        }


        //basic game funcs
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

            //check disc stock
            var p = GetCurrent();
            if (!p.Has(type)) return false;
            p.Consume(type);

            Board[targetRow, col] = CurrentPlayer;
            BoardType[targetRow, col] = type;
            placedRow = targetRow;
            return true;
        }

        public void ApplyDiscEffect(int row, int col, out List <(int r, int c)> changedDisc)
        {
            changedDisc = new List<(int r, int c)>();
            var type = BoardType[row, col];
            int owner = Board[row, col];

            if (owner != 0)
            {
                //ordinary disc
                if (type == DiscType.Ordinary)
                {
                    changedDisc.Add((row, col));
                    return;
                }
                //apply boring disc effect
                if (type == DiscType.Boring)
                {
                    int countP1 =0, countP2 =0;
                    for (int i = 0; i < Rows; i++)
                    {
                        if (i != row && Board[i,col] != 0)
                        {
                            if (Board[i, col] == 1) countP1++;
                            else if (Board[i, col] == 2) countP2++;
                        }

                        Board[i, col] = 0;
                        BoardType [i, col] = DiscType.Ordinary;
                    }

                    Player1.ReturnDisc(countP1);
                    Player2.ReturnDisc(countP2);

                    Board[0, col] = owner;
                    BoardType[0, col] = DiscType.Ordinary;
                    changedDisc.Add((0, col));
                    return;
                }

                //apply magnetic disc effect
                if ((type == DiscType.Magnetic))
                {
                    changedDisc.Add((row, col));

                    //row == 0, no place underneath
                    if (row == 0 || (row > 0 && Board[row - 1, col] == owner))
                    {
                        BoardType[row, col] = DiscType.Ordinary;
                        return;
                    }
                    for (int i = row - 2; i >= 0; i--)
                    {
                        if (Board[i, col] == owner && BoardType[i, col]==DiscType.Ordinary)
                        {
                            (Board[i + 1, col], Board[i, col]) = (Board[i, col], Board[i + 1, col]);
                            (BoardType[i + 1, col], BoardType[i, col]) = (BoardType[i, col], BoardType[i + 1, col]);
                          
                            changedDisc.Add((i+1, col));
                            changedDisc.Add((i, col));
                            break;
                        }
                    }
                    BoardType[row, col] = DiscType.Ordinary;
                    return;
                }
            }
        }

        public bool CheckCellWin (int row, int col)
        {
            int player = Board[row, col];
            if (player == 0) return false;

            //vertical wincheck
            int count = 1;
            int r = row - 1;
            while (r >=0 && Board[r, col] == player)
            {
                count++;
                r--;
            }

            r = row + 1;
            while (r <Rows && Board[r, col] == player)
            {
                count++;
                r++;
            }

            if (count >= WinLen) return true;

            //horizontal wincheck
            count = 1;
            int c = col - 1;
            while (c >= 0 && Board[row, c] == player)
            {
                count++;
                c--;
            }

            c = col + 1;
            while (c < Cols && Board[row, c] == player)
            {
                count++;
                c++;
            }

            if (count >= WinLen) return true;

            //Slash diagonal: top-left to bottom-right
            count = 1;
            r = row - 1;
            c = col - 1;
            while (r >= 0 && c >=0 && Board[r, c] == player)
            {
                count++;
                r--;
                c--;
            }

            r = row + 1;
            c = col + 1;
            while (r < Rows && c < Cols && Board[r, c] == player)
            {
                count++;
                r++;
                c++;
            }

            if (count >= WinLen) return true;

            //Slash diagonal: bottom-left to top-right
            count = 1;
            r = row - 1;
            c = col + 1;
            while(r >= 0 && c < Cols && Board[r, c] == player)
            {
                count++;
                r--;
                c++;
            }

            r = row + 1;
            c = col - 1;
            while(r < Rows && c >= 0 &&  Board[r, c] == player)
            {
                count++;
                r++;
                c--;
            }

            if (count >= WinLen) return true;

            return false;
        }

        public void WinCheck (List<(int r, int c)> changedDisc, out bool curWin, out bool oppWin)
        {
            curWin = false;
            oppWin = false;

            if (changedDisc == null || changedDisc.Count == 0) return;

            int cur = CurrentPlayer;
            int opp = (CurrentPlayer == 1) ? 2 : 1;

            foreach (var (r, c) in changedDisc)
            {
                if (r < 0 || r >= Rows || c < 0 || c >= Cols) continue;
                int owner = Board[r, c];
                if (owner == 0) continue;
                if (CheckCellWin(r, c))
                {
                    if (owner == cur) curWin = true;
                    else if (owner == opp) oppWin = true;

                    if (curWin && oppWin) return;
                }
            }
        }

        public void SwitchPlayer()
        {
            CurrentPlayer = (CurrentPlayer == 1) ? 2 : 1;
        }

        public bool IsBoardFull()
        {
            for (int j = 0; j < Cols; j++)
            {
                if (Board[Rows - 1, j] == 0) return false;
            }
            return true;
        }

        private bool IsColumnPlayable (int col)
        {
            if (col < 0 || col >= Cols) return false;
            return Board[Rows - 1, col]==0;
        }

        private bool IsDisctypePlayable (DiscType type)
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
            if (targetRow == -1) return false; //otherwise valid move exists

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

        public bool FindWinningMove(out int col, out DiscType type)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (!IsColumnPlayable(j)) continue;

                //collect playable disc types
                List<DiscType> playableTypes = new List<DiscType>();
                if (IsDisctypePlayable(DiscType.Ordinary)) playableTypes.Add(DiscType.Ordinary);
                if (IsDisctypePlayable(DiscType.Magnetic)) playableTypes.Add(DiscType.Magnetic);
                if (IsDisctypePlayable(DiscType.Boring)) playableTypes.Add(DiscType.Boring);

                foreach (DiscType t in playableTypes)
                {
                    if (TryMoveWins(j, t))
                    {
                        col = j;
                        type = t;
                        return true;
                    }
                }   
            }

            col = -1;
            type = DiscType.Ordinary;
            return false;
        }

        public bool RandomMove(out int col, out DiscType type)
        {
            var playableCol = new List<int>();
            for (int j = 0; j < Cols; j++)
            {
                if (IsColumnPlayable(j)) playableCol.Add(j);
            }
            if (playableCol.Count == 0)
            {
                col = -1;
                type = DiscType.Ordinary;
                return false;
            }

            //collect playable disc types
            List<DiscType> playableTypes = new List<DiscType>();
            if (IsDisctypePlayable(DiscType.Ordinary)) playableTypes.Add(DiscType.Ordinary);
            if (IsDisctypePlayable(DiscType.Magnetic)) playableTypes.Add(DiscType.Magnetic);
            if (IsDisctypePlayable(DiscType.Boring)) playableTypes.Add(DiscType.Boring);
            if (playableTypes.Count == 0)
            {
                col = -1;
                type = DiscType.Ordinary;
                return false;
            }

            //generate random move
            col = playableCol[rand.Next(playableCol.Count)];
            type = playableTypes[rand.Next(playableTypes.Count)];
            return true;
        }

        //save/load
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

            Player1.SetStock(p1O, p1M, p1B);
            Player2.SetStock(p2O, p2M, p2B);

            CurrentPlayer = (currentPlayer == 1) ? 1 : 2;
        }
    }
}