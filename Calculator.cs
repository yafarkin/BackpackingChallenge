using System.Collections.Concurrent;
using System.Diagnostics;

namespace BackpackingChallenge;

public class Calculator : ICalculator
{
    private readonly ConcurrentBag<Tuple<float, Item?[]>> _results = new();
    private readonly Backpack _backpack;
    private readonly uint _batchSize;
    private readonly uint _highIndex;
    private readonly byte _count;
    private volatile bool _isFinished;

    public Calculator(Backpack backpack)
    {
        _backpack = backpack;
        _batchSize = 1 << 20;
        _count = Math.Min((byte)31, Convert.ToByte(_backpack.Count));
        _highIndex = (uint)1 << _count;
    }

    public async Task<IEnumerable<Item?>> CalcAsync()
    {
        var taskCount = Convert.ToInt32(Math.Max(2, Environment.ProcessorCount * 0.75));

        var tasks = new List<Task>(taskCount);
        uint index = 0;

        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var periodToShow = TimeSpan.FromSeconds(5);

        while (index < _highIndex && !_isFinished)
        {
            if (tasks.Count < taskCount)
            {
                Console.Write(".");
                tasks.Add(CalcAsync(index, index + _batchSize));
                index += _batchSize;

                if (sw.Elapsed > periodToShow)
                {
                    var percent = (float)index / _highIndex;
                    var remainingPercent = 1 - percent;

                    Console.WriteLine();
                    Console.WriteLine(percent.ToString("P"));

                    if (percent > 0)
                    {
                        var eta = TimeSpan.FromSeconds(totalSw.Elapsed.Seconds * (remainingPercent / percent));
                        Console.WriteLine($"ETA: {eta:d\\:h\\:mm\\:ss} s.");
                    }

                    sw.Restart();
                }
            }
            else
            {
                await Task.Delay(1);
                tasks.RemoveAll(_ => _.IsCompleted);
            }
        }

        await Task.WhenAll(tasks);

        var variant = Array.Empty<Item?>();
        var bestFit = float.MinValue;

        foreach (var v in _results)
        {
            if (v.Item1 > bestFit)
            {
                variant = v.Item2;
                bestFit = v.Item1;
            }
        }

        return variant;
    }

    public async Task CalcAsync(uint from, uint to)
    {
        await Task.Yield();

        uint variant = 0;
        var bestFit = float.MinValue;
        for (var i = from; i < to; i++)
        {
            var r = CalcVariant(i);
            if (r > bestFit)
            {
                bestFit = r;
                variant = i;

                if (_backpack.Limit - bestFit == 0)
                {
                    _isFinished = true;
                    break;
                }
            }
        }

        var result = new Item?[_count];
        for (var i = 0; i < _count; i++)
        {
            var x = variant & (1 << i);
            result[i] = x != 0 ? _backpack.Items[i] : null;
        }

        _results.Add(new Tuple<float, Item?[]>(bestFit, result));

        float CalcVariant(uint v)
        {
            var volume = 0f;
            var fit = 0f;

            for (var i = 0; i < _count; i++)
            {
                var c = (uint)1 << i;
                var x = v & c;
                if (x == 0)
                {
                    continue;
                }

                var item = _backpack.Items[i];
                var newVolume = volume + item.Volume;
                if (newVolume > _backpack.Limit)
                {
                    fit = float.MinValue;
                    break;
                }

                volume = newVolume;

                var weight = item.Weight;
                fit += item.Item2 * weight;
            }

            return fit;
        }
    }
}