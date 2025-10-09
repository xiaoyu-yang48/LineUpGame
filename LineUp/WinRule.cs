using System.Collections.Generic;

namespace LineUp
{
    // Win checking and board state checks
    public class WinRule
    {
        public int WinLen { get; }
        public WinRule(int winLen) { WinLen = winLen; }

        public bool CheckCellWin(int[,] board, int rows, int cols, int row, int col)
        {
            int player = board[row, col];
            if (player == 0) return false;

            int count = 1;
            int r = row - 1;
            while (r >= 0 && board[r, col] == player) { count++; r--; }
            r = row + 1;
            while (r < rows && board[r, col] == player) { count++; r++; }
            if (count >= WinLen) return true;

            count = 1;
            int c = col - 1;
            while (c >= 0 && board[row, c] == player) { count++; c--; }
            c = col + 1;
            while (c < cols && board[row, c] == player) { count++; c++; }
            if (count >= WinLen) return true;

            count = 1;
            r = row - 1; c = col - 1;
            while (r >= 0 && c >= 0 && board[r, c] == player) { count++; r--; c--; }
            r = row + 1; c = col + 1;
            while (r < rows && c < cols && board[r, c] == player) { count++; r++; c++; }
            if (count >= WinLen) return true;

            count = 1;
            r = row - 1; c = col + 1;
            while (r >= 0 && c < cols && board[r, c] == player) { count++; r--; c++; }
            r = row + 1; c = col - 1;
            while (r < rows && c >= 0 && board[r, c] == player) { count++; r++; c--; }
            if (count >= WinLen) return true;

            return false;
        }

        public void WinCheck(int[,] board, int rows, int cols, int currentPlayer, List<(int r, int c)> changedDisc, out bool curWin, out bool oppWin)
        {
            curWin = false;
            oppWin = false;
            if (changedDisc == null || changedDisc.Count == 0) return;

            int cur = currentPlayer;
            int opp = (currentPlayer == 1) ? 2 : 1;

            foreach (var (r, c) in changedDisc)
            {
                if (r < 0 || r >= rows || c < 0 || c >= cols) continue;
                int owner = board[r, c];
                if (owner == 0) continue;
                if (CheckCellWin(board, rows, cols, r, c))
                {
                    if (owner == cur) curWin = true;
                    else if (owner == opp) oppWin = true;
                    if (curWin && oppWin) return;
                }
            }
        }

        public bool IsBoardFull(int[,] board, int rows, int cols)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[rows - 1, j] == 0) return false;
            }
            return true;
        }
    }
}
