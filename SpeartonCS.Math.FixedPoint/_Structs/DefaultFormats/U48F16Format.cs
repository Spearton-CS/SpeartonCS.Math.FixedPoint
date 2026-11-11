namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0xUUUU_UUUU_UUUU_FFFF
/// </summary>
public readonly struct U48F16Format : IFxPFormat<ulong>
{
    public static int IBits => 48;
    public static int FBits => 16;
    public static bool ISigned => false;

    public static ulong IMaxValue => 0x0000_FFFF_FFFF_FFFF;
    public static ulong IMinValue => ulong.MinValue;

    public static ulong FMaxValue => ushort.MaxValue;
    public static ulong FMinValue => ushort.MinValue;

    public static ulong IPartMask => 0xFFFF_FFFF_FFFF_0000;
    public static ulong FPartMask => 0x0000_0000_0000_FFFF;
}