namespace LineUp
{
    // Holds a changed cell position and ownership/type info if needed
    public class CellChange
    {
        public int Row { get; }
        public int Col { get; }
        public CellChange(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
