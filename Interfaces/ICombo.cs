using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static TycheFighters.Program;

namespace TycheFighters
{
    public readonly struct Combo
    {
        private readonly ushort sequence;

        private Combo(ushort sequence)
        {
            this.sequence = sequence;
        }

        public static implicit operator ushort(Combo combo)
        {
            return combo.sequence;
        }

        public static implicit operator Combo(ushort sequence)
        {
            return new Combo(sequence);
        }

        public static bool operator ==(Combo a, ushort sequence) => (Mask(a) & sequence) == a;

        public static bool operator !=(Combo a, ushort sequence) => (Mask(a) & sequence) != a;

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Combo && (Combo)obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
