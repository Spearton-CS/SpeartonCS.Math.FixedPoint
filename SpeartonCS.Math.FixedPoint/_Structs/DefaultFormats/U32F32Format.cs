namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0xUUUU_UUUU_FFFF_FFFF
/// </summary>
public readonly struct U32F32Format : IFxPFormat<ulong>
{
    public static int IBits => 32;
    public static int FBits => 32;
    public static bool ISigned => true;

    public static ulong IMaxValue => uint.MaxValue;
    public static ulong IMinValue => uint.MinValue;

    public static ulong FMaxValue => uint.MaxValue;
    public static ulong FMinValue => uint.MinValue;

    public static ulong IPartMask => 0xFFFF_FFFF_0000_0000;
    public static ulong FPartMask => 0x0000_0000_FFFF_FFFF;
}