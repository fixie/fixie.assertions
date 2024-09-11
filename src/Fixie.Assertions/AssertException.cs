namespace Fixie.Assertions;

public class AssertException : Exception
{
    public string? Expression { get; }
    public string Expected { get; }
    public string Actual { get; }
    readonly string message;

    public AssertException(string? expression, string expected, string actual)
    {
        Expression = expression;
        Expected = expected;
        Actual = actual;

        message = ScalarMessage(Expression, Expected, Actual);
    }

    public static AssertException ForValues<T>(string? expression, T expected, T actual)
    {
        return new AssertException(expression, SerializeByType(expected), SerializeByType(actual));
    }

    public override string Message => message;

    static string ScalarMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be {expected} but was {actual}";
    }

    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(object x) => x.ToString()!;

    static string SerializeByType<T>(T any)
    {
        if (any == null)
            throw new NotSupportedException("Cannot yet serialize nulls.");

        if (typeof(T) == typeof(bool))
            return Serialize((bool)(object)any);

        return Serialize(any);
    }
}