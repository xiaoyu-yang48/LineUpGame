namespace LineUp
{
    // Disc hierarchy (factory pattern placeholder)
    public abstract class Disc
    {
        public abstract DiscKind Kind { get; }

        // Owner id (0 = none/unassigned)
        public int DiscOwner { get; }

        protected Disc()
        {
            DiscOwner = 0;
        }

        protected Disc(int ownerId)
        {
            DiscOwner = ownerId;
        }

        // Hook for special behavior when the disc is placed at (row, col)
        public virtual void OnPlaced(Board board, int row, int col) { }
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
        public OrdinaryDisc() : base() { }
        public OrdinaryDisc(int ownerId) : base(ownerId) { }
        public override DiscKind Kind => DiscKind.Ordinary;
    }

    public sealed class BoringDisc : Disc
    {
        public BoringDisc() : base() { }
        public BoringDisc(int ownerId) : base(ownerId) { }
        public override DiscKind Kind => DiscKind.Boring;
    }

    public sealed class MagneticDisc : Disc
    {
        public MagneticDisc() : base() { }
        public MagneticDisc(int ownerId) : base(ownerId) { }
        public override DiscKind Kind => DiscKind.Magnetic;
    }

    public sealed class ExplosiveDisc : Disc
    {
        public ExplosiveDisc() : base() { }
        public ExplosiveDisc(int ownerId) : base(ownerId) { }
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
