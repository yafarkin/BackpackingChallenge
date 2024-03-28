namespace BackpackingChallenge;

public class Backpack
{
    public readonly Item[] Items;
    public int Count => Items.Length;

    public readonly float Limit;

    public Backpack(float limit, IEnumerable<Item> items)
    {
        Limit = limit;

        var tmpItems = new List<Item>();

        foreach (var item in items)
        {
            tmpItems.AddRange(Enumerable.Repeat(item, item.Count));
        }

        Items = tmpItems.ToArray();
    }
}