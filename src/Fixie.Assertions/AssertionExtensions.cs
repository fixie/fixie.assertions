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

    public static void ShouldBe<T>(this IEquatable<T> actual, IEquatable<T> expected, [CallerArgumentExpression(nameof(actual))] string? expression = null)
    {
        if (!actual.Equals(expected))
            throw AssertException.ForValues(expression, expected, actual);
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
}