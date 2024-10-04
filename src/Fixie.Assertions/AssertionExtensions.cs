using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static Fixie.Assertions.Serializer;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

public static partial class AssertionExtensions
{
    /// <summary>
    /// Assert that this object is equal to some expected object.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldBe<T>(this T? actual, T? expected, [CallerArgumentExpression(nameof(actual))] string expression = default!)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
            throw EqualityFailure(expression, expected, actual);
    }

    /// <summary>
    /// Assert that this object matches the type pattern: <c>(actual is T)</c>
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static T ShouldBe<T>(this object? actual, [CallerArgumentExpression(nameof(actual))] string expression = default!)
    {
        if (actual is T typed)
            return typed;

        throw TypeFailure(expression, typeof(T), actual?.GetType());
    }

    /// <summary>
    /// Assert that this object is not null.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldNotBeNull([NotNull] this object? actual, [CallerArgumentExpression(nameof(actual))] string expression = default!)
    {
        if (actual == null)
            throw new AssertException(expression, "not null", "null", $"{expression} should not be null but was null.", false);
    }

    /// <summary>
    /// Assert that this object is structurally equivalent to some expected object.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldMatch<T>(this T actual, T expected, [CallerArgumentExpression(nameof(actual))] string expression = default!)
    {
        var actualStructure = Serialize(actual);
        var expectedStructure = Serialize(expected);

        if (actualStructure != expectedStructure)
            throw StructuralEqualityFailure(expression, expectedStructure, actualStructure);
    }

    /// <summary>
    /// Assert that this object satisfies some expectation.
    /// </summary>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static void ShouldSatisfy<T>(this T actual, Func<T, bool> expectation, [CallerArgumentExpression(nameof(actual))] string expression = default!, [CallerArgumentExpression(nameof(expectation))] string expectationBody = default!)
    {
        if (!expectation(actual))
        {
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

    static AssertException EqualityFailure<T>(string expression, T expected, T actual)
        => Failure(expression, Serialize(expected), Serialize(actual), "be");

    static AssertException TypeFailure(string expression, Type expected, Type? actual)
        => Failure(expression, $"is {TypeName(expected)}", actual == null ? "null" : TypeName(actual), "match the type pattern");

    static AssertException StructuralEqualityFailure(string expression, string expectedStructure, string actualStructure)
        => Failure(expression, expectedStructure, actualStructure, "match");

    static AssertException ExpectationFailure<T>(string expression, string expectationBody, T actual)
        => Failure(expression, expectationBody, Serialize(actual), "satisfy");

    static AssertException Failure(string expression, string expected, string actual, string shouldVerb)
    {
        bool isMultiline = !IsTrivial(expected) || !IsTrivial(actual);

        var content = new StringBuilder();

        content.Append(expression);
        content.Append(" should ");
        content.Append(shouldVerb);

        if (isMultiline)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(Indent(expected));
            content.AppendLine();
            content.AppendLine();
        }
        else
        {
            content.Append(' ');
            content.Append(expected);
            content.Append(' ');
        }

        content.Append("but was");

        if (isMultiline)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(Indent(actual));
        }
        else
        {
            content.Append(' ');
            content.Append(actual);
        }

        if (expected == actual)
        {
            content.AppendLine();
            content.AppendLine();
            content.Append(
                "These serialized values are identical. Did you mean to perform " +
                "a structural comparison with `ShouldMatch` instead?");
        }

        var message = content.ToString();

        return new(expression, expected, actual, message, isMultiline);
    }

    static bool IsTrivial(string value)
    {
        return
            value == "null" || value == "true" || value == "false" ||
            value.StartsWith('\'') ||
            (value.Length > 0 && char.IsDigit(value[0]));
    }
}