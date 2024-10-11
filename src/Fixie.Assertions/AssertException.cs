using static System.Environment;

namespace Fixie.Assertions;

public class AssertException : Exception
{
    public string Expression { get; }
    public string Expected { get; }
    public string Actual { get; }
    public bool HasMultilineRepresentation { get; }

    public AssertException(string expression, string expected, string actual, string message, bool hasMultilineRepresentation = false)
        : base(message)
    {
        Expression = expression;
        Expected = expected;
        Actual = actual;
        HasMultilineRepresentation = hasMultilineRepresentation;
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

public class ComparisonException : AssertException
{
    public ComparisonException(string expression, string expected, string actual, string message)
        : base(expression, expected, actual, message, true)
    {
    }
}