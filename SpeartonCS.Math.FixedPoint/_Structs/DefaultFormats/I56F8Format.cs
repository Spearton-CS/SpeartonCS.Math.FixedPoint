namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_IIII_IIII_IIII_IIFF
/// </summary>
public readonly struct I56F8Format : IFxPFormat<long>
{
    public static int IBits => 56;
    public static int FBits => 8;
    public static bool ISigned => true;

    public static long IMaxValue => 0x007F_FFFF_FFFF_FFFF;
    public static long IMinValue => unchecked((long)0xFF80_0000_0000_0000);

    public static long FMaxValue => byte.MaxValue;
    public static long FMinValue => byte.MinValue;

    public static long IPartMask => unchecked((long)0xFFFF_FFFF_FFFF_FF00);
    public static long FPartMask => 0x0000_0000_0000_00FF;
}