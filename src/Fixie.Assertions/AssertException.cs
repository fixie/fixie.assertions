using static System.Environment;

namespace Fixie.Assertions;

public class AssertException(string message) : Exception(message)
{
    public override string? StackTrace => FilterStackTrace(base.StackTrace);

    static readonly string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

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

        return string.Join(NewLine, results);
    }
}

public class ComparisonException(string expected, string actual, string message) : AssertException(message)
{
    public string Expected => expected;
    public string Actual => actual;
}