using System.Collections.Generic;

namespace LineUp
{
    // Command pattern placeholder for moves
    public abstract class Move
    {
        public abstract void Execute();
        public abstract void Undo();
        public virtual IReadOnlyList<CellChange> GetChanges() => new List<CellChange>();
    }
}
