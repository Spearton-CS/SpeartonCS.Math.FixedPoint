namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// 0x_UUUU_UUFF
/// </summary>
public readonly struct U24F8Format : IFxPFormat<uint>
{
    public static int IBits => 24;
    public static int FBits => 8;
    public static bool ISigned => true;

    public static uint IMaxValue => 0x007F_FFFF;
    public static uint IMinValue => 0xFF80_0000;

    public static uint FMaxValue => byte.MaxValue;
    public static uint FMinValue => byte.MinValue;

    public static uint IPartMask => 0xFFFF_FF00;
    public static uint FPartMask => 0x0000_00FF;
}