namespace BackpackingChallenge;

public class Backpack
{
    public Item[] Items = Array.Empty<Item>();
    public byte Count => Convert.ToByte(Items.Length);

    public float Limit;
}