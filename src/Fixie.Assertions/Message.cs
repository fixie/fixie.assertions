using System.Text;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

class Message
{
    readonly StringBuilder output = new();

    bool endsWithBlock = false;

    public Message Write(params string[] parts)
    {
        if (endsWithBlock)
            Blank();

        foreach (var part in parts)
            output.Append(part);

        endsWithBlock = false;

        return this;
    }

    public Message Block(string callout)
    {
        Blank();
        output.Append(Indent(callout));

        endsWithBlock = true;

        return this;
    }

    public Message Blank()
    {
        output.AppendLine();
        output.AppendLine();

        endsWithBlock = false;

        return this;
    }

    public Message Serialize<T>(T value)
    {
        Block(Serializer.Serialize(value));
        return this;
    }

    public Message ShouldHaveThrown<TException>(string expression, string? expectedMessage) where TException : Exception
    {
        var expectedType = TypeName(typeof(TException));

        Write(expression, " should have thrown ", expectedType);

        if (expectedMessage == null)
        {
            Blank();
        }
        else
        {
            Write(" with message");
            Serialize(expectedMessage);
        }

        return this;
    }

    public override string ToString() =>
        output.ToString();

    static string Indent(string multiline) =>
        string.Join(Environment.NewLine, multiline.Split(Environment.NewLine).Select(x => $"    {x}"));
}