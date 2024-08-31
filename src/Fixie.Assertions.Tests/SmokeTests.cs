namespace Fixie.Assertions.Tests;

public class SmokeTests
{
    public void Pass()
    {
    }

    public void Fail()
    {
        throw new Exception("This test is written to fail, to demonstrate failure reporting.");
    }
}