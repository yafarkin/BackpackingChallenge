namespace BackpackingChallenge;

public class Backpack
{
    public uint[] Volumes = null!;
    public byte[] Weights = null!;

    public byte Count => Convert.ToByte(Volumes.Length);

    public ulong Size;
}