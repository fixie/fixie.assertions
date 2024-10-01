using System.Numerics;
using System.Reflection;
using System.Text;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

class CsonWriter(StringBuilder output)
{
    readonly StringBuilder output = output;
    int indentation = 0;
    int lengthAtLineStart = 0;

    public void WriteNull()
    {
        Append("null");
    }

    public void WriteBool(bool value)
    {
        Append(value ? "true" : "false");
    }

    public void WriteNumber<T>(T value) where T : struct, INumber<T>
    {
        Append(value.ToString());
    }

    public void WriteChar(char value)
    {
        Append('\'');
        Append(Escape(value));
        Append('\'');
    }

    public void WriteGuid(Guid value)
    {
        Append('\"');
        Append(value.ToString());
        Append('\"');
    }

    public void WriteString(string value)
    {
        if (IsMultiline(value))
        {
            var lengthAtOpenTerminalStart = output.Length;
            var indentationLength = lengthAtOpenTerminalStart-lengthAtLineStart;
            var terminalLength = RawStringTerminalLength(value);

            Append('\"', terminalLength);
            AppendLine();

            foreach (var content in value.Split(Environment.NewLine))
            {
                Append(' ', indentationLength);
                Append(content);
                AppendLine();
            }

            Append(' ', indentationLength);
            Append('\"', terminalLength);
        }
        else
        {
            Append('\"');
            foreach (var c in value)
                Append(Escape(c));
            Append('\"');
        }
    }

    public void WriteType(Type value)
    {
        bool nullable = false;
        var underlyingType = Nullable.GetUnderlyingType(value);

        Append("typeof(");

        if (underlyingType != null)
        {
            value = underlyingType;
            nullable = true;
        }

        Append(TypeName(value));

        if (nullable)
            Append('?');
        
        Append(')');
    }

    public void WriteEnum<T>(T value) where T : struct, Enum
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            Append(typeof(T).FullName);
            Append('.');
            Append(value);
            return;
        }
            
        var numeric = value.ToString();
        bool negative = numeric.StartsWith('-');

        Append('(');
        Append(typeof(T).FullName);
        Append(')');

        if (negative)
            Append('(');

        Append(value);

        if (negative)
            Append(')');
    }

    public void WriteList<TItem>(IEnumerable<TItem> items)
    {
        WriteItems('[', items, ']', WriteSerialized);
    }

    public void WritePairs<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        WriteItems('{', pairs, '}', pair =>
        {
            Append('[');
            WriteSerialized(pair.Key);
            Append("] = ");
            WriteSerialized(pair.Value);
        });
    }

    public void WriteProperties<T>(T value)
    {
        var fields =
            typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(field => (field.Name, Value: field.GetValue(value)));

        var properties =
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetIndexParameters().Length == 0)
                .Where(property =>
                {
                    // property.CanRead is insufficient, because it is true
                    // even for private get accessors on public properties.
                    //
                    // GetGetMethod() returns only public accessors by default.

                    return property.GetGetMethod() != null;
                })
                .Select(property => (property.Name, Value: property.GetValue(value)));

        WriteItems('{', fields.Concat(properties), '}', property =>
        {
            Append(property.Name);
            Append(" = ");
            WriteSerialized(property.Value);
        });
    }

    public void WriteItems<T>(char open, IEnumerable<T> items, char close, Action<T> writeItem)
    {
        Append(open);

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

        Append(close);
    }

    void StartItems()
    {
        AppendLine();
        
        indentation++;
        if (indentation > 31)
            throw new SerializationDepthException(output.ToString());

        AppendIndentation();
    }

    void WriteItemSeparator()
    {
        Append(',');
        AppendLine();
        AppendIndentation();
    }

    void EndItems()
    {
        AppendLine();
        indentation--;
        AppendIndentation();
    }

    void AppendIndentation()
    {
        Append(' ', indentation*2);
    }

    static int RawStringTerminalLength(string x)
    {
        var longestDoubleQuoteSequence = 0;
        var currentDoubleQuoteSequence = 0;

        foreach (var c in x)
        {
            if (c != '\"')
            {
                currentDoubleQuoteSequence = 0;
                continue;
            }

            currentDoubleQuoteSequence++;
            if (currentDoubleQuoteSequence > longestDoubleQuoteSequence)
                longestDoubleQuoteSequence = currentDoubleQuoteSequence;
        }

        if (longestDoubleQuoteSequence < 3)
            return 3;

        return longestDoubleQuoteSequence + 1;
    }

    static string Escape(char x) =>
        x switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\t' => @"\t",
            '\n' => @"\n",
            '\v' => @"\v",
            '\f' => @"\f",
            '\r' => @"\r",
            //'\e' => @"\e", TODO: Applicable in C# 13
            ' ' => " ",
            '"' => @"\""",
            '\'' => @"\'",
            '\\' => @"\\",
            _ when (char.IsControl(x) || char.IsWhiteSpace(x)) => $"\\u{(int)x:X4}",
            _ => x.ToString()
        };

    static string TypeName(Type x)
        => x switch
            {
                _ when x == typeof(bool) => "bool",
                _ when x == typeof(sbyte) => "sbyte",
                _ when x == typeof(byte) => "byte",
                _ when x == typeof(short) => "short",
                _ when x == typeof(ushort) => "ushort",
                _ when x == typeof(int) => "int",
                _ when x == typeof(uint) => "uint",
                _ when x == typeof(long) => "long",
                _ when x == typeof(ulong) => "ulong",
                _ when x == typeof(nint) => "nint",
                _ when x == typeof(nuint) => "nuint",
                _ when x == typeof(decimal) => "decimal",
                _ when x == typeof(double) => "double",
                _ when x == typeof(float) => "float",
                _ when x == typeof(char) => "char",
                _ when x == typeof(string) => "string",
                _ when x == typeof(object) => "object",
                _ => x.ToString()
            };

    void WriteSerialized<T>(T value)
        => CsonSerializer.SerializeInternal(this, value);

    void Append(char content) => output.Append(content);
    void Append(char content, int repeatCount) => output.Append(content, repeatCount);
    void Append(string? content) => output.Append(content);
    void Append(object? content) => output.Append(content);
    
    void AppendLine()
    {
        output.AppendLine();
        lengthAtLineStart = output.Length;
    }
}