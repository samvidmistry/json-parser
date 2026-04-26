namespace Parser;

public class Number(decimal value) : JsonObject
{
    private readonly decimal Value = value;

    public override string Print()
    {
        return this.Value.ToString();
    }
}
