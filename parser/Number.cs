namespace Parser;

public class Number(double value) : JsonObject
{
    private readonly double Value = value;

    public override string Print()
    {
        return this.Value.ToString();
    }
}
