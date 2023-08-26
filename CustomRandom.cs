namespace TycheFighters
{
    public class Random
    {
        private uint seed = 0;
        private uint weyl = 0;
        private const uint offset = 0xda1ce2a9;

        public Random()
        {
            seed = (uint)DateTime.Now.Microsecond;
        }

        public ushort Next(ushort max = ushort.MaxValue)
        {
            seed *= seed; seed += (weyl += offset); return (ushort)((seed = (seed >> 16) | (seed << 16)) % max);
        }
    }
}