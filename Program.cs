using BackpackingChallenge;
using System.Diagnostics;

var items = new List<Item>
{
    new("Item 1", 5000f, 1f, 30),
    new("Item 2", 80000f, 0.5f, 4),
    new("Item 3", 500f, 0.5f, 2)
};

var backpack = new Backpack(100_000, items);

ICalculator calc;

calc = new HeavyCalculator(backpack);
//calc = new GreedyCalculator(backpack);
//calc = new Calculator(backpack);

var sw = Stopwatch.StartNew();
var solution = (await calc.CalcAsync()).Where(x => x != null).GroupBy(x => x.Name).ToList();
sw.Stop();

Console.WriteLine();

var totalVolume = 0f;
var usedItems = new Dictionary<string, int>();
foreach (var g in solution)
{
    var item = g.First()!;
    var count = g.Count();
    var totalItemVolume = item.Volume* count;
    usedItems.Add(item.Name, count);
    Console.WriteLine($"{item.Name}, {item.Volume}, {count}, {totalItemVolume}");
    totalVolume += totalItemVolume;
}

Console.WriteLine($"total volume: {totalVolume}");

Console.WriteLine();
Console.WriteLine("not in list:");
totalVolume = 0;
foreach (var item in items)
{
    var unusedCount = item.Count;
    if (usedItems.TryGetValue(item.Name, out var usedCount))
    {
        unusedCount -= usedCount;
    }

    if (unusedCount > 0)
    {
        Console.WriteLine($"{item.Name}, {item.Volume}, {unusedCount}");
        totalVolume += item.Volume * unusedCount;
    }
}
Console.WriteLine($"total unused volume: {totalVolume}");

Console.WriteLine();
Console.WriteLine($"ETA: {sw.Elapsed:d\\:h\\:mm\\:ss} s.");
