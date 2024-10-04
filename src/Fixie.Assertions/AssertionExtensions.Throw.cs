using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using static Fixie.Assertions.Serializer;
using static Fixie.Assertions.StringUtilities;

namespace Fixie.Assertions;

public static partial class AssertionExtensions
{
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

        ShouldHaveThrown<TException>(expression);

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

        ShouldHaveThrown<TException>(expression);
        
        throw new UnreachableException();
    }

    /// <summary>
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Func<object?> shouldThrow, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrow))] string expression = default!) where TException : Exception
    {
        // To be absolutely clear that we are always working with an
        // Action here, never apply `var` type inference on this variable,
        // and never inline this variable.

        Action actionShouldThrow = () => shouldThrow();

        return actionShouldThrow.ShouldThrow<TException>(expectedMessage, expression);
    }

    /// <summary>
    /// Assert that this operation throws an exception of the specified type with some expected message.
    /// 
    /// <para>This overload works around limitations of the type system, and is intended for delegate types equivalent to <c>Func&lt;T&gt;</c> satisfying the constraint <c>where T : struct</c>.</para>
    /// 
    /// <para>If any other delegate type is provided, the assertion will fail with an explanation.</para>
    /// </summary>
    /// <param name="expectedMessage">When provided, assert that the exception message matches the given string.</param>
    /// <param name="expression">Leave this parameter at its default to enable automatically descriptive failure messages.</param>
    public static TException ShouldThrow<TException>(this Delegate shouldThrow, string? expectedMessage = null, [CallerArgumentExpression(nameof(shouldThrow))] string expression = default!) where TException : Exception
    {
        var function = shouldThrow.Method;
        var returnType = function.ReturnType;
        var returnsValueType = returnType != typeof(void) && returnType.IsValueType;

        if (!returnsValueType || function.GetParameters().Length > 0)
        {
            var expectedSignature = "Func<T> where T : struct";
            var actualSignature = Signature(function);

            throw new AssertException(expression, expectedSignature, actualSignature,
                $"""
                 {expression} should be a function compatible with

                     {expectedSignature}

                 but instead the function has the incompatible type

                     {actualSignature}

                 Be sure to consider the overloads of ShouldThrow<TException>.
                 """, false);
        }

        // To be absolutely clear that we are always working with an
        // Action here, never apply `var` type inference on this variable,
        // and never inline this variable.

        Action actionShouldThrow = () =>
        {
            try
            {
                shouldThrow.DynamicInvoke();
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; // Unreachable.
            }
        };

        return actionShouldThrow.ShouldThrow<TException>(expectedMessage, expression);
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

    static void ShouldHaveThrown<TException>(string expression) where TException : Exception
    {
        var expectedType = typeof(TException).FullName!;

        throw new AssertException(expression, expectedType, "no exception was thrown",
            $"{expression} should have thrown {expectedType} but did not.", false);
    }
}