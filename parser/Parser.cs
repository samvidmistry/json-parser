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

            if (t.Type == TokenType.BeginObject)
            {
                return this.ParseObject().GetAs<JsonObject>();
            }
            else if (t.Type == TokenType.String)
            {
                return this.ParseString().GetAs<JsonObject>();
            }
            else if (t.Type == TokenType.Number)
            {
                return this.ParseNumber().GetAs<JsonObject>();
            }
            else if (t.Type == TokenType.Boolean)
            {
                return this.ParseBoolean().GetAs<JsonObject>();
            }
            else if (t.Type == TokenType.Null)
            {
                return this.ParseNull().GetAs<JsonObject>();
            }
            else if (t.Type == TokenType.BeginArray)
            {
                return this.ParseArray().GetAs<JsonObject>();
            }
            else
            {
                return new Either<JsonObject>(new Error("Unsupported token type."));
            }
        }
    }

    private Either<Array> ParseArray()
    {
        var t = this.lexer.Read();
        if (t.Type != TokenType.BeginArray)
        {
            return new Either<Array>(this.CreateUnexpectedTokenError(t, TokenType.BeginArray));
        }

        var members = new List<JsonObject>();
        while (true)
        {
            t = this.lexer.Peek();
            if (t.Type == TokenType.EndArray)
            {
                _ = this.lexer.Read();
                return new Either<Array>(new Array(members));
            }

            if (members.Count == 0 && t.Type == TokenType.ValueSeparator)
            {
                return new Either<Array>(new Error("Encountered value-separator before array elements"));
            }

            if (t.Type == TokenType.ValueSeparator)
            {
                _ = this.lexer.Read();
                continue;
            }

            var element = this.Parse();
            if (element.GetError() is not null)
            {
                return new Either<Array>(element.GetError());
            }

            members.Add(element.GetObject());
        }

        throw new InvalidOperationException("The parser seems to have run into a bug. Look what you made it do!!!");
    }

    private Either<Null> ParseNull()
    {
        var t = this.lexer.Read();
        if (t.Type != TokenType.Null)
        {
            return new Either<Null>(this.CreateUnexpectedTokenError(t, TokenType.Null));
        }

        return new Either<Null>(new Null());
    }

    /// <summary>
    /// Parse a boolean coing in token stream.
    /// </summary>
    private Either<Boolean> ParseBoolean()
    {
        var t = this.lexer.Read();
        if (t.Type != TokenType.Boolean)
        {
            return new Either<Boolean>(this.CreateUnexpectedTokenError(t, TokenType.Boolean));
        }

        return new Either<Boolean>(new Boolean(bool.Parse(this.file.Substring(t.Start, t.End - t.Start))));
    }

    /// <summary>
    /// Parse a number coming in token stream.
    /// </summary>
    public Either<Number> ParseNumber()
    {
        var t = this.lexer.Read();
        if (t.Type != TokenType.Number)
        {
            return new Either<Number>(this.CreateUnexpectedTokenError(t, TokenType.Number));
        }

        return new Either<Number>(new Number(decimal.Parse(this.file.Substring(t.Start, t.End - t.Start))));
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

    /// <summary>
    /// Parse a string coming in token stream.
    /// </summary>
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
