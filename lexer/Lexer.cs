namespace Lexer;

/// <summary>
/// A lexer for JSON. Lexer is a stateful object that reads
/// tokens from a byte stream on-demand. The class is
/// primarily designed to be used in a single pass.
/// There is no way to seek or rewind the stream of tokens.
/// <remark>
/// The API is inspired by <see cref="StreamReader" /> class.
/// </remark>
/// </summary>
class Lexer
{
    // Stream is a low-level representation of byte data, StreamReader
    // is a higher level representation for working with text.
    private readonly StreamReader reader;
    private int index;
    
    // If the stream is not rewindable, we gotta store
    // the token that we peeked at to return
    private Token? peekedToken;

    public Lexer(StreamReader reader)
    {
	this.reader = reader;
	index = 0;
    }

    /// <summary>
    /// Peek at the next token without consuming it.
    /// </summary>
    public Token Peek()
    {
	if (this.peekedToken is null)
	{
	    this.peekedToken = this.Read();
	}

	return this.peekedToken;
    }

    private bool IsWhiteSpace(char c)
    {
	return c == ' ' || c == '\t' || c == '\r' || c == '\n';
    }

    private int ReadIgnoringWhiteSpace()
    {
	while (true)
	{
	    var c = this.reader.Read();
	    if (c == -1) return -1;
	    index++;
	    if (!IsWhiteSpace((char)c)) return c;
	}
    }

    /// <summary>
    /// Read the next token, advancing the stream.
    /// </summary>
    public Token Read()
    {
	if (this.peekedToken is not null)
	{
	    var temp = peekedToken;
	    this.peekedToken = null;
	    return temp;
	}

	var n = this.ReadIgnoringWhiteSpace();

	if (n == -1)
	{
	    return new Token(TokenType.EOF, index, index);
	}

	var c = (char)n;
	Token? returnToken;

	if (c == '{')
	{
	    returnToken = new Token(TokenType.BeginObject, index - 1, index);
	}
	else if (c == '}')
	{
	    returnToken = new Token(TokenType.EndObject, index - 1, index);
	}
	else if (c == '[')
	{
	    returnToken = new Token(TokenType.BeginArray, index - 1, index);
	}
	else if (c == ']')
	{
	    returnToken = new Token(TokenType.EndArray, index - 1, index);
	}
	else if (c == ':')
	{
	    returnToken = new Token(TokenType.NameSeparator, index - 1, index);
	}
	else if (c == ',')
	{
	    returnToken = new Token(TokenType.ValueSeparator, index - 1, index);
	}
	else if (c == '"')
	{
	    bool escaped = false;
	    var startIndex = index - 1;
	    
	    while (true)
	    {
		var next = (char)this.reader.Read();
		index++;

		if (escaped) { // string cannot end here
		    escaped = false;
		    continue;
		}

		if (next == '\\')
		{
		    escaped = true;
		    continue;
		}

		if (next == '"')
		{
		    break;
		}
	    }

	    returnToken = new Token(TokenType.String, startIndex, index);
	}
	else
	{
	    // Ideally the system should never throw exceptions and use
	    // Either to return errors, but I'm lazy to incorporate it in
	    // lexer now. :)
	    throw new InvalidOperationException($"Invalid character {c} found at {index - 1}");
	}

	return returnToken;
    }

    /// <summary>
    /// Read (lazily) to the end of token stream.
    /// Returns <see cref="TokenType.EOF" /> on EOF.
    /// </summary>
    public IEnumerable<Token> ReadToEnd()
    {
	while (true)
	{
	    // consumer's responsibility to deal with EOF
	    // we'll keep returning EOF if asked
	    yield return this.Read();
	}
    }
}
