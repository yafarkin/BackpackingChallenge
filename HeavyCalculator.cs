using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace BackpackingChallenge;

public class HeavyCalculator : ICalculator
{
    private readonly ConcurrentBag<Tuple<float, Item?[]>> _results = new();
    private readonly uint _batchSize = 1 << 20;
    private readonly Backpack _backpack;
    private volatile bool _isFinished;

    public HeavyCalculator(Backpack backpack)
    {
        _backpack = backpack;
    }

    public async Task<IEnumerable<Item?>> CalcAsync()
    {
        var taskCount = Convert.ToInt32(Math.Max(2, Environment.ProcessorCount * 0.75));

        var tasks = new List<Task>(taskCount);

        var bits = new BitArray(_backpack.Count);

        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var periodToShow = TimeSpan.FromSeconds(5);

        var pows = new float[bits.Length];
        var maxPow = 0f;
        for (var i = 0; i < bits.Length; i++)
        {
            var pow = (float)Math.Pow(2, i);
            pows[i] = pow;
            maxPow += pow;
        }

        while (!_isFinished)
        {
            if (tasks.Count < taskCount)
            {
                Console.Write(".");
                tasks.Add(CalcAsync(bits, _batchSize));
                bits = AddValue(bits, _batchSize);
                if (bits.Length > _backpack.Count)
                {
                    break;
                }

                if (sw.Elapsed > periodToShow)
                {
                    var percent = 0d;
                    for (var i = 0; i < bits.Length; i++)
                    {
                        if (bits[i])
                        {
                            var bitPercent = pows[i];
                            percent += bitPercent / maxPow;
                        }
                    }

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

    private async Task CalcAsync(BitArray from, uint count)
    {
        await Task.Yield();

        var variant = new BitArray(from);
        var bits = new BitArray(from);
        var bestFit = float.MinValue;

        for(var i = 0; i < count; i++)
        {
            var r = CalcVariant(bits);
            if (r > bestFit)
            {
                bestFit = r;
                variant = new BitArray(bits);

                if (_backpack.Limit - bestFit == 0)
                {
                    _isFinished = true;
                    break;
                }
            }

            if (_isFinished)
            {
                break;
            }

            if (IncrementArray(bits))
            {
                _isFinished = true;
            }
        }

        var result = new List<Item?>();
        for (var i = 0; i < _backpack.Count; i++)
        {
            if (variant[i])
            {
                result.Add(_backpack.Items[i]);
            }
        }

        _results.Add(new Tuple<float, Item?[]>(bestFit, result.ToArray()));
    }

    private float CalcVariant(BitArray bits)
    {
        var volume = 0f;
        var fit = 0f;

        for (var i = 0; i < bits.Length; i++)
        {
            if (!bits[i])
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

    private BitArray AddBitArrays(BitArray src, BitArray valueToAdd)
    {
        var result = new BitArray(src.Length);

        var carry = false; // Флаг переноса

        for (var i = 0; i < src.Length; i++)
        {
            var bit1 = i < src.Length && src[i];
            var bit2 = i < valueToAdd.Length && valueToAdd[i];

            // Вычисляем сумму текущих битов и переноса
            var sum = bit1 ^ bit2 ^ carry;
            carry = (bit1 && bit2) || (bit1 && carry) || (bit2 && carry);

            result[i] = sum;
        }

        // Если есть перенос на старшем разряде, увеличиваем длину результата и устанавливаем единицу
        if (carry)
        {
            result.Length++;
            result[^1] = true;
        }

        return result;
    }

    private BitArray AddValue(BitArray bits, uint value)
    {
        var valueBits = new BitArray(BitConverter.GetBytes(value));
        return AddBitArrays(bits, valueBits);
    }

    private bool IncrementArray(BitArray bits)
    {
        var carry = 1;
        for (var i = 0; i < bits.Length && carry > 0; i++)
        {
            var currentBit = bits[i];
            bits[i] = currentBit ^ true;
            carry = currentBit ? 1 : 0;
        }

        return bits.HasAllSet();
    }
}