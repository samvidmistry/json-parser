using System.Text;

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
internal class Lexer(StreamReader reader)
{
    // Stream is a low-level representation of byte data, StreamReader
    // is a higher level representation for working with text.
    private readonly StreamReader reader = reader;
    private int index;

    private static char[] escapeChars =
        ['"', '\\', '/', 'b', 'f', 'n', 'r', 't', 'u'];

    // If the stream is not rewindable, we gotta store
    // the token that we peeked at to return
    private Token? peekedToken;

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

    private static bool IsWhiteSpace(char c)
    {
        return c is ' ' or '\t' or '\r' or '\n';
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

                if (escaped)
                {
                    // We must make sure only one of the
                    // escaped characters show up
                    if (!escapeChars.Contains(next))
                    {
                        throw new InvalidDataException($"Invalid escape character {next} in string literal.");
                    }

                    if (next == 'u')
                    {
                        // unicode codepoint literal
                        for (int i = 0; i < 4; i++)
                        {
                            next = (char)this.reader.Read();
                            index++;

                            // check if it is in hex range
                            if (next is not (>= '0' and <= '9' or
                                  >= 'a' and <= 'f' or
                                  >= 'A' and <= 'F'))
                            {
                                throw new InvalidDataException($"Invalid unicode literal character {next} in string literal.");
                            }
                        }
                    }

                    // string cannot end here
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

                // check if it is a control character
                if (next < 0x20)
                {
                    throw new InvalidDataException($"Invalid control character {next} in string literal.");
                }
            }

            returnToken = new Token(TokenType.String, startIndex, index);
        }
        else if (c == '-' || char.IsDigit(c))
        {
            var start = index - 1;
            var sb = new StringBuilder();
            _ = sb.Append((char)c);
            // parse until white space or 28 characters max
            // TODO: System supports only 28 digit long numbers which is already too much?
            for (int i = 0; i < 28; i++)
            {
                var cc = this.reader.Peek();
                if (IsWhiteSpace((char)cc))
                {
                    break;
                }

                this.reader.Read(); // consume peeked char
                index++;  // Maybe I should've tied the read and index increment at a single place
                _ = sb.Append((char)cc);
            }

            var num = sb.ToString();
            return decimal.TryParse(num, out _)
                ? new Token(TokenType.Number, start, index)
                : throw new InvalidOperationException($"Number cannot be parser: {num}");
        }
        else if (c == 't')
        {
            var expected = new char[] { 'r', 'u', 'e' };
            foreach (char ch in expected)
            {
                var cc = this.reader.Read();
                index++;
                if ((char)cc != ch)
                {
                    throw new InvalidOperationException($"Unexpected character {cc} at {index - 1}.");
                }
            }

            return new Token(TokenType.Boolean, index - 4, index);
        }
        else if (c == 'f')
        {
            var expected = new char[] { 'a', 'l', 's', 'e' };
            foreach (char ch in expected)
            {
                var cc = this.reader.Read();
                index++;
                if ((char)cc != ch)
                {
                    throw new InvalidOperationException($"Unexpected character {cc} at {index - 1}.");
                }
            }

            return new Token(TokenType.Boolean, index - 5, index);
        }
        else if (c == 'n')
        {
            var expected = new char[] { 'u', 'l', 'l' };
            foreach (char ch in expected)
            {
                var cc = this.reader.Read();
                index++;
                if ((char)cc != ch)
                {
                    throw new InvalidOperationException($"Unexpected character {cc} at {index - 1}.");
                }
            }

            return new Token(TokenType.Null, index - 4, index - 1);
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
