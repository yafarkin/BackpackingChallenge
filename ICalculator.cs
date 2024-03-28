namespace BackpackingChallenge;

public interface ICalculator
{
    Task<IEnumerable<Item?>> CalcAsync();
}