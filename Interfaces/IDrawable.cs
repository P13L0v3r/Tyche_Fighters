namespace TycheFighters
{
    public interface IDrawable
    {
        public ushort position { get; set; }
        public List<Triangle> Draw();
    }

    public interface IInstance : IDrawable
    {
        public byte age { get; set; }
        public byte lifeSpan { get; set; }
        public bool IsDead();
    }
}
