namespace Tests;

class ThrownExceptionTests
{
    public void ShouldAssertExpectedExceptions()
    {
        var doNothing = () => { };
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
        var doNothing = async () => { await Task.CompletedTask; };
        var divideByZero = async () =>
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
}