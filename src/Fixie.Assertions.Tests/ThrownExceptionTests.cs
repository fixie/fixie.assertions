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

    public async Task ShouldPassWhenThrowsAsExpected()
    {
        {
            Action getReferenceThrows = () => GetReferenceThrows();
            Action getNullableThrows = () => GetNullableThrows();
            Action getValueThrows = () => GetValueThrows();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        {
            Func<object> getReferenceThrows = () => GetReferenceThrows();
            Func<int?> getNullableThrows = () => GetNullableThrows();
            Func<int> getValueThrows = () => GetValueThrows();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        {
            var getReferenceThrows = () => GetReferenceThrows();
            var getNullableThrows = () => GetNullableThrows();
            var getValueThrows = () => GetValueThrows();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        {
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableThrows = () => GetNullableThrows();
            Delegate getValueThrows = () => GetValueThrows();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();

            getReferenceThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<Exception>().ShouldBe<DivideByZeroException>();
            
            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            
            getReferenceThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getNullableThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
            getValueThrows.ShouldThrow<DivideByZeroException>(expected).ShouldBe<DivideByZeroException>();
        }

        {
            Func<Task> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task> getValueThrows = async () => await GetValueThrowsAsync();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        {
            Func<Task<object>> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task<int?>> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task<int>> getValueThrows = async () => await GetValueThrowsAsync();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        {
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableThrows = async () => await GetNullableThrowsAsync();
            var getValueThrows = async () => await GetValueThrowsAsync();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<Exception>()).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();

            (await getReferenceThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getNullableThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
            (await getValueThrows.ShouldThrow<DivideByZeroException>(expected)).ShouldBe<DivideByZeroException>();
        }

        {
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableThrows = async () => await GetNullableThrowsAsync();
            Delegate getValueThrows = async () => await GetValueThrowsAsync();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }
    }

    public async Task ShouldFailWhenDoesNotThrow()
    {
        {
            Action getReference = () => GetReference();
            Action getNullable = () => GetNullable();
            Action getValue = () => GetValue();

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            Func<object> getReference = () => GetReference();
            Func<int?> getNullable = () => GetNullable();
            Func<int> getValue = () => GetValue();

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            var getReference = () => GetReference();
            var getNullable = () => GetNullable();
            var getValue = () => GetValue();

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            Delegate getReference = () => GetReference();
            Delegate getNullable = () => GetNullable();
            Delegate getValue = () => GetValue();

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            Func<Task> getReference = async () => await GetReferenceAsync();
            Func<Task> getNullable = async () => await GetNullableAsync();
            Func<Task> getValue = async () => await GetValueAsync();

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            Func<Task<object>> getReference = async () => await GetReferenceAsync();
            Func<Task<int?>> getNullable = async () => await GetNullableAsync();
            Func<Task<int>> getValue = async () => await GetValueAsync();

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            var getReference = async () => await GetReferenceAsync();
            var getNullable = async () => await GetNullableAsync();
            var getValue = async () => await GetValueAsync();

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            await Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            await Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }

        {
            Delegate getReference = async () => await GetReferenceAsync();
            Delegate getNullable = async () => await GetNullableAsync();
            Delegate getValue = async () => await GetValueAsync();

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");

            Contradiction(getReference, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getNullable, x => x.ShouldThrow<DivideByZeroException>(expected), "");
            Contradiction(getValue, x => x.ShouldThrow<DivideByZeroException>(expected), "");
        }
    }

    public async Task ShouldFailWhenThrowsWrongExceptionType()
    {
        {
            Action getReferenceThrows = () => GetReferenceThrows();
            Action getNullableThrows = () => GetNullableThrows();
            Action getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            Func<object> getReferenceThrows = () => GetReferenceThrows();
            Func<int?> getNullableThrows = () => GetNullableThrows();
            Func<int> getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            var getReferenceThrows = () => GetReferenceThrows();
            var getNullableThrows = () => GetNullableThrows();
            var getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableThrows = () => GetNullableThrows();
            Delegate getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            Func<Task> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task> getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            Func<Task<object>> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task<int?>> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task<int>> getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableThrows = async () => await GetNullableThrowsAsync();
            var getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }

        {
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableThrows = async () => await GetNullableThrowsAsync();
            Delegate getValueThrows = async () => await GetValueThrowsAsync();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(misspelled), "");

            Contradiction(getReferenceThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<OutOfMemoryException>(expected), "");
        }
    }

    public async Task ShouldFailWhenThrowsWrongExceptionMessage()
    {
        {
            Action getReferenceThrows = () => GetReferenceThrows();
            Action getNullableThrows = () => GetNullableThrows();
            Action getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            Func<object> getReferenceThrows = () => GetReferenceThrows();
            Func<int?> getNullableThrows = () => GetNullableThrows();
            Func<int> getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            var getReferenceThrows = () => GetReferenceThrows();
            var getNullableThrows = () => GetNullableThrows();
            var getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            Delegate getReferenceThrows = () => GetReferenceThrows();
            Delegate getNullableThrows = () => GetNullableThrows();
            Delegate getValueThrows = () => GetValueThrows();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            Func<Task> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task> getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            Func<Task<object>> getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Func<Task<int?>> getNullableThrows = async () => await GetNullableThrowsAsync();
            Func<Task<int>> getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            var getReferenceThrows = async () => await GetReferenceThrowsAsync();
            var getNullableThrows = async () => await GetNullableThrowsAsync();
            var getValueThrows = async () => await GetValueThrowsAsync();

            await Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            await Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }

        {
            Delegate getReferenceThrows = async () => await GetReferenceThrowsAsync();
            Delegate getNullableThrows = async () => await GetNullableThrowsAsync();
            Delegate getValueThrows = async () => await GetValueThrowsAsync();

            Contradiction(getReferenceThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getNullableThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
            Contradiction(getValueThrows, x => x.ShouldThrow<DivideByZeroException>(misspelled), "");
        }
    }

    static string GetReference() => "A";
    static int? GetNullable() => 1;
    static int GetValue() => 1;

    static string GetReferenceThrows() => throw new DivideByZeroException(expected);
    static int? GetNullableThrows() => throw new DivideByZeroException(expected);
    static int GetValueThrows() => throw new DivideByZeroException(expected);

    static async Task<string> GetReferenceAsync() => await Task.FromResult<string>("A");
    static async Task<int?> GetNullableAsync() => await Task.FromResult<int?>(1);
    static async Task<int> GetValueAsync() => await Task.FromResult<int>(1);

    static async Task<string> GetReferenceThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<int?> GetNullableThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    static async Task<int> GetValueThrowsAsync()
    {
        await Task.CompletedTask;
        throw new DivideByZeroException(expected);
    }

    const string misspelled = "Simulatd Failre (Misspelled)";
    const string expected = "Simulated Failure";
}