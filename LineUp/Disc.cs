namespace LineUp
{
    // Disc hierarchy (factory pattern placeholder)
    public abstract class Disc
    {
        public abstract DiscKind Kind { get; }
    }

    public enum DiscKind
    {
        Ordinary,
        Magnetic,
        Boring,
        Explosive
    }

    public sealed class OrdinaryDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Ordinary;
    }

    public sealed class BoringDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Boring;
    }

    public sealed class MagneticDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Magnetic;
    }

    public sealed class ExplosiveDisc : Disc
    {
        public override DiscKind Kind => DiscKind.Explosive;
    }

    public static class DiscFactory
    {
        public static Disc Create(DiscKind kind)
        {
            switch (kind)
            {
                case DiscKind.Magnetic: return new MagneticDisc();
                case DiscKind.Boring: return new BoringDisc();
                case DiscKind.Explosive: return new ExplosiveDisc();
                default: return new OrdinaryDisc();
            }
        }
    }
}
