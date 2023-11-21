using System.Diagnostics;
using BackpackingChallenge;

var backpack = new Backpack
{
    Size = 26,
    Weights = new byte[] {1,  1, 1,  2, 2, 1, 1, 1, 1, 1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, 1},
    Volumes = new uint[] {27, 2, 26, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 3, 1}
};

var calc = new Calculator(backpack);
var sw = Stopwatch.StartNew();
var solution = await calc.CalcParallelAsync();
sw.Stop();
Console.WriteLine(string.Join(", ", solution));
Console.WriteLine(sw.ElapsedMilliseconds);