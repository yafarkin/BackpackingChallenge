using System.Collections.Concurrent;

namespace BackpackingChallenge;

public class Calculator
{
    private readonly ConcurrentBag<Tuple<uint[], float>> _results = new();
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

    public async Task<uint[]> CalcParallelAsync(int taskCount = 0)
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

        var variant = Array.Empty<uint>();
        var bestFit = float.MinValue;

        foreach (var v in _results)
        {
            if (v.Item2 > bestFit)
            {
                variant = v.Item1;
                bestFit = v.Item2;
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

        ulong s = 0;
        var result = new uint[_count];
        for (var i = 0; i < _count; i++)
        {
            var x = variant & (1 << i);
            result[i] = x != 0 ? _backpack.Volumes[i] : 0;
            s += result[i];
        }

        _results.Add(new Tuple<uint[], float>(result, bestFit));
        return Task.CompletedTask;

        float CalcVariant(uint v)
        {
            var volume = 0ul;
            var fit = 0f;

            for (var i = 0; i < _count; i++)
            {
                var c = (uint)1 << i;
                var x = v & c;
                if (x == 0)
                {
                    continue;
                }

                var itemVolume = _backpack.Volumes[i];
                var newVolume = volume + itemVolume;
                if (newVolume > _backpack.Size)
                {
                    fit = float.MinValue;
                    break;
                }

                volume = newVolume;

                var weight = _backpack.Weights[i];
                fit += itemVolume * weight;
            }

            return fit;
        }
    }
}