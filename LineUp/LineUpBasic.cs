namespace LineUp
{
    public sealed class LineUpBasic : Game
    {
        public override string Name => "LineUpBasic";

        // keep minimal responsibilities; win/board checks are in WinRule, AI is in AIStrategy
    }
}
