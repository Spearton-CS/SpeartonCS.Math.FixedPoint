namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_UUUU_UUUU_UUUU_UUFF
/// </summary>
public readonly struct U56F8Format : IFxPFormat<ulong>
{
    public static int IBits => 56;
    public static int FBits => 8;
    public static bool ISigned => false;

    public static ulong IMaxValue => 0x00FF_FFFF_FFFF_FFFF;
    public static ulong IMinValue => ulong.MinValue;

    public static ulong FMaxValue => byte.MaxValue;
    public static ulong FMinValue => byte.MinValue;

    public static ulong IPartMask => 0xFFFF_FFFF_FFFF_FF00;
    public static ulong FPartMask => 0x0000_0000_0000_00FF;
}