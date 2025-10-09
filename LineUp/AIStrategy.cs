using System;
using System.Collections.Generic;

namespace LineUp
{
    // AI strategy interface and a simple implementation
    public abstract class AIStrategy
    {
        public abstract bool FindMove(LineUpClassic engine, out int col, out LineUpClassic.DiscType type);
    }

    public sealed class SimpleAIStrategy : AIStrategy
    {
        public override bool FindMove(LineUpClassic engine, out int col, out LineUpClassic.DiscType type)
        {
            // Build playable columns
            int rows = engine.Rows;
            int cols = engine.Cols;
            var board = engine.GetBoard();
            var playableCols = new List<int>();
            for (int j = 0; j < cols; j++)
            {
                if (board[rows - 1, j] == 0) playableCols.Add(j);
            }
            if (playableCols.Count == 0)
            {
                col = -1; type = LineUpClassic.DiscType.Ordinary; return false;
            }

            // Build playable disc types from current player's stock
            var current = engine.CurrentPlayer == 1 ? engine.Player1 : engine.Player2;
            var playableTypes = new List<LineUpClassic.DiscType>();
            if (current.Has(LineUpClassic.DiscType.Ordinary)) playableTypes.Add(LineUpClassic.DiscType.Ordinary);
            if (current.Has(LineUpClassic.DiscType.Magnetic)) playableTypes.Add(LineUpClassic.DiscType.Magnetic);
            if (current.Has(LineUpClassic.DiscType.Boring)) playableTypes.Add(LineUpClassic.DiscType.Boring);
            if (playableTypes.Count == 0)
            {
                col = -1; type = LineUpClassic.DiscType.Ordinary; return false;
            }

            // Try to find immediate winning move
            foreach (int j in playableCols)
            {
                foreach (var t in playableTypes)
                {
                    if (engine.TryMoveWins(j, t))
                    {
                        col = j; type = t; return true;
                    }
                }
            }

            // Fallback: random choice
            var rand = new Random();
            col = playableCols[rand.Next(playableCols.Count)];
            type = playableTypes[rand.Next(playableTypes.Count)];
            return true;
        }
    }
}
