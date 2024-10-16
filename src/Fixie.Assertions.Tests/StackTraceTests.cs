using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tests;

class StackTraceTests
{
    public void ShouldLeaveNonAssertStackTracesAlone()
    {
        var exception = Catch(ThrowException);
        
        exception.Message.ShouldBe("Exception with full stack trace.");

        exception.StackTrace.ShouldNotBeNull();
        
        NormalizeLineNumbers(exception.StackTrace)
            .ShouldBe(
                $"""
                {At<StackTraceTests>("ThrowException()")}
                {At<StackTraceTests>("Catch(Action throwingAction)")}
                """);
    }

    public void ShouldFilterImplementationDetailsFromAssertExceptions()
    {
        var exception = Catch(FailAssertion);
        
        exception.Message
            .ShouldBe(
                """
                1 should be
                
                    2
                
                but was
                
                    1
                """);

        exception.StackTrace.ShouldNotBeNull();
        
        NormalizeLineNumbers(exception.StackTrace)
            .ShouldBe(
                $"""
                {At<StackTraceTests>("FailAssertion()")}
                {At<StackTraceTests>("Catch(Action throwingAction)")}
                """);
    }

    static Exception Catch(Action throwingAction)
    {
        Exception? actual = null;

        try
        {
            throwingAction();
        }
        catch (Exception exception)
        {
            actual = exception;
        }

        actual.ShouldNotBeNull();

        return actual;
    }

    static void ThrowException()
        => throw new Exception("Exception with full stack trace.");

    static void FailAssertion()
        => 1.ShouldBe(2);

    static string At<T>(string method, [CallerFilePath] string path = default!)
    {
        var type = typeof(T);
        var typeFullName = type.FullName ??
                           throw new Exception($"Expected type {type.Name} to have a non-null FullName.");

        return $"   at {typeFullName.Replace("+", ".")}.{method} in {path}:line #";
    }
    
    static string NormalizeLineNumbers(string stackTrace)
    {
        return Regex.Replace(stackTrace, @"\.cs:line \d+", ".cs:line #");
    }
}