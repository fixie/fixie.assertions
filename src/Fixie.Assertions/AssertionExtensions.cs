using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Fixie.Assertions.Serializer;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

public static class AssertionExtensions
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
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Action shouldThrow, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrow))] string expression = default!) where TException : Exception
    {
        try
        {
            shouldThrow();
        }
        catch (Exception actual)
        {
            return ShouldBeException<TException>(expectedMessage, expression, actual);
        }

        ShouldHaveThrown<TException>(expression, expectedMessage);

        throw new UnreachableException();
    }

    /// <summary>
    /// Assert that this async operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static async Task<TException> ShouldThrow<TException>(this Func<Task> shouldThrowAsync, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrowAsync))] string expression = default!) where TException : Exception
    {
        try
        {
            await shouldThrowAsync();
        }
        catch (Exception actual)
        {
            return ShouldBeException<TException>(expectedMessage, expression, actual);
        }

        ShouldHaveThrown<TException>(expression, expectedMessage);
        
        throw new UnreachableException();
    }

    /// <summary>
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Func<object?> shouldThrow, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrow))] string expression = default!) where TException : Exception
    {
        Action actionShouldThrow = () => shouldThrow();

        return actionShouldThrow.ShouldThrow<TException>(expectedMessage, expression);
    }

    /// <summary>
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// 
    /// <para>This overload always fails, to provide guidance around surprising compiler error messages.</para>
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Delegate shouldThrow, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrow))] string expression = default!) where TException : Exception
    {
        // Consider attempting to call ShouldThrow on any of these:
        //
        //     var throwOrBool = () => true;
        //     var throwOrNullableInt = () => (int?)1;
        //     var throwOrCustomStruct = () => new CustomStruct();
        //
        // It is desirable that these function types would work with the
        // Func<object?> overload of ShouldThrow, but the compiler cannot
        // resolve to that overload when the return type is a value type.
        // It is better to provide clear guidance than expose the user to
        // a confusing compiler error message. Simply changing `var` to
        // `Action` would be enough to resolve the issue, but that is not
        // immediately clear from the compiler error you would face.
        //
        // Although it is tempting to honor these via DynamicInvoke of the
        // Delegate, doing so exposes us to severe risk around Func<T> for
        // unanticipated types T, such as when T is a Task, ValueTask or
        // similar. The underlying code might never be exected, leaving the
        // user confused about why their test is failing or why their
        // breakpoint isn't reachable.
        //
        // Although it is tempting to special-case detection and rejection
        // of Task and ValueTask, consider some unanticipated custom
        // awaitable type. It is difficult to detect such a type as it
        // relies more on a compiler-recognized pattern than on something
        // simpler like a well-known base type.
        //
        // The only safe option to mitigate this situation would be to
        // reduce the real world exposure to this overload by adding specific
        // overloads for the most common value types and again for their
        // nullable counterpart: Func<int>, Func<int?>, etc. Test coverage
        // for such types would grow large, so we can only consider doing so
        // for the most common value types.
        //
        // Note that the single async overload for Func<Task> is sufficient
        // for all Func<Task<T>>, even value types T. This is because the
        // compiler can handle the equivalence of Func<Task<T>> to Func<Task>
        // as Task<T> inherits Task and because all Task<T> are reference
        // types regardless of the result T.

        var actualSignature = Signature(shouldThrow.Method);

        throw new AssertException(expression, "Action or Func<Task>", actualSignature,
            $"""
             {expression} has a function type compatible with

                 {actualSignature}

             but ShouldThrow<TException> has no corresponding overload.

             You're getting this message because you're hitting a catch-all
             overload of ShouldThrow<TException> that exists only to provide
             the following guidance. Without this overload, you would have
             faced a confusing compiler error message.

             The most likely problem is that you intended to call the
             Func<object?> overload for a function compatible with
             Func<T> where T : struct, but the compiler cannot safely treat
             that as Func<object?> even though all such types T are objects.

             You may need to cast the target to either Action or Func<Task>,
             or wrap it in an equivalent lambda expression.
             """, false);
    }

    static string Signature(MethodInfo function)
    {
        var typeParameters =
            function
                .GetParameters()
                .Select(x => x.ParameterType)
                .ToList();

        if (function.ReturnType == typeof(void))
            return typeParameters.Count == 0
                ? "System.Action"
                : $"System.Action`{typeParameters.Count}[{string.Join(",", typeParameters)}]";

        typeParameters.Add(function.ReturnType);
        return $"System.Func`{typeParameters.Count}[{string.Join(",", typeParameters)}]";
    }

    static TException ShouldBeException<TException>(string? expectedMessage, string expression, Exception actual) where TException : Exception
    {
        if (actual is TException typed)
        {
            if (expectedMessage != null && actual.Message != expectedMessage)
                throw new AssertException(expression, expectedMessage, actual.Message,
                        $"""
                         {expression} should have thrown {typeof(TException).FullName} with message
             
                         {Indent(Serialize(expectedMessage))}
             
                         but instead the message was
             
                         {Indent(Serialize(actual.Message))}
                         """, false);

            return typed;
        }

        var expectedType = typeof(TException).FullName!;
        var actualType = actual.GetType().FullName!;

        if (expectedMessage == null)
        {
            throw new AssertException(expression, expectedType, actualType,
                $"""
                 {expression} should have thrown {expectedType}

                 but instead it threw {actualType} with message

                 {Indent(Serialize(actual.Message))}
                 """, false);
        }

        throw new AssertException(expression, expectedMessage, actual.Message,
            $"""
             {expression} should have thrown {expectedType} with message

             {Indent(Serialize(expectedMessage))}

             but instead it threw {actualType} with message

             {Indent(Serialize(actual.Message))}
             """, false);
    }

    static void ShouldHaveThrown<TException>(string expression, string? expectedMessage) where TException : Exception
    {
        var expectedType = typeof(TException).FullName!;

        if (expectedMessage == null)
        {
            throw new AssertException(expression, expectedType, "no exception was thrown",
                $"""
                 {expression} should have thrown {expectedType}

                 but no exception was thrown.
                 """, false);
        }

        throw new AssertException(expression, expectedType, "no exception was thrown",
            $"""
             {expression} should have thrown {expectedType} with message

             {Indent(Serialize(expectedMessage))}
             
             but no exception was thrown.
             """, false);
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