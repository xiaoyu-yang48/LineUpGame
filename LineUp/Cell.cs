namespace LineUp
{
    public class Cell
    {
        public int Row { get; }
        public int Col { get; }
        public Disc? Disc { get; set; }
        public bool IsEmpty => Disc == null;

        public Cell(int row, int col)
        {
            Row = row;
            Col = col;
            Disc = null;
        }
    }
}
