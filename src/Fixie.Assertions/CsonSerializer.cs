﻿using static Fixie.Assertions.StringUtilities;
using static System.Environment;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(char x) => $"'{Escape(x)}'";

    static string Serialize(Guid x) => Serialize(x.ToString());

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

    static string Serialize(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);

        if (underlyingType != null)
            return $"typeof({TypeName(underlyingType)}?)";

        return $"typeof({TypeName(type)})";
    }

    private static string TypeName(Type x)
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
}