using static System.Environment;

namespace Fixie.Assertions;

static class StringUtilities
{
    public static string Indent(string multiline) =>
        string.Join(NewLine, multiline.Split(NewLine).Select(x => $"    {x}"));

    public static bool IsMultiline(string value)
    {
        var lines = value.Split(NewLine);

        return lines.Length > 1 && lines.All(line => !line.Contains('\r') && !line.Contains('\n'));
    }

    public static string TypeName(Type x)
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