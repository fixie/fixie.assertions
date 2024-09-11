namespace Fixie.Assertions.Tests;

class PrimitiveAssertionTests
{
    public void ShouldAssertBools()
    {
        true.ShouldBe(true);
        false.ShouldBe(false);

        Contradiction(true, x => x.ShouldBe(false), "x should be false but was true");
        Contradiction(false, x => x.ShouldBe(true), "x should be true but was false");
    }
}