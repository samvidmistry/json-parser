using System.Text;

namespace Parser;

/// <summary>
/// Represents an Object in JSON representation.
/// </summary>
public class Object : JsonObject
{
    public readonly IDictionary<string, JsonObject> Members;

    public Object()
    {
	this.Members = new Dictionary<string, JsonObject>();
    }

    public Object(IDictionary<string, JsonObject> m)
    {
	this.Members = m;
    }

    public override string Print()
    {
        var builder = new StringBuilder();
	builder.Append("{\n");
	foreach (var m in this.Members)
	{
	    builder.Append($"\t{m.Key}: {m.Value.Print()}\n");
	}
	builder.Append("}\n");
	return builder.ToString();
    }
}
