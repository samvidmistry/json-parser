namespace Parser;

public class String : JsonObject
{
    public readonly string Value;

    public String(string value)
    {
        this.Value = value;
    }

    public override string Print()
    {
        return this.Value;
    }
}
