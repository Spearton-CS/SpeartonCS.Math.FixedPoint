using System.Numerics;

namespace SpeartonCS.Math.FixedPoint;

/// <summary>
/// Generic Fixed-Point number over <typeparamref name="TUnderlying"/>
/// </summary>
/// <typeparam name="TSelf"></typeparam>
/// <typeparam name="TUnderlying"></typeparam>
public interface IFxPNumber<TSelf, TUnderlying>
    : IFxPFormat<TUnderlying>,
    IEquatable<TSelf>, IComparisonOperators<TSelf, TSelf, bool>,
    IAdditionOperators<TSelf, TSelf, TSelf>, ISubtractionOperators<TSelf, TSelf, TSelf>,
    IMultiplyOperators<TSelf, TSelf, TSelf>, IDivisionOperators<TSelf, TSelf, TSelf>, IModulusOperators<TSelf, TSelf, TSelf>,
    IIncrementOperators<TSelf>, IDecrementOperators<TSelf>, IUnaryNegationOperators<TSelf, TSelf>,
    IMinMaxValue<TSelf>, IAdditiveIdentity<TSelf, TSelf>, IMultiplicativeIdentity<TSelf, TSelf>,
    IFormattable, ISpanFormattable, IUtf8SpanFormattable,
    IParsable<TSelf>, ISpanParsable<TSelf>, IUtf8SpanParsable<TSelf>

    where TSelf : unmanaged, IFxPNumber<TSelf, TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
{
    public abstract static TSelf Zero { get; }
    public abstract static TSelf One { get; }
    public abstract static TSelf AllBitsSet { get; }

    static TSelf IAdditiveIdentity<TSelf, TSelf>.AdditiveIdentity => TSelf.Zero;
    static TSelf IMultiplicativeIdentity<TSelf, TSelf>.MultiplicativeIdentity => TSelf.One;

    public abstract static TSelf BitCast(TUnderlying raw);
    public abstract static TUnderlying BitCast(TSelf fp);

    /// <summary>
    /// The REAL (decimal) <typeparamref name="TUnderlying"/> as <see cref="IPart"/>
    /// </summary>
    public TUnderlying IPart { get; set; }
    /// <summary>
    /// The REAL (binary) <typeparamref name="TUnderlying"/> as <see cref="FPart"/>
    /// </summary>
    public TUnderlying FPart { get; set; }
    /// <summary>
    /// Sets <see cref="FPart"/> as decimal <typeparamref name="TUnderlying"/>
    /// </summary>
    /// <param name="dec">Decimal value</param>
    /// <param name="denominator">1, 10, 100 and etc.</param>
    /// <returns>REAL (binary) value</returns>
    public TUnderlying SetFPartDecimal(TUnderlying dec, TUnderlying denominator);
    /// <summary>
    /// Gets <see cref="FPart"/> as decimal <typeparamref name="TUnderlying"/>
    /// </summary>
    /// <param name="denominator">1, 10, 100 and etc.</param>
    /// <returns>Decimal value</returns>
    public TUnderlying GetFPartDecimal(TUnderlying denominator);
    public TUnderlying GetFPartDecimalDenominator();
    public TUnderlying GetFPartDecimalDenominator(out TUnderlying decimalValue);
}