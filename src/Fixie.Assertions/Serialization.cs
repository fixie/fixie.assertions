using static Fixie.Assertions.StringUtilities;
using static System.Environment;

namespace Fixie.Assertions;

static class Serialization
{
    public static string Serialize(object? any)
    {
        if (any == null) return CsonSerializer.Serialize(any);

        var type = any.GetType();

        if (type == typeof(bool))
            return CsonSerializer.Serialize((bool)any);

        if (type == typeof(char))
            return Serialize((char)any);

        if (type == typeof(string))
            return Serialize((string)any);

        if (type.IsArray)
            return Serialize((Array)any);

        if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
            return Serialize((Type)any);

        return any.ToString() ?? any.GetType().ToString();
    }

    static string Serialize(Array items)
    {
        var formattedItems = string.Join("," + NewLine, items.Cast<object>().Select(arg => "    " + Serialize(arg)));

        return $"[{NewLine}{formattedItems}{NewLine}]";
    }

    static string Serialize(char x) => $"'{Escape(x)}'";

    static string Serialize(string x)
    {
        if (IsMultiline(x))
        {
            var terminal = RawStringTerminal(x);

            return $"{terminal}{NewLine}{x}{NewLine}{terminal}";
        }
        
        return $"\"{string.Join("", x.Select(Escape))}\"";
    }

    static string RawStringTerminal(string x)
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

        return new string('\"', longestDoubleQuoteSequence < 3 ? 3 : longestDoubleQuoteSequence + 1);
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

    static string Serialize(Type x) =>
        $"typeof({x switch
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
        }})";
}