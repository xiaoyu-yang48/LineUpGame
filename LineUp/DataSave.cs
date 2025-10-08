using System;
using System.IO;
using System.Text.Json;

namespace LineUp
{
    public class DataSave
    {
        private class SavedState
        {
            public int Rows { get; set; }
            public int Cols { get; set; }
            public int WinLen { get; set; }
            public bool IsVsComputer { get; set; }
            public int CurrentPlayer { get; set; }

            public int[][] Board {  get; set; }
            public LineUpClassic.DiscType[][] BoardType { get; set; }

            public int P1O { get; set; }
            public int P1M { get; set; }
            public int P1B { get; set; }
            public int P2O { get; set; }
            public int P2M { get; set; }
            public int P2B { get; set; }
        }

        //save game
        public static void Save(LineUpClassic engine, string path)
        {

            if (engine == null) throw new ArgumentNullException(nameof(engine), "Engine is null");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path), "Invalid file path");

            var state = new SavedState()
            {
                Rows = engine.Rows,
                Cols = engine.Cols,
                WinLen = engine.WinLen,
                IsVsComputer = engine.IsVsComputer,
                CurrentPlayer = engine.CurrentPlayer,

                Board = ToJagged(engine.GetBoard()),
                BoardType = ToJagged(engine.GetBoardType()),

                P1O = engine.Player1.OrdinaryDiscs,
                P1M = engine.Player1.MagneticDiscs,
                P1B = engine.Player1.BoringDiscs,

                P2O = engine.Player2.OrdinaryDiscs,
                P2M = engine.Player2.MagneticDiscs,
                P2B = engine.Player2.BoringDiscs,
            };

            //saving format
            var options = new JsonSerializerOptions { WriteIndented = true};
            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(path, json);
        }
        private static int[][] ToJagged(int[,] src)
        { 
            int rows = src.GetLength(0), cols = src.GetLength(1);
            var dst = new int[rows][];

            for (int r = 0; r < rows; r++)
            {
                //set dst length to cols
                dst[r] = new int [cols];

                for (int c = 0; c < cols; c++)
                {
                    dst[r][c] = src[r, c];
                }
            }
            return dst;
        }

        private static LineUpClassic.DiscType[][] ToJagged(LineUpClassic.DiscType[,] src)
        {
            int rows = src.GetLength(0), cols = src.GetLength(1);
            var dst = new LineUpClassic.DiscType[rows][];

            for (int r = 0; r < rows; r++)
            {
                //set dst length to cols
                dst[r] = new LineUpClassic.DiscType[cols];

                for (int c = 0; c < cols; c++)
                {
                    dst[r][c] = src[r, c];
                }
            }
            return dst;
        }

        //load game
        public static LineUpClassic Load(string path)
        {

            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path),"Invalid file path");
            if (!File.Exists(path)) throw new FileNotFoundException("File not found.");

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { };
            var state = JsonSerializer.Deserialize<SavedState>(json, options) ?? throw new InvalidOperationException("Invalid file.");

            var engine = new LineUpClassic(state.Rows, state.Cols, state.WinLen, state.IsVsComputer);

            var board = FromJagged(state.Board);
            var boardType = FromJagged(state.BoardType);

            //restore state
            engine.RestoreState(
                board, boardType, state.CurrentPlayer,
                (state.P1O, state.P1M, state.P1B),
                (state.P2O, state.P2M, state.P2B)
                );

            return engine;
        }

        private static int[,] FromJagged(int[][] src)
        {
            int rows = src.Length;
            int cols = (rows > 0) ? src[0].Length : 0;
            var dst = new int[rows, cols];

            for (int r = 0; r < rows; r++) 
            {
                for (int c = 0; c < cols; c++)
                {
                    dst[r,c] = src[r][c];
                }
            }
            return dst;
        }

        private static LineUpClassic.DiscType[,] FromJagged(LineUpClassic.DiscType[][] src)
        {
            int rows = src.Length;
            int cols = (rows > 0) ? src[0].Length : 0;
            var dst = new LineUpClassic.DiscType[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    dst[r, c] = src[r][c];
                }
            }
            return dst;
        }
    }
}
