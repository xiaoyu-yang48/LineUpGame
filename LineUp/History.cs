using System.Collections.Generic;

namespace LineUp
{
    // Undo/redo history placeholder
    public class History
    {
        private readonly Stack<GameMemento> undoStack = new Stack<GameMemento>();
        private readonly Stack<GameMemento> redoStack = new Stack<GameMemento>();
    }
}
