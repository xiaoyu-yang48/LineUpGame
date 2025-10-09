using System;

namespace LineUp
{
    // Board with cell grid and helpers
    public class Board
    {
        public int Rows { get; }
        public int Cols { get; }
        public Cell[][] Cells { get; }

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Cells = new Cell[rows][];
            for (int r = 0; r < rows; r++)
            {
                Cells[r] = new Cell[cols];
                for (int c = 0; c < cols; c++)
                {
                    Cells[r][c] = new Cell(r, c);
                }
            }
        }

        public bool InBounds(int row, int col) => row >= 0 && row < Rows && col >= 0 && col < Cols;

        public Cell GetCell(int row, int col)
        {
            if (!InBounds(row, col)) throw new ArgumentOutOfRangeException();
            return Cells[row][col];
        }

        public bool IsLegalMove(int col)
        {
            if (col < 0 || col >= Cols) return false;
            // top-most row index is Rows - 1; legal if top is empty
            return GetCell(Rows - 1, col).IsEmpty;
        }

        // Place a disc into a column; returns placed row or -1
        public int PlaceDisc(int col, Disc disc)
        {
            if (!IsLegalMove(col)) return -1;
            for (int r = 0; r < Rows; r++)
            {
                if (Cells[r][col].Disc == null)
                {
                    Cells[r][col].Disc = disc;
                    return r;
                }
            }
            return -1;
        }

        // Apply gravity to all columns (collapse discs downward)
        public void ApplyGravity()
        {
            for (int c = 0; c < Cols; c++)
            {
                int nextFillRow = 0;
                for (int r = 0; r < Rows; r++)
                {
                    var disc = Cells[r][c].Disc;
                    if (disc != null)
                    {
                        if (nextFillRow != r)
                        {
                            Cells[nextFillRow][c].Disc = disc;
                            Cells[r][c].Disc = null;
                        }
                        nextFillRow++;
                    }
                }
            }
        }

        // Rotate the board 90° clockwise.
        // Mapping: (oldRow, oldCol) -> (newRow, newCol) = (oldCol, Rows - 1 - oldRow)
        // 注意：由于 Rows/Cols 为只读且网格形状固定，只有方阵时才能原地覆盖；非方阵时本方法不改变棋盘。
        public void RotateCW()
        {
            if (Rows != Cols)
            {
                // 非方阵：保留占位行为，不修改当前网格尺寸与数据
                return;
            }

            var rotated = new Disc?[Rows, Cols];

            for (int oldRow = 0; oldRow < Rows; oldRow++)
            {
                for (int oldCol = 0; oldCol < Cols; oldCol++)
                {
                    int newRow = oldCol;
                    int newCol = Rows - 1 - oldRow;
                    rotated[newRow, newCol] = Cells[oldRow][oldCol].Disc;
                }
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    Cells[r][c].Disc = rotated[r, c];
                }
            }
        }
    }
}
