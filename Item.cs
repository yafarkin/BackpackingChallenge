namespace BackpackingChallenge;

public class Item : Tuple<string, float, float>
{
    public string Name => Item1;
    public float Volume => Item2;
    public float Weight => Item3;

    public Item(string item1, float item2, float item3)
        : base(item1, item2, item3)
    {
    }
}