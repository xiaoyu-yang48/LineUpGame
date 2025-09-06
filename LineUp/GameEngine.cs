using System;

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

        public GameEngine(int rows, int cols, int winLen)
        {
            Rows = rows;
            Cols = cols;
            WinLen = winLen;
            Board = new int[Rows, Cols];
            BoardType = new DiscType[Rows, Cols];
            Player1 = new Player(1, rows * cols);
            Player2 = new Player(2, rows * cols);
        }

        public int[,] GetBoard()
        {
            return Board;
        }

        public DiscType[,] GetBoardType()
        {
            return BoardType;
        }

        public enum DiscType
        {
            Boring, Magnetic, Drill
        }

        private Player GetCurrent()
        {
            return (CurrentPlayer == 1) ? Player1 : Player2;
        }

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

        public void ApplyDiscEffect(int row, int col, out int newRow)
        {
            newRow = row;
            var type = BoardType[row, col];
            int owner = Board[row, col];
            if (owner != 0)
            {
                //apply drill disc effect
                if (type == DiscType.Drill)
                {
                    int countP1 =0, countP2 =0;
                    for (int i = 0; i < Rows; i++)
                    {
                        if (i != row && Board[i,col] != 0)
                        {
                            if (Board[i, col] == 1) countP1++;
                            if (Board[i, col] == 2) countP2++;
                        }

                        Board[i, col] = 0;
                        BoardType [i, col] = DiscType.Boring;
                    }

                    Player1.ReturnDisc(countP1);
                    Player2.ReturnDisc(countP2);

                    Board[0, col] = owner;
                    BoardType[0, col] = DiscType.Boring;
                    newRow = 0;
                    return;
                }

                //apply magnetic disc effect
                if ((type == DiscType.Magnetic))
                {
                    for (int i = row - 2; i >= 0; i--)
                    {
                        if (Board[i, col] == owner && BoardType[i, col]==DiscType.Boring)
                        {
                            (Board[i + 1, col], Board[i, col]) = (Board[i, col], Board[i + 1, col]);
                            (BoardType[i + 1, col], BoardType[i, col]) = (BoardType[i, col], BoardType[i + 1, col]);
                            break;
                        }
                    }
                    BoardType[row, col] = DiscType.Boring;
                    newRow = row;
                    return;
                }
            }
        }

        public bool WinCheck (int row, int col)
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

            //Slash ¨L ¨J
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

            //Slash ¨K ¨I
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

    }
}