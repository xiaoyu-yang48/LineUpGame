using System;

namespace LineUp
{
    // Structural board representation (not yet wired to GameEngine)
    public class Board
    {
        public int Rows { get; }
        public int Cols { get; }

        // Owner matrix (0 = empty, 1/2 = players)
        public int[,] Owners { get; }

        // Disc kind matrix for structural usage
        public DiscKind[,] DiscKinds { get; }

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Owners = new int[rows, cols];
            DiscKinds = new DiscKind[rows, cols];
        }
    }
}
