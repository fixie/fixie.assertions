using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Fixie.Assertions.Serialization;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

public static class AssertionExtensions
{
    /// <summary>
    /// Assert that this object is equal to some expected object.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldBe<T>(this T? actual, T? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
        where T : class
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
            throw EqualityFailure(expression, expected, actual);
    }

    /// <summary>
    /// Assert that this value is equal to some expected value.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
        where T : struct
    {
        if (!actual.Equals(expected))
            throw EqualityFailure(expression, expected, actual);
    }

    /// <summary>
    /// Assert that this object is an instance of the specified type.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static T ShouldBe<T>(this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual is T typed)
            return typed;

        throw EqualityFailure(expression, typeof(T), actual?.GetType());
    }

    /// <summary>
    /// Assert that this object is not null.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldNotBeNull([NotNull] this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual == null)
            throw new AssertException(expression, "not null", "null", $"{expression} should not be null but was null");
    }

    /// <summary>
    /// Assert that this enumerable is structurally equal to some expected array.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldMatch<T>(this IEnumerable<T> actual, T[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualArray = actual.ToArray();

        if (actualArray.Length != expected.Length)
            throw EqualityFailure(expression, expected, actualArray);

        foreach (var (actualItem, expectedItem) in actualArray.Zip(expected))
            if (!Equals(actualItem, expectedItem))
                throw EqualityFailure(expression, expected, actualArray);
    }

    /// <summary>
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Action shouldThrow, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrow))] string? expression = null) where TException : Exception
    {
        try
        {
            shouldThrow();
        }
        catch (Exception actual)
        {
            return ShouldBeException<TException>(expectedMessage, expression, actual);
        }

        ShouldHaveThrown<TException>(expression);

        throw new UnreachableException();
    }

    /// <summary>
    /// Assert that this async operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static async Task<TException> ShouldThrow<TException>(this Func<Task> shouldThrowAsync, string expectedMessage, [CallerArgumentExpression(nameof(shouldThrowAsync))] string? expression = null) where TException : Exception
    {
        try
        {
            await shouldThrowAsync();
        }
        catch (Exception actual)
        {
            return ShouldBeException<TException>(expectedMessage, expression, actual);
        }

        ShouldHaveThrown<TException>(expression);
        
        throw new UnreachableException();
    }

    static TException ShouldBeException<TException>(string expectedMessage, string? expression, Exception actual) where TException : Exception
    {
        if (actual is TException typed)
        {
            if (actual.Message != expectedMessage)
                throw new AssertException(expression, expectedMessage, actual.Message,
                        $"""
                         {expression} should have thrown {typeof(TException).FullName} with message
             
                         {Indent(Serialize(expectedMessage))}
             
                         but instead the message was
             
                         {Indent(Serialize(actual.Message))}
                         """);

            return typed;
        }

        var expectedType = typeof(TException);
        var actualType = actual.GetType();

        throw new AssertException(expression, expectedMessage, actual.Message,
            $"""
             {expression} should have thrown {expectedType.FullName} with message

             {Indent(Serialize(expectedMessage))}

             but instead it threw {actualType.FullName} with message

             {Indent(Serialize(actual.Message))}
             """);
    }

    static void ShouldHaveThrown<TException>(string? expression) where TException : Exception
    {
        var expectedType = typeof(TException).FullName!;

        throw new AssertException(expression, expectedType, "no exception was thrown",
            $"{expression} should have thrown {expectedType} but did not");
    }

    /// <summary>
    /// Assert that this object satisfies some expectation.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void Should<T>(this T actual, Func<T, bool> expectation, [CallerArgumentExpression(nameof(actual))] string? expression = default!, [CallerArgumentExpression(nameof(expectation))] string? expectationBody = default!)
    {
        if (!expectation(actual))
        {
            expectationBody.ShouldNotBeNull();
            expectationBody = DropTrivialLambdaPrefix(expectationBody);

            throw ExpectationFailure(expression, expectationBody, actual);
        }
    }

    static string DropTrivialLambdaPrefix(string expectationBody)
    {
        var identifierMatch = Regex.Matches(expectationBody, @"^(\w+)");

        if (identifierMatch.Count == 1)
        {
            var identifier = identifierMatch[0].Groups[1].Value;

            expectationBody = Regex.Replace(expectationBody,
                "^" + Regex.Escape(identifier) + @"\s*=>\s*" + Regex.Escape(identifier) + @"\s+", "");
        }

        return expectationBody;
    }

    static AssertException EqualityFailure<T>(string? expression, T expected, T actual)
        => new(expression, Serialize(expected), Serialize(actual));

    static AssertException ExpectationFailure<T>(string? expression, string expectation, T actual)
        => new(expression, expectation, Serialize(actual));
}