using Lexer;
using Primitives;

namespace Parser;

/// <summary>
/// Parser provides methods to extract JSON
/// constructs from a stream of characters.
/// </summary>
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

    /// <summary>
    /// Parse the next construct in the stream of text.
    /// It returns the first syntactically valid full construct
    /// it finds in the character stream.
    /// </summary>
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
		return this.ParseObject().GetAs<JsonObject>();
	    }
	    else if (t.Type == TokenType.String) {
		return this.ParseString().GetAs<JsonObject>();
	    }
	    else
	    {
		throw new InvalidOperationException("Unsupported Token Type");
	    }
	}
    }

    /// <summary>
    /// Parses a JSON Object coming up in the stream.
    /// </summary>
    public Either<Object> ParseObject()
    {
	var t = this.lexer.Read();
	if (t.Type != TokenType.BeginObject)
	{
	    return new Either<Object>(this.CreateUnexpectedTokenError(t, TokenType.BeginObject));
	}

	var members = new Dictionary<string, JsonObject>();
	while (true)
	{
	    t = this.lexer.Peek();
	    if (t.Type == TokenType.EndObject)
	    {
		this.lexer.Read(); // consume the token
		return new Either<Object>(new Object(members));
	    }
	    else if (t.Type == TokenType.String || t.Type == TokenType.ValueSeparator)
	    {
		if (t.Type == TokenType.ValueSeparator)
		{
		    if (members.Count == 0)
		    {
			return new Either<Object>(new Error("Leading comma found before object members."));
		    }

		    this.lexer.Read(); // consume ValueSeparator
		}
		
		// parse member → string name-separator json-object
		var keyString = this.ParseString();
		if (keyString.GetError() is not null)
		{
		    return new Either<Object>(keyString.GetError());
		}
		var key = keyString.GetObject().Value;
		t = this.lexer.Read();
		if (t.Type != TokenType.NameSeparator)
		{
		    return new Either<Object>(this.CreateUnexpectedTokenError(t, TokenType.NameSeparator));
		}

		var jsonObject = this.Parse();
		if (jsonObject.GetError() is not null)
		{
		    return new Either<Object>(jsonObject.GetError()!);
		}

		members.Add(key, jsonObject.GetObject());
	    }
	    else
	    {
		return new Either<Object>(this.CreateUnexpectedTokenError(t, TokenType.EndObject, TokenType.String));
	    }
	    
	}

	throw new InvalidOperationException("The parser seems to have run into a bug. Look what you made it do!!!");
    }

    public Either<String> ParseString()
    {
	var t = this.lexer.Read();
	if (t.Type != TokenType.String)
	{
	    return new Either<String>(this.CreateUnexpectedTokenError(t, TokenType.String));
	}

	// moving by 1 from both sides to exclude quotes
	return new Either<String>(new String(this.file.Substring(t.Start + 1, t.End - (t.Start + 1) - 1)));
    }

    private Error CreateUnexpectedTokenError(Token actual, params TokenType[] expected)
    {
	return new Error($"Expected {System.String.Join(", ", expected.Select(t => t.ToString()))} at ${actual.Start}, found {actual.Type}");
    }
}
