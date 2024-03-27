using System.Diagnostics;
using BackpackingChallenge;

var backpack = new Backpack
{
    Limit = 2_000,
    Items = new Item[]
    {
        new("Item 1", 1000, 0.33f),
        new("Item 2", 1000, 0.5f),
        new("Item 3", 1000, 1.25f)
    }
};

var calc = new Calculator(backpack);
var sw = Stopwatch.StartNew();
var solution = await calc.CalcParallelAsync();
sw.Stop();
var totalVolume = 0f;
foreach (var item in solution)
{
    if (item == null)
    {
        continue;
    }

    Console.WriteLine($"{item.Name}, {item.Volume}");
    totalVolume += item.Volume;
}

Console.WriteLine($"total volume: {totalVolume}");

Console.WriteLine();
Console.WriteLine("not in list:");
foreach (var item in backpack.Items)
{
    var inList = solution.Where(_ => _ != null).FirstOrDefault(_ => _.Name == item.Name);
    if (null == inList)
    {
        Console.WriteLine($"{item.Name}, {item.Volume}");
    }
}

Console.WriteLine($"{sw.ElapsedMilliseconds} ms.");