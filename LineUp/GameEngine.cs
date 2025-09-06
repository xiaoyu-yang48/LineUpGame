using System;

namespace LineUp
{
    public class GameEngine
    {
        private int[,] Board;
        public int Rows { get; }
        public int Cols { get; }
        public int WinLen { get; }

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
        
    }
}