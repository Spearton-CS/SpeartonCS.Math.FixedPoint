namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_IIII_IIFF
/// </summary>
public readonly struct I24F8Format : IFxPFormat<int>
{
    public static int IBits => 24;
    public static int FBits => 8;
    public static bool ISigned => true;

    public static int IMaxValue => 0x007F_FFFF;
    public static int IMinValue => unchecked((int)0xFF80_0000);

    public static int FMaxValue => byte.MaxValue;
    public static int FMinValue => byte.MinValue;

    public static int IPartMask => unchecked((int)0xFFFF_FF00);
    public static int FPartMask => 0x0000_00FF;
}