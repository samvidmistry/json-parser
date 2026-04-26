using System.Text;

namespace Parser;

public class Array(IList<JsonObject> members) : JsonObject
{
    public readonly IList<JsonObject> Members = members;

    public override string Print()
    {
        var sb = new StringBuilder();
        sb.Append("[\n");
        foreach(var m in this.Members)
        {
            sb.Append($"\t{m.ToString()},\n");
        }
        sb.Append("]\n");

        return sb.ToString();
    }
}
