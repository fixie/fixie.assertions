using System.Text;

namespace Fixie.Assertions;

class CsonWriter(StringBuilder output)
{
    readonly StringBuilder output = output;
    int indentation = 0;

    public void Write(char content)
    {
        output.Append(content);
    }

    public void Write(string content)
    {
        output.Append(content);
    }

    public void WriteItems<T>(char open, IEnumerable<T> items, char close, Action<T> writeItem)
    {
        Write(open);

        bool any = false;
        foreach (var item in items)
        {
            if (!any)
            {
                StartItems();
                any = true;
            }
            else
                WriteItemSeparator();

            writeItem(item);
        }

        if (any)
            EndItems();

        Write(close);
    }

    void StartItems()
    {
        output.AppendLine();
        
        indentation++;
        if (indentation > 63)
            throw new CsonException("This type could not be serialized because the object graph is too deep.");

        WriteIndentation();
    }

    void WriteItemSeparator()
    {
        output.Append(',');
        output.AppendLine();
        WriteIndentation();
    }

    void EndItems()
    {
        output.AppendLine();
        indentation--;
        WriteIndentation();
    }

    void WriteIndentation()
    {
        for (int i = 0;  i < indentation; i++)
            output.Append("  ");
    }
}