using System.Text;

namespace Fixie.Assertions;

class CsonWriter(StringBuilder output)
{
    readonly StringBuilder output = output;
    int indentation = 0;
    private List<string> path = [];

    private void WriteIndentation()
    {
        for (int i = 0;  i < indentation; i++)
            output.Append("  ");
    }

    private void Indent()
    {
        indentation++;
        if (indentation > 63)
            throw new CsonException("This type could not be serialized because the object graph is too deep.");
    }

    private void Dedent()
    {
        indentation--;
    }

    public void WriteRawValue(string json)
    {
        output.Append(json);
    }

    public void WriteStringValue(string? value)
    {
        output.Append($"\"{value}\"");
    }

    public void StartItems()
    {
        output.AppendLine();
        Indent();
        WriteIndentation();
    }

    public void WriteItemSeparator()
    {
        output.AppendLine(",");
        WriteIndentation();
    }

    public void EndItems()
    {
        output.AppendLine();
        Dedent();
        WriteIndentation();
    }

    public void WriteStartArray()
    {
        output.Append('[');
    }

    public void WriteEndArray()
    {
        output.Append(']');
    }

    public void WriteStartObject()
    {
        output.Append('{');
    }

    public void WritePropertyName(string propertyName)
    {
        output.Append($"\"{propertyName}\": ");
    }

    public void WriteEndObject()
    {
        output.Append('}');
    }
}