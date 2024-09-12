﻿using static Fixie.Assertions.StringUtilities;
using static System.Environment;

namespace Fixie.Assertions;

static class Serialization
{
    static string Serialize(bool x) => x ? "true" : "false";

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

    public static string SerializeList<T>(T[] items)
    {
        var formattedItems = string.Join("," + NewLine, items.Select(arg => "    " + SerializeByType(arg)));

        return $"[{NewLine}{formattedItems}{NewLine}]";
    }

    public static string SerializeByType<T>(T any)
    {
        if (any == null) return "null";

        if (typeof(T) == typeof(bool))
            return Serialize((bool)(object)any);

        if (typeof(T) == typeof(char))
            return Serialize((char)(object)any);

        if (typeof(T) == typeof(string))
            return Serialize((string)(object)any);

        if (typeof(T) == typeof(Type))
            return Serialize((Type)(object)any);

        return any.ToString() ?? any.GetType().ToString();
    }
}