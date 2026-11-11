using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace SpeartonCS.Math.FixedPoint;

public struct FxPNumber<TUnderlying, TFormat> : IFxPNumber<FxPNumber<TUnderlying, TFormat>, TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
    where TFormat : IFxPFormat<TUnderlying>
{
    #region Format

    public static FxPNumber<TUnderlying, TFormat> Zero => BitCast(TUnderlying.Zero);
    public static FxPNumber<TUnderlying, TFormat> One => BitCast(TUnderlying.One << FBits);
    public static FxPNumber<TUnderlying, TFormat> MinValue => BitCast(TUnderlying.MinValue);
    public static FxPNumber<TUnderlying, TFormat> MaxValue => BitCast(TUnderlying.MaxValue);
    public static FxPNumber<TUnderlying, TFormat> AllBitsSet => BitCast(TUnderlying.AllBitsSet);

    public static TUnderlying IMinValue => TFormat.IMinValue;
    public static TUnderlying IMaxValue => TFormat.IMaxValue;
    public static TUnderlying FMinValue => TFormat.FMinValue;
    public static TUnderlying FMaxValue => TFormat.FMaxValue;

    public static TUnderlying IPartMask => TFormat.IPartMask;
    public static TUnderlying FPartMask => TFormat.FPartMask;

    public static int IBits => TFormat.IBits;
    public static int FBits => TFormat.FBits;
    public static bool ISigned => TFormat.ISigned;

    #endregion

    #region Underlying

    public TUnderlying IPart
    {
        readonly get => raw >> FBits;
        set => raw = (raw & FPartMask) | (value << FBits);
    }
    public TUnderlying FPart
    {
        readonly get => raw & FPartMask;
        set => raw = (raw & IPartMask) | (value & FPartMask);
    }

    private TUnderlying raw;
    public static FxPNumber<TUnderlying, TFormat> BitCast(TUnderlying raw) => new() { raw = raw };
    public static TUnderlying BitCast(FxPNumber<TUnderlying, TFormat> fp) => fp.raw;

    #endregion

    #region Parsing and formatting

    public static FxPNumber<TUnderlying, TFormat> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static FxPNumber<TUnderlying, TFormat> Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static FxPNumber<TUnderlying, TFormat> Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        throw new NotImplementedException();
    }

    public readonly override string ToString() => ToString(null, null);
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        throw new NotImplementedException();
    }

    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Math

    public static FxPNumber<TUnderlying, TFormat> operator +(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw += right.raw;
        return left;
    }
    public static FxPNumber<TUnderlying, TFormat> operator -(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw -= right.raw;
        return left;
    }

    public static FxPNumber<TUnderlying, TFormat> operator -(FxPNumber<TUnderlying, TFormat> value)
    {
        value.raw = -value.raw;
        return value;
    }

    public static FxPNumber<TUnderlying, TFormat> operator ++(FxPNumber<TUnderlying, TFormat> value)
    {
        value.raw += TUnderlying.One << FBits;
        return value;
    }
    public static FxPNumber<TUnderlying, TFormat> operator --(FxPNumber<TUnderlying, TFormat> value)
    {
        value.raw -= TUnderlying.One << FBits;
        return value;
    }

    public static FxPNumber<TUnderlying, TFormat> operator *(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        int fBits = FBits;
        TUnderlying fMask = FPartMask;

        TUnderlying aInt = left.raw >> fBits;
        TUnderlying aFrac = left.raw & fMask;

        TUnderlying bInt = right.raw >> fBits;
        TUnderlying bFrac = right.raw & fMask;

        left.raw = ((aInt * bInt) << fBits)
                 + (aInt * bFrac)
                 + (aFrac * bInt)
                 + ((aFrac * bFrac) >>> fBits);
        return left;
    }
    public static FxPNumber<TUnderlying, TFormat> operator /(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        int
            fBits = FBits;
        TUnderlying
            a = left.raw,
            b = right.raw;

        TUnderlying
            intPart = (a / b) << fBits,
            rem = a % b;

        int
            halfF = fBits >> 1,
            restF = fBits - halfF;

        TUnderlying
            mid = (rem << halfF) / b,
            rem2 = (rem << halfF) % b,
            low = (rem2 << restF) / b;

        left.raw = intPart + (mid << restF) + low;
        return left;
    }

    public static FxPNumber<TUnderlying, TFormat> operator %(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw %= right.raw;
        return left;
    }

    #endregion

    #region Equality

    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is FxPNumber<TUnderlying, TFormat> other && this == other;
    public readonly bool Equals(FxPNumber<TUnderlying, TFormat> other) => this == other;
    public readonly override int GetHashCode() => raw.GetHashCode();

    public static bool operator ==(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw == right.raw;
    public static bool operator !=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw != right.raw;
    public static bool operator <(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw < right.raw;
    public static bool operator >(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw > right.raw;
    public static bool operator <=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw <= right.raw;
    public static bool operator >=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw >= right.raw;

    #endregion
}