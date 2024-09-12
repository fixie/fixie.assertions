using static Fixie.Assertions.Serialization;
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
        }
        else
        {
            this.message = message;
        }
    }

    public static AssertException ForValues<T>(string? expression, T expected, T actual)
    {
        return new AssertException(expression, Serialize(expected), Serialize(actual));
    }

    public static AssertException ForPredicate<T>(string? expression, string expectation, T actual)
    {
        return new AssertException(expression, expectation, Serialize(actual));
    }

    public static Exception ForException<TException>(string? expression, string expectedMessage, string actualMessage) where TException : Exception
    {
        return new AssertException(expression, expectedMessage, actualMessage,
            $"""
             {expression} should have thrown {typeof(TException).FullName} with message
             
             {Indent(Serialize(expectedMessage))}
             
             but instead the message was
             
             {Indent(Serialize(actualMessage))}
             """);
    }

    public static Exception ForException(string? expression, Type expectedType, string expectedMessage, Type actualType, string actualMessage)
    {
        return new AssertException(expression, expectedMessage, actualMessage,
            $"""
             {expression} should have thrown {expectedType.FullName} with message

             {Indent(Serialize(expectedMessage))}

             but instead it threw {actualType.FullName} with message

             {Indent(Serialize(actualMessage))}
             """);
    }

    public override string Message => message;

    static string MultilineMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be{NewLine}{Indent(expected)}{NewLine}{NewLine}" +
               $"but was{NewLine}{Indent(actual)}";
    }

    static string ScalarMessage(string? expression, string expected, string actual)
    {
        return $"{expression} should be {expected} but was {actual}";
    }
}