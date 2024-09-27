using System.Text;

namespace Fixie.Assertions;

class CsonWriter(StringBuilder output)
{
    readonly StringBuilder output = output;
    int indentation = 0;

    public void StartItems()
    {
        output.AppendLine();
        Indent();
        WriteIndentation();
    }

    public void WriteItemSeparator()
    {
        output.Append(',');
        output.AppendLine();
        WriteIndentation();
    }

    public void EndItems()
    {
        output.AppendLine();
        Dedent();
        WriteIndentation();
    }

    public void Write(char content)
    {
        output.Append(content);
    }

    public void Write(string content)
    {
        output.Append(content);
    }

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
}