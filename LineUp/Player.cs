using System;

namespace LineUp
{
    public abstract class Player
    {
        public int Id { get; }
        public int OrdinaryDiscs { get; set; }
        public int BoringDiscs { get; set; }
        public int MagneticDiscs { get; set; }

        protected Player(int id, int totalDiscs) 
        { 
            Id = id;
            BoringDiscs = 2;
            MagneticDiscs = 2;
            OrdinaryDiscs = totalDiscs / 2 - BoringDiscs - MagneticDiscs;
        }

        public void SetStock(int ordinaryNum,int magneticNum, int boringNum)
        {
            OrdinaryDiscs = ordinaryNum;
            MagneticDiscs = magneticNum;
            BoringDiscs = boringNum;
        }

        public bool Has(GameEngine.DiscType type)
        {
            switch (type)
            {
                case GameEngine.DiscType.Ordinary:
                    return OrdinaryDiscs > 0;
                case GameEngine.DiscType.Boring:
                    return BoringDiscs > 0;
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
                case GameEngine.DiscType.Ordinary:
                    OrdinaryDiscs--;
                    break;
                case GameEngine.DiscType.Boring:
                    BoringDiscs--;
                    break;
                case GameEngine.DiscType.Magnetic:
                    MagneticDiscs--;
                    break;
            }
        }

        public void ReturnDisc (int count = 1)
        {
            OrdinaryDiscs += count;
        }
    }

    public sealed class HumanPlayer : Player
    {
        public HumanPlayer(int id, int totalDiscs) : base(id, totalDiscs) { }
    }

    public sealed class ComputerPlayer : Player
    {
        public ComputerPlayer(int id, int totalDiscs) : base(id, totalDiscs) { }
    }
}
