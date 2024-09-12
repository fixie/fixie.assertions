using static System.Environment;

namespace Fixie.Assertions;

static class StringUtilities
{
    public static string Indent(string multiline) =>
        string.Join(NewLine, multiline.Split(NewLine).Select(x => $"    {x}"));

    public static bool IsMultiline(string value)
    {
        var lines = value.Split(NewLine);

        return lines.Length > 1 && lines.All(line => !line.Contains("\r") && !line.Contains("\n"));
    }
}