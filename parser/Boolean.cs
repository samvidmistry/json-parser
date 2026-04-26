namespace Parser;

public class Boolean(bool b) : JsonObject
{
    private readonly bool Value = b;

    public override string Print()
    {
        return this.Value ? "true" : "false";
    }
}
