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
        HasMultilineRepresentation = IsMultiline(expected) || IsMultiline(actual);

        Expression = expression;
        Expected = expected;
        Actual = actual;

        if (message == null)
        {
            this.message = HasMultilineRepresentation
                ? MultilineMessage(Expression, Expected, Actual)
                : ScalarMessage(Expression, Expected, Actual);

            if (Expected == Actual)
                this.message = this.message + $"{NewLine}{NewLine}These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?";
        }
        else
        {
            this.message = message;
        }
    }

    public override string Message => message;

    static string MultilineMessage(string? expression, string expected, string actual)
        => $"{expression} should be{NewLine}{NewLine}{Indent(expected)}{NewLine}{NewLine}" +
           $"but was{NewLine}{NewLine}{Indent(actual)}";

    static string ScalarMessage(string? expression, string expected, string actual)
        => $"{expression} should be {expected} but was {actual}";

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