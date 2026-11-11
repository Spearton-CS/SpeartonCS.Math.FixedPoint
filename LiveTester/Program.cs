using SpeartonCS.Math.FixedPoint;

using System.Diagnostics;

namespace LiveTester;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Sleep for a sec");
        Thread.Sleep(1000);
        float fMin = -32768.0f;
        float fMax = 32767.9999f;
        float fInc = 0.1f;
        fMax -= fInc;

        var newMin = FxPNumber<int, FxP16Format<int>>.MinValue;
        var newMax = FxPNumber<int, FxP16Format<int>>.MaxValue;
        var newInc = FxPNumber<int, FxP16Format<int>>.Zero;
        newInc.SetFPartDecimal(1, 10);
        newMax -= newInc;

        Console.WriteLine("Warmup");

        for (float f = fMin; f < fMin + 1.0f; f += fInc) { }
        for (var i = newMin; i < newMin + newInc + newInc; i += newInc) { }

        Stopwatch sw = new();

        int dummyFloat = 0;
        sw.Restart();
        for (float f = fMin; f < fMax; f += fInc)
            dummyFloat ^= (int)f;
        sw.Stop();
        Console.WriteLine($"float:\t\t\t{sw.Elapsed} (Anti-DCE: {dummyFloat})");

        int dummyNew = 0;
        sw.Restart();
        for (var i = newMin; i < newMax; i += newInc)
            dummyNew ^= i.IPart;
        sw.Stop();
        Console.WriteLine($"F16Format<int>:\t\t{sw.Elapsed} (Anti-DCE: {dummyNew})");

        Console.ReadKey();
    }
}