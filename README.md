# SpeartonCS.Math.FixedPoint

Fixed-point **generic math** for .NET 10 and higher.

- 🧮 Fully generic: both the storage integer type and the format (number of fractional bits) are type parameters
- 🚀 NativeAOT-compatible and trimming-friendly (`IsAotCompatible`)
- 📦 No dependencies
- 🔧 The number and the format are interfaces: you can implement your own `IFxPNumber` and `IFxPFormat`
- 🧱 Everything lives in a single namespace: `SpeartonCS.Math.FixedPoint`

> ⚠️ **Beta.** The API may change between versions. Unit tests and benchmarks are **not written yet** (both are planned), so expect bugs, especially in the cases listed in [Known limitations](#known-limitations).

---

## Installation

This package is currently a **pre-release (beta)**, so the `--prerelease` flag is required:

```
dotnet add package SpeartonCS.Math.FixedPoint --prerelease
```

Requires **.NET 10** or higher.

---

## Concept

A fixed-point number is stored as one plain integer (`raw`), and its real value is

```
value = raw / 2^FBits
```

where `FBits` is the number of fractional bits. The storage type and the format are chosen independently:

```csharp
FxPNumber<TUnderlying, TFormat>
```

| Parameter | Meaning | Examples |
|---|---|---|
| `TUnderlying` | Integer type that holds `raw` | `int`, `uint`, `long`, `ulong`, `Int128`, `UInt128` |
| `TFormat` | Number of fractional bits | `FxP8Format<int>`, `FxP32Format<long>` |

`TUnderlying` must satisfy `unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>`.
`TFormat` must implement `IFxPFormat<TUnderlying>`.

Signed storage gives a signed number, unsigned storage gives an unsigned one.

Naming convention used in this document: `I{integer bits}F{fractional bits}`.

| Alias | Storage | Format | Integer bits | Fractional bits |
|---|---|---|---|---|
| `I24F8` | `int` | `FxP8Format<int>` | 24 | 8 |
| `I16F16` | `int` | `FxP16Format<int>` | 16 | 16 |
| `I48F16` | `long` | `FxP16Format<long>` | 48 | 16 |
| `I32F32` | `long` | `FxP32Format<long>` | 32 | 32 |
| `U16F16` | `uint` | `FxP16Format<uint>` | 16 | 16 |

---

## Quick start

### 1. Declare aliases with `global using`

Writing `FxPNumber<long, FxP32Format<long>>` everywhere is noisy. Declare the aliases once, for example in `GlobalUsings.cs`, and use the short names in every file of the project:

```csharp
// GlobalUsings.cs

// 32 integer bits, 32 fractional bits, stored in a long
global using I32F32 = SpeartonCS.Math.FixedPoint.FxPNumber<
    long,
    SpeartonCS.Math.FixedPoint.FxP32Format<long>>;

// 24 integer bits, 8 fractional bits, stored in an int
global using I24F8 = SpeartonCS.Math.FixedPoint.FxPNumber<
    int,
    SpeartonCS.Math.FixedPoint.FxP8Format<int>>;
```

Aliases cost nothing at runtime: the compiler resolves them to the fully closed generic types, so the JIT and NativeAOT see every concrete type.

### 2. Use them

```csharp
I32F32 a = I32F32.Parse("3.25");
I32F32 b = I32F32.One + I32F32.One;   // 2

I32F32 c = a * b;                      // 6.5
I32F32 d = c / a;                      // 2
I32F32 e = -c;

Console.WriteLine(c);                  // 6.500000000
Console.WriteLine(c.ToString("2"));    // 6.50

bool less = a < c;                     // true

// Parts
long whole = a.IPart;                  // 3
long frac  = a.FPart;                  // raw binary fraction, 0.25 * 2^32
long cents = a.GetFPartDecimal(100);   // 25

// Raw access
long raw = I32F32.BitCast(a);
I32F32 f = I32F32.BitCast(raw);
```

```csharp
I24F8 x = I24F8.Parse("1.5");
x++;                                   // +1.0, so 2.5
x.IPart = 10;                          // 10.5, the fractional part is untouched
x.SetFPartDecimal(25, 100);            // 10.25
```

---

## API overview

### `IFxPNumber<TSelf, TUnderlying>`

`FxPNumber<,>` implements it. The interface itself inherits `IFxPFormat<TUnderlying>`, so every format member is also available on the number type.

| Group | Members |
|---|---|
| Constants | `Zero`, `One`, `MinValue`, `MaxValue`, `AllBitsSet` |
| Raw access | `BitCast(TUnderlying)`, `BitCast(TSelf)` |
| Parts | `IPart`, `FPart` (get and set) |
| Decimal fraction | `SetFPartDecimal(dec, denominator)`, `GetFPartDecimal(denominator)`, `GetFPartDecimalDenominator()`, `GetFPartDecimalDenominator(out decimalValue)` |
| Arithmetic | `+`, `-`, `*`, `/`, `%`, unary `-`, `++`, `--` |
| Comparison | `==`, `!=`, `<`, `>`, `<=`, `>=`, `IEquatable<T>` |
| Text | `Parse`, `TryParse`, `ToString`, `TryFormat` for `char` and UTF-8 (`IParsable`, `ISpanParsable`, `IUtf8SpanParsable`, `IFormattable`, `ISpanFormattable`, `IUtf8SpanFormattable`) |
| Generic math | `IAdditiveIdentity`, `IMultiplicativeIdentity`, `IMinMaxValue`, and the operator interfaces from `System.Numerics` |

`++` and `--` add and subtract **1.0**, not one raw unit.

`IPart` is the **floored** integer part (`raw >> FBits`) and `FPart` is always non-negative, so `-1.5` is stored as `IPart = -2`, `FPart = 0.5`.

Multiplication and division are implemented without a wider intermediate type, so they have range constraints (see [Known limitations](#known-limitations)).

### Parsing and formatting

- `Parse` / `TryParse` accept `.` or `,` as the separator and work on `string`, `ReadOnlySpan<char>` and UTF-8 `ReadOnlySpan<byte>`.
- Extra fractional digits beyond `MaxFPartDecimalDenominator` are ignored.
- The format string is an optional letter followed by the number of decimals: `"2"` and `"F2"` both print two decimals. With no format string the number prints as many decimals as `MaxFPartDecimalDenominator` allows.

---

## Built-in formats

Each format is a `readonly struct FxP{N}Format<TUnderlying>`, where `N` is the number of fractional bits.

| Format | Fractional bits | Max decimal digits of the fraction |
|---|---|---|
| `FxP4Format<T>` | 4 | 1 |
| `FxP8Format<T>` | 8 | 2 |
| `FxP12Format<T>` | 12 | 3 |
| `FxP16Format<T>` | 16 | 4 |
| `FxP20Format<T>` | 20 | 6 |
| `FxP24Format<T>` | 24 | 7 |
| `FxP32Format<T>` | 32 | 9 |
| `FxP40Format<T>` | 40 | 12 |
| `FxP48Format<T>` | 48 | 14 |
| `FxP56Format<T>` | 56 | 16 |
| `FxP64Format<T>` | 64 | 19 |
| `FxP96Format<T>` | 96 | 28 |
| `FxP112Format<T>` | 112 | 33 |
| `FxP120Format<T>` | 120 | 36 |

The storage type must be **wider than the fractional part**, otherwise there is no room for the integer part (for example `FxP32Format<int>`). Such an instantiation throws when the type is initialized.

### `IFxPFormat<TUnderlying>`

| Member | Meaning |
|---|---|
| `FBits` | Number of fractional bits |
| `IBits` | Number of integer bits (`sizeof(TUnderlying) * 8 - FBits`) |
| `ISigned` | Whether the integer part is signed |
| `IMaxValue` / `IMinValue` | Largest and smallest integer part |
| `FMaxValue` / `FMinValue` | Largest and smallest fractional part |
| `IPartMask` / `FPartMask` | Bit masks of the integer and fractional parts |
| `MaxFPartDecimalDenominator` | `10^k`, where `k = floor(FBits * log10(2))`: the number of decimal digits the fractional part represents without loss |

---

## Implementing your own

The library is built around two interfaces, and **you can implement both yourself**.

### Custom format: `IFxPFormat<TUnderlying>`

Use this when you need a fractional width that is not in the built-in list, for example 10 bits:

```csharp
using System.Numerics;
using System.Runtime.CompilerServices;
using SpeartonCS.Math.FixedPoint;

namespace MyGame.Math;

public readonly unsafe struct FxP10Format<TUnderlying> : IFxPFormat<TUnderlying>
    where TUnderlying : unmanaged, IBinaryInteger<TUnderlying>, IMinMaxValue<TUnderlying>
{
    public const int fbits = 10;

    static FxP10Format()
    {
        if (sizeof(TUnderlying) * 8 <= fbits)
            throw new ArgumentException($"FxP{fbits}Format: {typeof(TUnderlying).Name} isn't wide enough");
    }

    public static int IBits    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (sizeof(TUnderlying) * 8) - fbits; }
    public static int FBits    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => fbits; }
    public static bool ISigned { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TUnderlying.MinValue != TUnderlying.Zero; }

    // floor(10 * log10(2)) = 3 decimal digits
    public static TUnderlying MaxFPartDecimalDenominator
    { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TUnderlying.CreateTruncating(1_000); }

    public static TUnderlying IMaxValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TUnderlying.MaxValue >> fbits; }
    public static TUnderlying IMinValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TUnderlying.MinValue >> fbits; }
    public static TUnderlying FMaxValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => FPartMask; }
    public static TUnderlying FMinValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => TUnderlying.Zero; }
    public static TUnderlying IPartMask { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ~FPartMask; }
    public static TUnderlying FPartMask { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (TUnderlying.One << fbits) - TUnderlying.One; }
}
```

Then plug it into `FxPNumber` like any built-in format:

```csharp
global using I22F10 = SpeartonCS.Math.FixedPoint.FxPNumber<
    int,
    MyGame.Math.FxP10Format<int>>;
```

### Custom number: `IFxPNumber<TSelf, TUnderlying>`

If `FxPNumber<TUnderlying, TFormat>` does not fit your needs, implement the interface on your own type. Typical reasons: checked or saturating arithmetic, a different multiplication or rounding strategy, extra invariants, a SIMD-backed layout.

Requirements:

- `TSelf` must be `unmanaged` and implement `IFxPNumber<TSelf, TUnderlying>`.
- You provide everything from the [API overview](#api-overview), including the format members (`IBits`, `FBits`, masks and so on), because `IFxPNumber` inherits `IFxPFormat<TUnderlying>`.
- `AdditiveIdentity` and `MultiplicativeIdentity` already default to `Zero` and `One`.

Any generic code written against `IFxPNumber<TSelf, TUnderlying>` will then accept your type, as well as the built-in one.

---

## Design notes

- **Static abstract members.** Formats expose their parameters as `static` members of a struct type, so every concrete instantiation gets its own specialized code and the constants fold at compile time.
- **Generic math.** Storage is constrained by `IBinaryInteger<T>` and `IMinMaxValue<T>`, so any integer type up to `UInt128` works.
- **No wider type needed.** Multiplication and division are built from integer and fractional parts instead of widening to a bigger type. This keeps the code generic, but limits the usable range (see below).
- **Arithmetic is unchecked.** Overflow wraps, like the underlying integer type.
- **NativeAOT and trimming.** No reflection, no dynamic code generation, no dependencies.
- **Performance.** In an informal hand-written benchmark under NativeAOT, no measurable difference was found between a hand-written concrete fixed-point type and the fully generic version. This is anecdotal, and proper benchmarks are planned.

---

## Known limitations

These are known issues of the current beta. They will be fixed or covered by tests before a stable release.

- **Negative numbers in text.** Because `IPart` is floored and `FPart` is non-negative, negative values are not printed as a sign followed by a magnitude: `-1.5` is printed as `-2.5`. Parsing is affected too: `"-1.5"` is read as `-0.5`, and `"-0.5"` is read as `+0.5` (the sign is lost). Arithmetic on the raw value is not affected. Until this is fixed, avoid text round-trips of negative values.
- **Multiplication** is correct only when the storage type is at least twice as wide as the fractional part (`2 * FBits <= storage bits`). `I24F8` (`int`), `I16F16` (`int`) and `I32F32` (`long`) are fine, while formats such as `FxP96Format<Int128>` give wrong results.
- **Division** can overflow when the divisor is large relative to the storage type. For example, in `I32F32`, `40000 / 50000` returns a wrong result, while small divisors work.
- **Decimal conversion** (`Parse`, `ToString`, `SetFPartDecimal`, `GetFPartDecimal`) can overflow when the fractional part is very wide for the storage type. For example, parsing a 10-digit fraction in `FxP96Format<Int128>` returns a wrong value, while short fractions work.

---

## Not included (yet)

- Operators mixing a fixed-point number with a plain integer or floating-point value. Use `BitCast`, `IPart` and `FPart` for conversions.
- Checked and saturating arithmetic (this is a good use case for a custom `IFxPNumber`).

---

## Project status

| Item | Status |
|---|---|
| Formats (4 to 120 fractional bits) | ✅ |
| `IFxPFormat` / `IFxPNumber` / `FxPNumber` | ✅ beta |
| Known limitations | ⚠️ see [above](#known-limitations) |
| Unit tests | 🕓 planned |
| Benchmarks | 🕓 planned |

---

## Links

- Repository: https://github.com/Spearton-CS/SpeartonCS.Math.FixedPoint
- NuGet: https://www.nuget.org/packages/SpeartonCS.Math.FixedPoint