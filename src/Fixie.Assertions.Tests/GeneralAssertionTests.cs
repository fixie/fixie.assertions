namespace Fixie.Assertions.Tests;

class GeneralAssertionTests
{
    public void ShouldAssertEquatables()
    {
        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get), "x should be GET but was POST");
    }

    public void ShouldAssertObjects()
    {
        var objectA = new SampleA();
        var objectB = new SampleB();

        objectA.ShouldBe(objectA);
        objectB.ShouldBe(objectB);

        Contradiction(objectB, x => x.ShouldBe((object?)null),
            $"x should be null but was {FullName<SampleB>()}");
        Contradiction(objectB, x => x.ShouldBe(objectA),
            $"x should be {FullName<SampleA>()} but was {FullName<SampleB>()}");
        Contradiction(objectA, x => x.ShouldBe(objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleA>()}");
    }

    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new SampleA();

        nullObject.ShouldBe(null);
        nonNullObject.ShouldNotBeNull();

        Contradiction((object?)null, x => x.ShouldBe(nonNullObject),
            $"x should be {FullName<SampleA>()} but was null");
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            $"x should be null but was {FullName<SampleA>()}");
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            "x should not be null but was null");
    }

    class SampleA;
    class SampleB;
}