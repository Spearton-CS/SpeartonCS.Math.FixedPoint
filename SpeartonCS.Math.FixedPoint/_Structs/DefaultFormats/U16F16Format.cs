namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_UUUU_FFFF
/// </summary>
public readonly struct U16F16Format : IFxPFormat<uint>
{
    public static int IBits => 16;
    public static int FBits => 16;
    public static bool ISigned => false;

    public static uint IMaxValue => ushort.MaxValue;
    public static uint IMinValue => ushort.MinValue;

    public static uint FMaxValue => ushort.MaxValue;
    public static uint FMinValue => ushort.MinValue;

    public static uint IPartMask => 0xFFFF_0000;
    public static uint FPartMask => 0x0000_FFFF;
}
