namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_IIII_FFFF
/// </summary>
public readonly struct I16F16Format : IFxPFormat<int>
{
    public static int IBits => 16;
    public static int FBits => 16;
    public static bool ISigned => true;

    public static int IMaxValue => short.MaxValue;
    public static int IMinValue => short.MinValue;

    public static int FMaxValue => ushort.MaxValue;
    public static int FMinValue => ushort.MinValue;

    public static int IPartMask => unchecked((int)0xFFFF_0000);
    public static int FPartMask => 0x0000_FFFF;
}