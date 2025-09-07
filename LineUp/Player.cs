using System;

namespace LineUp
{
    public class Player
    {
        public int Id { get; }
        public int BoringDiscs { get; set; }
        public int DrillDiscs { get; set; }
        public int MagneticDiscs { get; set; }

        public Player(int id, int totalDiscs) 
        { 
            Id = id;
            DrillDiscs = 2;
            MagneticDiscs = 2;
            BoringDiscs = totalDiscs / 2 - DrillDiscs - MagneticDiscs;
        }

        public void SetStock(int boringNum,int magneticNum, int drillNum)
        {
            BoringDiscs = boringNum;
            MagneticDiscs = magneticNum;
            DrillDiscs = drillNum;
        }

        public bool Has(GameEngine.DiscType type)
        {
            switch (type)
            {
                case GameEngine.DiscType.Boring:
                    return BoringDiscs > 0;
                case GameEngine.DiscType.Drill:
                    return DrillDiscs > 0;
                case GameEngine.DiscType.Magnetic:
                    return MagneticDiscs > 0;
                default:
                    return false;
            }
        }
        public void Consume(GameEngine.DiscType type)
        {
            switch (type)
            {
                case GameEngine.DiscType.Boring:
                    BoringDiscs--;
                    break;
                case GameEngine.DiscType.Drill:
                    DrillDiscs--;
                    break;
                case GameEngine.DiscType.Magnetic:
                    MagneticDiscs--;
                    break;
            }
        }

        public void ReturnDisc (int count = 1)
        {
            BoringDiscs += count;
        }
    }
}
