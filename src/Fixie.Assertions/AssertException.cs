using System.Text;
using static Fixie.Assertions.StringUtilities;
using static System.Environment;

namespace Fixie.Assertions;

public class AssertException : Exception
{
    public string? Expression { get; }
    public string Expected { get; }
    public string Actual { get; }
    public bool HasMultilineRepresentation { get; }
    readonly string message;

    public AssertException(string? expression, string expected, string actual, string? message = null)
    {
        Expression = expression;
        Expected = expected;
        Actual = actual;

        if (message == null)
        {
            this.message = WriteMessage(Expression, Expected, Actual, out bool isMultiline);
            HasMultilineRepresentation = isMultiline;
        }
        else
        {
            this.message = message;
        }
    }

    public override string Message => message;

    static string WriteMessage(string? expression, string expected, string actual, out bool isMultiline)
    {
        isMultiline = !IsTrivial(expected) || !IsTrivial(actual);

        var content = new StringBuilder();

        content.Append(expression);
        content.Append(" should be");

        if (isMultiline)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(Indent(expected));
            content.AppendLine();
            content.AppendLine();
        }
        else
        {
            content.Append(' ');
            content.Append(expected);
            content.Append(' ');
        }

        content.Append("but was");

        if (isMultiline)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(Indent(actual));
        }
        else
        {
            content.Append(' ');
            content.Append(actual);
        }

        if (expected == actual)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(
                "These serialized values are identical. Did you mean to perform " +
                "a structural comparison with `ShouldMatch` instead?");
        }

        return content.ToString();
    }

    static bool IsTrivial(string value)
    {
        return
            value == "null" || value == "true" || value == "false" ||
            value.StartsWith('\'') ||
            (value.Length > 0 && char.IsDigit(value[0]));
    }

    public override string? StackTrace => FilterStackTrace(base.StackTrace);

    static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

    static string? FilterStackTrace(string? stackTrace)
    {
        if (stackTrace == null)
            return null;

        List<string> results = [];

        foreach (var line in stackTrace.Split(NewLine))
        {
            var trimmedLine = line.TrimStart();
            if (!trimmedLine.StartsWith("at " + FilterStackTraceAssemblyPrefix))
                results.Add(line);
        }

        return string.Join(NewLine, results.ToArray());
    }
}