namespace Fixie.Assertions;

public class AssertException : Exception
{
    public string? Expression { get; }
    public string Expected { get; }
    public string Actual { get; }
    readonly string message;

    public AssertException(string? expression, string expected, string actual, string? message = null)
    {
        Expression = expression;
        Expected = expected;
        Actual = actual;

        if (message == null)
        {
            this.message = ScalarMessage(Expression, Expected, Actual);
        }
        else
        {
            this.message = message;
        }
    }

    public static AssertException ForValues<T>(string? expression, T expected, T actual)
    {
        return new AssertException(expression, SerializeByType(expected), SerializeByType(actual));
    }

    public static AssertException ForMessage(string? expression, string expected, string actual, string message)
    {
        return new AssertException(expression, expected, actual, message);
    }

    public override string Message => message;

    static string ScalarMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be {expected} but was {actual}";
    }

    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(object x) => x.ToString()!;

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

    static string SerializeByType<T>(T any)
    {
        if (any == null) return "null";

        if (typeof(T) == typeof(bool))
            return Serialize((bool)(object)any);

        if (typeof(T) == typeof(Type))
            return Serialize((Type)(object)any);

        return Serialize(any);
    }
}