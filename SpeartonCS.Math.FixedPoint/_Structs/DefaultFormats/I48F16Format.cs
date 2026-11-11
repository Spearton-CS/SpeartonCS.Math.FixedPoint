namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0xIIII_IIII_IIII_FFFF
/// </summary>
public readonly struct I48F16Format : IFxPFormat<long>
{
    public static int IBits => 48;
    public static int FBits => 16;
    public static bool ISigned => true;

    public static long IMaxValue => 0x0000_7FFF_FFFF_FFFF;
    public static long IMinValue => unchecked((long)0xFFFF_8000_0000_0000);

    public static long FMaxValue => 0x0000_0000_0000_FFFF;
    public static long FMinValue => 0;

    public static long IPartMask => unchecked((long)0xFFFF_FFFF_FFFF_0000);
    public static long FPartMask => 0x0000_0000_0000_FFFF;
}