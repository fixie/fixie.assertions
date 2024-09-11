using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Fixie.Assertions;

public static class AssertionExtensions
{
    public static void ShouldBe(this bool actual, bool expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this char actual, char expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this string? actual, string? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!actual.Equals(expected))
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldBe(this Type actual, Type expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual != expected)
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static T ShouldBe<T>(this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual is T typed)
            return typed;

        throw AssertException.ForValues(expression, typeof(T), actual?.GetType());
    }

    public static void ShouldBe(this object? actual, object? expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!Equals(actual, expected))
            throw AssertException.ForValues(expression, expected, actual);
    }

    public static void ShouldNotBeNull([NotNull] this object? actual, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (actual == null)
            throw AssertException.ForMessage(expression, "not null", "null", $"{expression} should not be null but was null");
    }

    public static void ShouldBe<T>(this IEnumerable<T> actual, T[] expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        var actualArray = actual.ToArray();

        if (actualArray.Length != expected.Length)
            throw AssertException.ForLists(expression, expected, actualArray);

        foreach (var (actualItem, expectedItem) in actualArray.Zip(expected))
            if (!Equals(actualItem, expectedItem))
                throw AssertException.ForLists(expression, expected, actualArray);
    }
}