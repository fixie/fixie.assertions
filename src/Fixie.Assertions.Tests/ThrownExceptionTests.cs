namespace Tests;

class ThrownExceptionTests
{
    public void DetectPotentialForSimplification()
    {
        // If the compiler ever gains support for casting Func<T> to Func<object?>
        // for value types T, this canary test will start to fail. We may be able
        // to eliminate the `Delegate`-accepting overload in that case.

        Func<string> getString = () => "A";
        Func<char> getChar = () => 'A';

        Func<object?> castedForReferenceType = getString;

        Action performInvalidCast = () =>
        {
            Func<object?> castedForValueType = (Func<object?>)(object)getChar;
        };

        performInvalidCast.ShouldThrow<InvalidCastException>(
            "Unable to cast object of type 'System.Func`1[System.Char]' " +
            "to type 'System.Func`1[System.Object]'.");
    }

    public void ShouldAssertExceptionsForAction()
    {
        ActionOverload(Void, VoidThrows);
    }

    public async Task ShouldAssertExceptionsForAsyncFunc()
    {
        await AsyncFuncOverload(NoResultAsync, NoResultThrowsAsync);
        await AsyncFuncOverload(GetObjectAsync, GetObjectThrowsAsync);
        await AsyncFuncOverload(GetReferenceAsync, GetReferenceThrowsAsync);
        await AsyncFuncOverload(GetNullableStructAsync, GetNullableStructThrowsAsync);
        await AsyncFuncOverload(GetStructAsync, GetStructThrowsAsync);
    }

    public void ShouldAssertExceptionsForFuncReturningReference()
    {
        FuncReturningReferenceOverload(GetObject, GetObjectThrows);
        FuncReturningReferenceOverload(GetReference, GetReferenceThrows);
    }

    public void ShouldAssertExceptionsForFuncReturningValueWhenWrappedAsAction()
    {
        // For arbitrary value types, the delegate type must
        // be presented as an Action for ShouldThrow to work.
        // See test coverage for the Delegate overload for
        // when such lambdas are not presented as Action.
        // This test demonstrates that the intended workaround
        // of presenting the functions as Action works.

        Action getStruct = () => GetStruct();
        Action getStructThrows = () => GetStructThrows();

        Action getNullableStruct = () => GetNullableStruct();
        Action getNullableStructThrows = () => GetNullableStructThrows();

        ActionOverload(getStruct, getStructThrows);
        ActionOverload(getNullableStruct, getNullableStructThrows);
    }

    public void ShouldFailWithClearGuidanceForUnsupportedDelegateTypes()
    {
        // The motivating case for the opaque Delegate overload is that
        // the natural types of these lambda expressions are surprisingly
        // not compatible with the Func<object?> overload, and the resulting
        // compiler error is too confusing. It is better to provide real
        // guidance in these cases with an overload that always fails with
        // a message explaining what to do.

        var funcReturingBool = () => true;
        var funcReturingNullableInt = () => (int?)1;
        var funcReturningStruct = () => new Struct();

        Contradiction(funcReturingBool, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<bool>>());
        Contradiction(funcReturingNullableInt, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<int?>>());
        Contradiction(funcReturningStruct, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<Struct>>());


        // Valid overloads for these function types exist, so if we encounter them as
        // some opaque Delegate we need to provide clear guidance directing them to
        // the proper overloads.

        Action action = () => { };
        Func<string> funcReturningReference = () => "A";
        Func<Task> funcTaskAsync = async () => await Task.CompletedTask;
        Func<Task<int>> funcTaskResultAsync = async () => await Task.FromResult(1);

        Contradiction((Delegate)action, x => x.ShouldThrow<Exception>(), DelegateMisused<Action>());
        Contradiction((Delegate)funcReturningReference, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<string>>());
        Contradiction((Delegate)funcTaskAsync, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<Task>>());
        Contradiction((Delegate)funcTaskResultAsync, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<Task<int>>>());


        // We'd prefer to just let the user encounter a simple compiler error
        // when attempting to call ShouldThrow on delegates like these, but
        // the more vital scenarios above open the door to the possibility
        // that someone will make an attempt, and so we need to provide clear
        // guidance.

        Action<int> actionWithInput = (int x) => { };
        Action<int, string> actionWithInputs = (int x, string y) => { };
        Func<int, int> funcWithInput = (int x) => x;
        Func<int, string, int> funcWithInputs = (int x, string y) => x;
        Func<ValueTask> funcValueTaskAsync = async () => await Task.CompletedTask;

        Contradiction(actionWithInput, x => x.ShouldThrow<Exception>(), DelegateMisused<Action<int>>());
        Contradiction(actionWithInputs, x => x.ShouldThrow<Exception>(), DelegateMisused<Action<int, string>>());
        Contradiction(funcWithInput, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<int, int>>());
        Contradiction(funcWithInputs, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<int, string, int>>());
        Contradiction(funcValueTaskAsync, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<ValueTask>>());
    }

    static void ActionOverload(Action returns, Action throws)
    {
        // Pass when target throws as expected.
        {
            throws.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            throws.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();

            throws.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            throws.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrowWhileNotExpectingMessage);
            Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrowWhileExpectingMessage);
        }

        // Fail when target throws the wrong exception type.
        {
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            Contradiction(throws, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    static async Task AsyncFuncOverload(Func<Task> returns, Func<Task> throws)
    {
        // Pass when target throws as expected.
        {
            (await throws.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await throws.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();
            
            (await throws.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
            (await throws.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            await Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrowWhileNotExpectingMessage);
            await Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrowWhileExpectingMessage);
        }

        // Fail when target throws the wrong exception type.
        {
            await Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            await Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            await Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            await Contradiction(throws, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    static void FuncReturningReferenceOverload(Func<object?> returns, Func<object?> throws)
    {
        // Pass when target throws as expected.
        {
            throws.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            throws.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();

            throws.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            throws.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrowWhileNotExpectingMessage);
            Contradiction(returns, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrowWhileExpectingMessage);
        }

        // Fail when target throws the wrong exception type.
        {
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(throws, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            Contradiction(throws, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    static void Void() { }
    static object GetObject() => new();
    static string GetReference() => "A";
    static Struct? GetNullableStruct() => new();
    static Struct GetStruct() => new();

    static void VoidThrows() => throw new DivideByZeroException(expected);
    static object GetObjectThrows() => throw new DivideByZeroException(expected);
    static string GetReferenceThrows() => throw new DivideByZeroException(expected);
    static Struct? GetNullableStructThrows() => throw new DivideByZeroException(expected);
    static Struct GetStructThrows() => throw new DivideByZeroException(expected);

    static async Task NoResultAsync() => await Task.CompletedTask;
    static async Task<object> GetObjectAsync() => await Task.FromResult<object>(new());
    static async Task<string> GetReferenceAsync() => await Task.FromResult<string>("A");
    static async Task<Struct?> GetNullableStructAsync() => await Task.FromResult<Struct?>(new());
    static async Task<Struct> GetStructAsync() => await Task.FromResult<Struct>(new());

    static async Task NoResultThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<object> GetObjectThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<string> GetReferenceThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<Struct?> GetNullableStructThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<Struct> GetStructThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    const string misspelled = "Simulatd Failre (Misspelled)";
    const string expected = "Simulated Failure";

    const string DidNotThrowWhileNotExpectingMessage =
        """
        x should have thrown System.DivideByZeroException
        
        but no exception was thrown.
        """;

    const string DidNotThrowWhileExpectingMessage =
        """
        x should have thrown System.DivideByZeroException with message
        
            "Simulated Failure"

        but no exception was thrown.
        """;

    const string WrongTypeNoMessage =
        """
        x should have thrown System.OutOfMemoryException

        but instead it threw System.DivideByZeroException with message

            "Simulated Failure"
        """;

    const string WrongTypeMisspelledMessage =
        """
        x should have thrown System.OutOfMemoryException with message

            "Simulatd Failre (Misspelled)"

        but instead it threw System.DivideByZeroException with message

            "Simulated Failure"
        """;

    const string WrongTypeExpectedMessage =
        """
        x should have thrown System.OutOfMemoryException with message

            "Simulated Failure"

        but instead it threw System.DivideByZeroException with message

            "Simulated Failure"
        """;

    const string WrongMessage =
        """
        x should have thrown System.DivideByZeroException with message

            "Simulatd Failre (Misspelled)"

        but instead the message was

            "Simulated Failure"
        """;

    static string DelegateMisused<TResult>() =>
        $"""
         x has a function type compatible with

             {typeof(TResult)}

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
         """;

    struct Struct;
}