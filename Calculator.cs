using System.Collections.Concurrent;

namespace BackpackingChallenge;

public class Calculator
{
    private readonly ConcurrentBag<Tuple<float, Item?[]>> _results = new();
    private readonly Backpack _backpack;
    private readonly uint _batchSize;
    private readonly uint _highIndex;
    private readonly byte _count;

    public Calculator(Backpack backpack)
    {
        _backpack = backpack;
        _batchSize = 1 << 20;
        _count = Math.Min((byte)31, _backpack.Count);
        _highIndex = (uint)1 << _count;
    }

    public async Task<Item?[]> CalcParallelAsync(int taskCount = 0)
    {
        if (0 == taskCount)
        {
            taskCount = Convert.ToInt32(Math.Max(2, Environment.ProcessorCount * 0.75));
        }

        var tasks = new List<Task>(taskCount);
        uint index = 0;
        while (index <= _highIndex)
        {
            if (tasks.Count < taskCount)
            {
                tasks.Add(CalcAsync(index, index + _batchSize));
                index += _batchSize;
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

    public Task CalcAsync(uint from, uint to)
    {
        uint variant = 0;
        var bestFit = float.MinValue;
        for (var i = from; i < to; i++)
        {
            var r = CalcVariant(i);
            if (r > bestFit)
            {
                bestFit = r;
                variant = i;
            }
        }

        var result = new Item?[_count];
        for (var i = 0; i < _count; i++)
        {
            var x = variant & (1 << i);
            result[i] = x != 0 ? _backpack.Items[i] : null;
        }

        _results.Add(new Tuple<float, Item?[]>(bestFit, result));
        return Task.CompletedTask;

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