using System.Numerics;

namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// Bits per I and F parts of <see cref="IFxPNumber{TSelf, TUnderlying}"/>, and indicator of sign in I-part
/// </summary>
public interface IFxPFormat<TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
{
    /// <summary>
    /// Bits before fixed point
    /// </summary>
    public abstract static int IBits { get; }
    /// <summary>
    /// Bits after fixed point
    /// </summary>
    public abstract static int FBits { get; }
    public abstract static TUnderlying MaxFPartDecimalDenominator { get; }
    /// <summary>
    /// Indicator of sign in I-part
    /// </summary>
    public abstract static bool ISigned { get; }

    public abstract static TUnderlying IMaxValue { get; }
    public abstract static TUnderlying IMinValue { get; }
    public abstract static TUnderlying FMaxValue { get; }
    public abstract static TUnderlying FMinValue { get; }

    public abstract static TUnderlying IPartMask { get; }
    public abstract static TUnderlying FPartMask { get; }
}