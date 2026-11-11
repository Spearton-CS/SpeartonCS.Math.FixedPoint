using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpeartonCS.Math.FixedPoint;

public readonly unsafe struct FxP16Format<TUnderlying> : IFxPFormat<TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
{
    public const int fbits = 16;
    static FxP16Format()
    {
        if (sizeof(TUnderlying) <= (fbits / 8))
            throw new ArgumentException($"FxP{fbits}Format: {typeof(TUnderlying).Name} isn't enough wide (<= {fbits / 8} bytes)");
    }

    public static int IBits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (sizeof(TUnderlying) * 8) - fbits;
    }

    public static int FBits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => fbits;
    }
    public static TUnderlying MaxFPartDecimalDenominator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TUnderlying.CreateTruncating(1_000_0);
    }
    public static bool ISigned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TUnderlying.MinValue != TUnderlying.Zero;
    }

    public static TUnderlying IMaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TUnderlying.MaxValue >> fbits;
    }
    public static TUnderlying IMinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TUnderlying.MinValue >> fbits;
    }

    public static TUnderlying FMaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => FPartMask;
    }
    public static TUnderlying FMinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TUnderlying.Zero;
    }

    public static TUnderlying IPartMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ~FPartMask;
    }
    public static TUnderlying FPartMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (TUnderlying.One << fbits) - TUnderlying.One;
    }
}
