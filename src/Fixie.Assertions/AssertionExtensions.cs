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
}