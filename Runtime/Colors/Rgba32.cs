namespace Affdata.Core.Colors
{
    /// <summary>An immutable color with four 8-bit channels.</summary>
    public readonly struct Rgba32
    {
        public Rgba32(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        /// <summary>Channels packed in RRGGBBAA order, independent of machine endianness.</summary>
        public uint PackedRgba => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;

        public static Rgba32 FromPacked(uint rgba)
        {
            return new Rgba32((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)rgba);
        }
    }
}
