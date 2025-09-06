using System;

namespace LineUp
{
    internal class Player
    {
        public int PlayerId = 1;
        public int PlayerBoringDiscs = int( GameEngine.Rows * GameEngine.Cols / 2);
        public int PlayerDrillDiscs = 2;
        public int PlayerMagneticDiscs = 2;
    }
}
