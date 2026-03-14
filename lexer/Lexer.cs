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
    private long index;
    
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

	if (this.reader.Peek() == -1)
	{
	    return new Token(TokenType.EOF, index, index);
	}

	// TODO: Supporting only ASCII for now
	// TODO: Step 1, only 2 characters necessary
	var c = (char)this.reader.Read();
	Token? returnToken;

	// TODO: Support all characters
	if (c == '{')
	{
	    returnToken = new Token(TokenType.BeginObject, index, index + 1);
	    index++;
	}
	else if (c == '}')
	{
	    returnToken = new Token(TokenType.EndObject, index, index + 1);
	    index++;
	}
	else
	{
	    throw new InvalidOperationException("unsupported case (for now?)");
	}

	return returnToken;
    }

    /// <summary>
    /// Read (lazily) to the end of token stream.
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
