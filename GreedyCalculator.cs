namespace BackpackingChallenge;

public class GreedyCalculator : ICalculator
{
    private readonly Backpack _backpack;

    public GreedyCalculator(Backpack backpack)
    {
        _backpack = backpack;
    }

    public Task<IEnumerable<Item>> CalcAsync()
    {
        var result = new List<Item>();
        var items = _backpack.Items.OrderByDescending(x => x.Weight).ThenBy(x => x.Volume);

        var totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.Volume;
            if (totalWeight > _backpack.Limit)
            {
                break;
            }

            result.Add(item);
        }

        return Task.FromResult(result.AsEnumerable());
    }
}