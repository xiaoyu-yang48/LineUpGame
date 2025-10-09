using System.Collections.Generic;

namespace LineUp
{
    // Win checking and board state checks
    public class WinRule
    {
        public int WinLen { get; }
        public WinRule(int winLen) { WinLen = winLen; }

        private static int OwnerAt(Board board, int row, int col)
        {
            var disc = board.GetCell(row, col).Disc;
            return disc?.DiscOwner ?? 0;
        }

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

        // Overload: use Board/Cell directly
        public bool CheckCellWin(Board board, Cell cell)
        {
            int rows = board.Rows, cols = board.Cols;
            int row = cell.Row, col = cell.Col;
            int player = OwnerAt(board, row, col);
            if (player == 0) return false;

            int count = 1;
            int r = row - 1;
            while (r >= 0 && OwnerAt(board, r, col) == player) { count++; r--; }
            r = row + 1;
            while (r < rows && OwnerAt(board, r, col) == player) { count++; r++; }
            if (count >= WinLen) return true;

            count = 1;
            int c = col - 1;
            while (c >= 0 && OwnerAt(board, row, c) == player) { count++; c--; }
            c = col + 1;
            while (c < cols && OwnerAt(board, row, c) == player) { count++; c++; }
            if (count >= WinLen) return true;

            count = 1;
            r = row - 1; c = col - 1;
            while (r >= 0 && c >= 0 && OwnerAt(board, r, c) == player) { count++; r--; c--; }
            r = row + 1; c = col + 1;
            while (r < rows && c < cols && OwnerAt(board, r, c) == player) { count++; r++; c++; }
            if (count >= WinLen) return true;

            count = 1;
            r = row - 1; c = col + 1;
            while (r >= 0 && c < cols && OwnerAt(board, r, c) == player) { count++; r--; c++; }
            r = row + 1; c = col - 1;
            while (r < rows && c >= 0 && OwnerAt(board, r, c) == player) { count++; r++; c--; }
            if (count >= WinLen) return true;

            return false;
        }

        public void WinCheck(int[,] board, int rows, int cols, int currentPlayer, List<CellChange> changedCells, out bool curWin, out bool oppWin)
        {
            curWin = false;
            oppWin = false;
            if (changedCells == null || changedCells.Count == 0) return;

            int cur = currentPlayer;
            int opp = (currentPlayer == 1) ? 2 : 1;

            foreach (var change in changedCells)
            {
                int r = change.Row;
                int c = change.Col;
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

        // Overload: WinCheck using Board and Cells; derive current player from first changed cell owner
        public void WinCheck(Board board, List<Cell> changedCells, out bool curWin, out bool oppWin)
        {
            curWin = false;
            oppWin = false;
            if (changedCells == null || changedCells.Count == 0) return;

            int curOwner = 0;
            foreach (var cell in changedCells)
            {
                curOwner = OwnerAt(board, cell.Row, cell.Col);
                if (curOwner != 0) break;
            }
            if (curOwner == 0) return; // nothing to check
            int oppOwner = curOwner == 1 ? 2 : 1;

            foreach (var cell in changedCells)
            {
                int owner = OwnerAt(board, cell.Row, cell.Col);
                if (owner == 0) continue;
                if (CheckCellWin(board, cell))
                {
                    if (owner == curOwner) curWin = true;
                    else if (owner == oppOwner) oppWin = true;
                    if (curWin && oppWin) return;
                }
            }
        }
    }
}
