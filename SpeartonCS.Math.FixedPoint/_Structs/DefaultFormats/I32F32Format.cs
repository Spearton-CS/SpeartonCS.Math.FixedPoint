namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0xIIII_IIII_FFFF_FFFF
/// </summary>
public readonly struct I32F32Format : IFxPFormat<long>
{
    public static int IBits => 32;
    public static int FBits => 32;
    public static bool ISigned => true;

    public static long IMaxValue => int.MaxValue;
    public static long IMinValue => int.MinValue;

    public static long FMaxValue => uint.MaxValue;
    public static long FMinValue => uint.MinValue;

    public static long IPartMask => unchecked((long)0xFFFF_FFFF_0000_0000);
    public static long FPartMask => 0x0000_0000_FFFF_FFFF;
}