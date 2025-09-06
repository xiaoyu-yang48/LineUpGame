using System;

namespace LineUp
{
    public class GameEngine
    {
        private int[,] Board;
        public int Rows { get; }
        public int Cols { get; }
        public int WinLen { get; }
        public int CurrentPlayer { get; private set; } = 1;

        public GameEngine(int rows, int cols, int winLen)
        {
            Rows = rows;
            Cols = cols;
            WinLen = winLen;
            Board = new int[Rows, Cols];
        }

        public int[,] GetBoard()
        {
            return Board;
        }

        public bool DropDisc(int col, out int placedRow)
        {
            placedRow = -1;
            if (col < 0 || col >= Cols) return false;

            for (int i = 0; i < Rows; i++)
            {
                if (Board[i, col] == 0)
                {
                    Board[i, col] = CurrentPlayer;
                    placedRow = i;
                    return true;
                }
            }
            return false;
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