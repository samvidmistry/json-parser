using Lexer;
using Primitives;

namespace Parser;

public class Parser
{
    private readonly Lexer.Lexer lexer;
    private readonly string file;

    // TODO: Design a better input
    public Parser(string fileName)
    {
	this.file = File.ReadAllText(fileName);
	this.lexer = new Lexer.Lexer(new StreamReader(new MemoryStream(File.ReadAllBytes(fileName))));
    }

    public Either<JsonObject> Parse()
    {
	// TODO: I will currently return the first full object I find in a file
	
	while (true)
	{
	    var t = this.lexer.Peek();
	    if (t.Type == TokenType.EOF)
	    {
		return new Either<JsonObject>(new Error($"Encountered unexpected EOF at {t.Start}."));
	    }

	    // TODO: Support all tokens
	    if (t.Type == TokenType.BeginObject)
	    {
		return this.ParseObject();
	    }
	}
    }

    private Either<JsonObject> ParseObject()
    {
	var t = this.lexer.Read();
	if (t.Type != TokenType.BeginObject)
	{
	    // TODO: Should I return an actual Object or JsonObject here?
	    return new Either<JsonObject>(this.CreateUnexpectedTokenError(TokenType.BeginObject, t.Type));
	}

	// TODO: Handle rest of the object

	t = this.lexer.Read();
	if (t.Type != TokenType.EndObject)
	{
	    return new Either<JsonObject>(this.CreateUnexpectedTokenError(TokenType.EndObject, t.Type));
	}
	
	return new Either<JsonObject>(new Object());
    }

    private Error CreateUnexpectedTokenError(TokenType expected, TokenType actual)
    {
	return new Error($"Expected {expected}, found {actual}");
    }
}
