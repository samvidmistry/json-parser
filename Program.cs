var parser = new Parser.Parser(args[0]);
var jsonObject = parser.Parse();
if (jsonObject.GetError() is not null)
{
    Console.WriteLine(jsonObject.GetError().ErrorMessage);
    return 1;
}
else
{
    Console.WriteLine(jsonObject.GetObject());
    return 0;
}
