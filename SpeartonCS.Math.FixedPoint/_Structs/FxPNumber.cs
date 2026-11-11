using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpeartonCS.Math.FixedPoint;

public struct FxPNumber<TUnderlying, TFormat> : IFxPNumber<FxPNumber<TUnderlying, TFormat>, TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
    where TFormat : IFxPFormat<TUnderlying>
{
    unsafe static FxPNumber()
    {
        int fbytes = TFormat.FBits / 8;
        if (sizeof(TUnderlying) <= fbytes)
            throw new ArgumentException($"FxPNumber ({typeof(TFormat).Name}): {typeof(TUnderlying).Name} isn't enough wide (<= {fbytes} bytes)");
    }

    #region Format

    public static FxPNumber<TUnderlying, TFormat> Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitCast(TUnderlying.Zero);
    }
    public static FxPNumber<TUnderlying, TFormat> One
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitCast(TUnderlying.One << FBits);
    }
    public static FxPNumber<TUnderlying, TFormat> MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitCast(TUnderlying.MinValue);
    }
    public static FxPNumber<TUnderlying, TFormat> MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitCast(TUnderlying.MaxValue);
    }
    public static FxPNumber<TUnderlying, TFormat> AllBitsSet
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitCast(TUnderlying.AllBitsSet);
    }

    public static TUnderlying IMinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.IMinValue;
    }
    public static TUnderlying IMaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.IMaxValue;
    }
    public static TUnderlying FMinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.FMinValue;
    }
    public static TUnderlying FMaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.FMaxValue;
    }

    public static TUnderlying IPartMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.IPartMask;
    }
    public static TUnderlying FPartMask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.FPartMask;
    }

    public static int IBits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.IBits;
    }
    public static int FBits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.FBits;
    }
    public static TUnderlying MaxFPartDecimalDenominator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.MaxFPartDecimalDenominator;
    }
    public static bool ISigned
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TFormat.ISigned;
    }

    #endregion

    #region Underlying

    public TUnderlying IPart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => raw >> FBits;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => raw = (raw & FPartMask) | (value << FBits);
    }
    public TUnderlying FPart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => raw & FPartMask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => raw = (raw & IPartMask) | (value & FPartMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TUnderlying SetFPartDecimal(TUnderlying decimalValue, TUnderlying denominator)
    {
        TUnderlying rawFraction = ((decimalValue << TFormat.FBits) + (denominator / (TUnderlying.One + TUnderlying.One))) / denominator;

        FPart = rawFraction;
        return rawFraction;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TUnderlying GetFPartDecimal(TUnderlying denominator)
        => ((FPart * denominator) + (TUnderlying.One << (TFormat.FBits - 1))) >> TFormat.FBits;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TUnderlying GetFPartDecimalDenominator() => GetFPartDecimalDenominator(out _);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly TUnderlying GetFPartDecimalDenominator(out TUnderlying decimalValue)
    {
        TUnderlying denom = MaxFPartDecimalDenominator;
        decimalValue = GetFPartDecimal(denom);

        if (decimalValue == TUnderlying.Zero)
            return TUnderlying.One;

        TUnderlying ten = TUnderlying.CreateTruncating(10);
        while (denom > TUnderlying.One && (decimalValue % ten) == TUnderlying.Zero)
        {
            decimalValue /= ten;
            denom /= ten;
        }

        return denom;
    }

    private TUnderlying raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> BitCast(TUnderlying raw) => new() { raw = raw };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TUnderlying BitCast(FxPNumber<TUnderlying, TFormat> fp) => fp.raw;

    #endregion

    #region Parsing and formatting

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;
        throw new FormatException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), provider);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider = null)
    {
        if (TryParse(utf8Text, provider, out var result))
            return result;
        throw new FormatException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), provider, out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        result = default;
        s = s.Trim();
        if (s.IsEmpty)
            return false;

        int sepIndex = -1;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '.' || c == ',')
            {
                sepIndex = i;
                break;
            }
        }

        ReadOnlySpan<char> intSpan = sepIndex >= 0 ? s[..sepIndex] : s;
        ReadOnlySpan<char> fracSpan = sepIndex >= 0 ? s[(sepIndex + 1)..] : ReadOnlySpan<char>.Empty;

        TUnderlying iVal = TUnderlying.Zero;
        if (!intSpan.IsEmpty && !TUnderlying.TryParse(intSpan, NumberStyles.Integer, provider, out iVal))
            return false;

        result.IPart = iVal;

        if (!fracSpan.IsEmpty)
        {
            TUnderlying ten = TUnderlying.CreateTruncating(10);
            TUnderlying maxDenom = MaxFPartDecimalDenominator;
            TUnderlying denom = TUnderlying.One;
            int len = 0;

            while (len < fracSpan.Length && denom < maxDenom)
            {
                denom *= ten;
                len++;
            }

            fracSpan = fracSpan[..len];
            if (!fracSpan.IsEmpty)
            {
                if (!TUnderlying.TryParse(fracSpan, NumberStyles.None, provider, out TUnderlying fDec))
                    return false;

                result.SetFPartDecimal(fDec, denom);
            }
        }

        return true;
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, [MaybeNullWhen(false)] out FxPNumber<TUnderlying, TFormat> result)
    {
        result = default;
        while (!utf8Text.IsEmpty && utf8Text[0] <= ' ') utf8Text = utf8Text[1..];
        while (!utf8Text.IsEmpty && utf8Text[^1] <= ' ') utf8Text = utf8Text[..^1];
        if (utf8Text.IsEmpty)
            return false;

        int sepIndex = -1;
        for (int i = 0; i < utf8Text.Length; i++)
        {
            byte b = utf8Text[i];
            if (b == (byte)'.' || b == (byte)',')
            {
                sepIndex = i;
                break;
            }
        }

        ReadOnlySpan<byte> intSpan = sepIndex >= 0 ? utf8Text[..sepIndex] : utf8Text;
        ReadOnlySpan<byte> fracSpan = sepIndex >= 0 ? utf8Text[(sepIndex + 1)..] : ReadOnlySpan<byte>.Empty;

        TUnderlying iVal = TUnderlying.Zero;
        if (!intSpan.IsEmpty && !TUnderlying.TryParse(intSpan, NumberStyles.Integer, provider, out iVal))
            return false;

        result.IPart = iVal;

        if (!fracSpan.IsEmpty)
        {
            TUnderlying ten = TUnderlying.CreateTruncating(10);
            TUnderlying maxDenom = MaxFPartDecimalDenominator;
            TUnderlying denom = TUnderlying.One;
            int len = 0;

            while (len < fracSpan.Length && denom < maxDenom)
            {
                denom *= ten;
                len++;
            }

            fracSpan = fracSpan[..len];
            if (!fracSpan.IsEmpty)
            {
                if (!TUnderlying.TryParse(fracSpan, NumberStyles.None, provider, out TUnderlying fDec))
                    return false;

                result.SetFPartDecimal(fDec, denom);
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString() => ToString(null, null);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        Span<char> buffer = stackalloc char[64];
        if (TryFormat(buffer, out int written, format.AsSpan(), formatProvider))
            return new string(buffer[..written]);

        return string.Empty;
    }

    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        charsWritten = 0;

        if (!IPart.TryFormat(destination, out int iWritten, default, provider))
            return false;
        charsWritten += iWritten;

        TUnderlying ten = TUnderlying.CreateTruncating(10);
        TUnderlying denom = MaxFPartDecimalDenominator;
        int decimals = 0;

        if (!format.IsEmpty)
        {
            ReadOnlySpan<char> digits = (format[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z') ? format[1..] : format;
            if (!digits.IsEmpty && int.TryParse(digits, out int d) && d >= 0)
            {
                decimals = d;
                denom = TUnderlying.One;
                for (int i = 0; i < d; i++)
                    denom *= ten;
            }
        }
        else
        {
            for (TUnderlying tmp = denom; tmp > TUnderlying.One; tmp /= ten)
                decimals++;
        }

        if (decimals == 0)
            return true;

        string sep = (provider?.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo)?.NumberDecimalSeparator ?? ".";
        if (destination.Length - charsWritten < sep.Length + decimals)
            return false;

        sep.AsSpan().CopyTo(destination[charsWritten..]);
        charsWritten += sep.Length;

        TUnderlying fDec = GetFPartDecimal(denom);
        if (fDec >= denom)
            fDec = denom - TUnderlying.One;

        for (int i = decimals - 1; i >= 0; i--)
        {
            TUnderlying digit = fDec % ten;
            fDec /= ten;
            destination[charsWritten + i] = (char)('0' + int.CreateTruncating(digit));
        }

        charsWritten += decimals;
        return true;
    }

    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        bytesWritten = 0;

        if (!IPart.TryFormat(utf8Destination, out int iWritten, default, provider))
            return false;
        bytesWritten += iWritten;

        TUnderlying ten = TUnderlying.CreateTruncating(10);
        TUnderlying denom = MaxFPartDecimalDenominator;
        int decimals = 0;

        if (!format.IsEmpty)
        {
            ReadOnlySpan<char> digits = (format[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z') ? format[1..] : format;
            if (!digits.IsEmpty && int.TryParse(digits, out int d) && d >= 0)
            {
                decimals = d;
                denom = TUnderlying.One;
                for (int i = 0; i < d; i++)
                    denom *= ten;
            }
        }
        else
        {
            for (TUnderlying tmp = denom; tmp > TUnderlying.One; tmp /= ten)
                decimals++;
        }

        if (decimals == 0)
            return true;

        if (utf8Destination.Length - bytesWritten < 1 + decimals)
            return false;

        utf8Destination[bytesWritten++] = (byte)'.';

        TUnderlying fDec = GetFPartDecimal(denom);
        if (fDec >= denom)
            fDec = denom - TUnderlying.One;

        for (int i = decimals - 1; i >= 0; i--)
        {
            TUnderlying digit = fDec % ten;
            fDec /= ten;
            utf8Destination[bytesWritten + i] = (byte)('0' + int.CreateTruncating(digit));
        }

        bytesWritten += decimals;
        return true;
    }

    #endregion

    #region Math

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> operator +(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw += right.raw;
        return left;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> operator -(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw -= right.raw;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> operator -(FxPNumber<TUnderlying, TFormat> value)
    {
        value.raw = -value.raw;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> operator ++(FxPNumber<TUnderlying, TFormat> value)
    {
        value.raw += TUnderlying.One << FBits;
        return value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FxPNumber<TUnderlying, TFormat> operator %(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right)
    {
        left.raw %= right.raw;
        return left;
    }

    #endregion

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is FxPNumber<TUnderlying, TFormat> other && this == other;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(FxPNumber<TUnderlying, TFormat> other) => this == other;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode() => raw.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw == right.raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw != right.raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw < right.raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw > right.raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw <= right.raw;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(FxPNumber<TUnderlying, TFormat> left, FxPNumber<TUnderlying, TFormat> right) => left.raw >= right.raw;

    #endregion
}