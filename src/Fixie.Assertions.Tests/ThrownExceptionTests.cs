namespace Tests;

// WARNING: All changes to this file must respect the intended duplication
// described here. Although it is tempting to reduce duplication here, the
// duplication is necessary to cover several subtle aspects of type inference,
// type casting, and overload resolution.
//
// We need to be sure that end users will not be affected by delegate type
// casting limitations and type inference effects that have affected other
// assertion libraries in the past.
//
// In particular, the natural inferred type for a lambda expression can be a
// `Func<T>` with a known return type, even though its usage in `ShouldThrow`
// is intended as an `Action`. Without additional non-`Action` overloads, the
// compiler would be unable to call `ShouldThrow` on a `Func<T>`. The compiler
// cannot automatically treat a `Func<T>` as an `Action` even though the test
// author could replace `var` with `Action` successfully as a workaround.
//
// To spare the user that experience and to remove the need for the explicit
// `Action` declaration workaround, we overload `ShouldThrow` with a
// `Funct<object?>` variant.
//
// Unfortunately, that overload alone is insufficient. The compiler cannot
// match `Func<T>` to `Func<object>` for value types `T`, even though a
// developer might expect that to be valid. Such users would get a compiler
// error and would again have to replace `var` with `Action` to work around
// that, but the experience is especially confusing without context. We
// overload `ShouldThrow` with a `Delegate` variant which is only selected by
// the compiler during overload resolution for `Func<T>` for value types `T`.
//
// We do not have to similarly overload for the `async` case. Rather than
// involving `Action` at all, this is defined on `Func<Task>`. Fortunately, the
// compiler is able to locate the overload when the target is alternately
// `Func<Task<T>>` even for value types `T`, as the delegate type's type
// parameter is `Task<>`, a reference type regardless of the contained `T`
// type. In other words, the `async` case is simpler because it does not
// involve `Action` conversions at all, and because the type casting support
// for reference type subtypes covers the cases we had to overload for in the
// non-`async` path.
//
// Attempts to reduce duplication here would appear to work, but would silently
// skip over the precise type casting and overload resolution issues described
// above. For meaningful coverage, this file must remain verbose, and changes
// to any test here likely require comparable changes to the other tests here.

class ThrownExceptionTests
{
    public void ShouldAssertExpectedExceptions()
    {
        Action doNothing = () => { };
        Action divideByZero = () => throw new DivideByZeroException("Divided By Zero");

        divideByZero.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
        divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero").ShouldBe<DivideByZeroException>();

        divideByZero.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
        divideByZero.ShouldThrow<Exception>("Divided By Zero").ShouldBe<DivideByZeroException>();

        Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>(),
            """
            noop should have thrown System.DivideByZeroException but did not.
            """);
        Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not.
            """);

        divideByZero.ShouldThrow<DivideByZeroException>();
        Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"
            
            but instead the message was
            
                "Divided By Zero"
            """);

        Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>(),
            """
            divide should have thrown System.ArgumentNullException
            
            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
        Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"
            
            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

    public async Task ShouldAssertExpectedAsyncExceptions()
    {
        Func<Task> doNothing = async () => { await Task.CompletedTask; };
        Func<Task> divideByZero = async () =>
        {
            await Task.CompletedTask;
            throw new DivideByZeroException("Divided By Zero");
        };

        (await divideByZero.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
        (await divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero")).ShouldBe<DivideByZeroException>();

        (await divideByZero.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
        (await divideByZero.ShouldThrow<Exception>("Divided By Zero")).ShouldBe<DivideByZeroException>();

        await Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>(),
            """
            noop should have thrown System.DivideByZeroException but did not.
            """);
        await Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not.
            """);

        await divideByZero.ShouldThrow<DivideByZeroException>();
        await Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"

            but instead the message was
            
                "Divided By Zero"
            """);

        await Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>(),
            """
            divide should have thrown System.ArgumentNullException
            
            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
        await Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"

            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

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
        Action @void = Void;
        Action voidThrows = VoidThrows;

        // Pass when target throws as expected.
        {
            voidThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            voidThrows.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();

            voidThrows.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            voidThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            Contradiction(@void, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(@void, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        // Fail when target throws the wrong exception type.
        {
            Contradiction(voidThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(voidThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(voidThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            Contradiction(voidThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    public async Task ShouldAssertExceptionsForAsyncFunc()
    {
        Func<Task> noResult = NoResultAsync;
        Func<Task> noResultThrows = NoResultThrowsAsync;

        // Pass when target throws as expected.
        {
            (await noResultThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await noResultThrows.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();

            (await noResultThrows.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
            (await noResultThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            await Contradiction(noResult, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            await Contradiction(noResult, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        // Fail when target throws the wrong exception type.
        {
            await Contradiction(noResultThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            await Contradiction(noResultThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            await Contradiction(noResultThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            await Contradiction(noResultThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    public async Task ShouldAssertExceptionsForAsyncFuncWithResult()
    {
        // Pass when target throws as expected.
        {
            var getObjectThrows = async () => await GetObjectThrowsAsync();
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            var getStructThrows = async () => await GetStructThrowsAsync();

            (await getObjectThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableStructThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getStructThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getObjectThrows.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();
            (await getReferenceThrows.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableStructThrows.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();
            (await getStructThrows.ShouldThrow<Exception>(expected)).ShouldBe<DivideByZeroException>();

            (await getObjectThrows.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
            (await getReferenceThrows.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
            (await getNullableStructThrows.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();
            (await getStructThrows.ShouldThrow<DivideByZeroException>()).ShouldBe<DivideByZeroException>();

            (await getObjectThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableStructThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getStructThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            var getObject = async () => await GetObjectAsync();
            var getReference = async () => await GetReferenceAsync();
            var getNullableStruct = async () => await GetNullableStructAsync();
            var getStruct = async () => await GetStructAsync();

            await Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            await Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            await Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);

            await Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);
            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);
            await Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);
            await Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);

            await Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
            await Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
            await Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        // Fail when target throws the wrong exception type.
        {
            var getObjectThrows = async () => await GetObjectThrowsAsync();
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            var getStructThrows = async () => await GetStructThrowsAsync();

            await Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            await Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            await Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);

            await Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            await Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            await Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);

            await Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
            await Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
            await Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            var getObjectThrows = async () => await GetObjectThrowsAsync();
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            var getStructThrows = async () => await GetStructThrowsAsync();

            await Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            await Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            await Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            await Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    public void ShouldAssertExceptionsForForFuncReturningReference()
    {
        var getObject = GetObject;
        var getObjectThrows = GetObjectThrows;

        var getReference = GetReference;
        var getReferenceThrows = GetReferenceThrows;

        // Pass when target throws as expected.
        {
            getObjectThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getObjectThrows.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();

            getObjectThrows.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            getObjectThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getReferenceThrows.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        // Fail when target throws the wrong exception type.
        {
            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    public void ShouldAssertExceptionsForFuncReturningValue()
    {
        var getStruct = GetStruct;
        var getStructThrows = GetStructThrows;

        var getNullableStruct = GetNullableStruct;
        var getNullableStructThrows = GetNullableStructThrows;

        // Pass when target throws as expected.
        {
            getStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();
            
            getStructThrows.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();

            getNullableStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableStructThrows.ShouldThrow<Exception>(expected).ShouldBe<DivideByZeroException>();
            
            getNullableStructThrows.ShouldThrow<DivideByZeroException>().ShouldBe<DivideByZeroException>();
            getNullableStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        // Fail when target does not throw.
        {
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);

            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        // Fail when target throws the wrong exception type.
        {
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);

            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        // Fail when target throws the right exception type with the wrong message.
        {
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }
    }

    public void ShouldFailWhenAbstractDelegateHasInvalidSignature()
    {
        Action<int> actionWithInput = (int x) => { };
        Action<int, string> actionWithInputs = (int x, string y) => { };

        Func<int, int> funcWithInput = (int x) => x;
        Func<int, string, int> funcWithInputs = (int x, string y) => x;
        Func<string> funcReturningReference = () => "A";
        Func<Task> funcAsync = async () => await Task.CompletedTask;

        Contradiction((Delegate)actionWithInput, x => x.ShouldThrow<Exception>(), DelegateMisused<Action<int>>());
        Contradiction((Delegate)actionWithInputs, x => x.ShouldThrow<Exception>(), DelegateMisused<Action<int, string>>());
        Contradiction((Delegate)funcWithInput, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<int, int>>());
        Contradiction((Delegate)funcWithInputs, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<int, string, int>>());
        Contradiction((Delegate)funcReturningReference, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<string>>());
        Contradiction((Delegate)funcAsync, x => x.ShouldThrow<Exception>(), DelegateMisused<Func<Task>>());
    }

    public void ShouldHandleArbitraryDelegate()
    {
        {
            Delegate getObjectThrows = () => GetObjectThrows();
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableStructThrows = () => GetNullableStructThrows();
            Delegate getStructThrows = () => GetStructThrows();

            Contradiction(getObjectThrows, x => x.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>(), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>(), DelegateMisused<Func<string>>());
            getNullableStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            Contradiction(getObjectThrows, x => x.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>(), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>(), DelegateMisused<Func<string>>());
            getNullableStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            
            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>(), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>(), DelegateMisused<Func<string>>());
            getNullableStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            
            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>(), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>(), DelegateMisused<Func<string>>());
            getNullableStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getStructThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        {
            Delegate getObject = () => GetObject();
            Delegate getReference = () => GetReference();
            Delegate getNullableStruct = () => GetNullableStruct();
            Delegate getStruct = () => GetStruct();

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisused<Func<object>>());
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisused<Func<string>>());
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(), DidNotThrow);

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisused<Func<object>>());
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisused<Func<string>>());
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DidNotThrow);

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisused<Func<object>>());
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisused<Func<string>>());
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DidNotThrow);
        }

        {
            Delegate getObjectThrows = () => GetObjectThrows();
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableStructThrows = () => GetNullableStructThrows();
            Delegate getStructThrows = () => GetStructThrows();

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisused<Func<string>>());
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), WrongTypeNoMessage);

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisused<Func<string>>());
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), WrongTypeMisspelledMessage);

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisused<Func<string>>());
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), WrongTypeExpectedMessage);
        }

        {
            Delegate getObjectThrows = () => GetObjectThrows();
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableStructThrows = () => GetNullableStructThrows();
            Delegate getStructThrows = () => GetStructThrows();

            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisused<Func<object>>());
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisused<Func<string>>());
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), WrongMessage);
        }

        {
            Delegate getObjectThrows = async () => await GetObjectThrowsAsync();
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            Delegate getStructThrows = async () => await GetStructThrowsAsync();

            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedStructAsync);

            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedStructAsync);

            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedStructAsync);
        }

        {
            Delegate getObject = async () => await GetObjectAsync();
            Delegate getReference = async () => await GetReferenceAsync();
            Delegate getNullableStruct = async () => await GetNullableStructAsync();
            Delegate getStruct = async () => await GetStructAsync();

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedObjectAsync);
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedNullableAsync);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(), DelegateMisusedStructAsync);

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedObjectAsync);
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedNullableAsync);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedStructAsync);

            Contradiction(getObject, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedObjectAsync);
            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedNullableAsync);
            Contradiction(getStruct, x => x.ShouldThrow<DivideByZeroException>(expected), DelegateMisusedStructAsync);
        }

        {
            Delegate getObjectThrows = async () => await GetObjectThrowsAsync();
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            Delegate getStructThrows = async () => await GetStructThrowsAsync();

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(), DelegateMisusedStructAsync);

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), DelegateMisusedStructAsync);

            Contradiction(getObjectThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), DelegateMisusedStructAsync);
        }

        {
            Delegate getObjectThrows = async () => await GetObjectThrowsAsync();
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableStructThrows = async () => await GetNullableStructThrowsAsync();
            Delegate getStructThrows = async () => await GetStructThrowsAsync();

            Contradiction(getObjectThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedObjectAsync);
            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedReferenceAsync);
            Contradiction(getNullableStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedNullableAsync);
            Contradiction(getStructThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), DelegateMisusedStructAsync);
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

    const string DidNotThrow =
        """
        x should have thrown System.DivideByZeroException but did not.
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

    static string DelegateMisused(Type actual) =>
        $"""
         x should be a function compatible with

             Func<T> where T : struct

         but instead the function has the incompatible type

             {actual}

         Be sure to consider the overloads of ShouldThrow<TException>.
         """;

    static string DelegateMisused<TResult>() =>
        DelegateMisused(typeof(TResult));

    static string DelegateMisusedAsync<TResult>()
    {
        var task = typeof(Task<>).MakeGenericType(typeof(TResult));
        var func = typeof(Func<>).MakeGenericType(task);
        
        return DelegateMisused(func);
    }

    static string DelegateMisusedObjectAsync = DelegateMisusedAsync<object>();
    static string DelegateMisusedReferenceAsync = DelegateMisusedAsync<string>();
    static string DelegateMisusedNullableAsync = DelegateMisusedAsync<Struct?>();
    static string DelegateMisusedStructAsync = DelegateMisusedAsync<Struct>();

    struct Struct;
}