namespace Tests;

class BoolTests
{
    public void ShouldSerializeBools()
    {
        Serialize(true).ShouldBe("true");
        Serialize(false).ShouldBe("false");

        Serialize((bool?)null).ShouldBe("null");
        Serialize((bool?)true).ShouldBe("true");
        Serialize((bool?)false).ShouldBe("false");
    }

    public void ShouldAssertBools()
    {
        true.ShouldBe(true);
        false.ShouldBe(false);

        Contradiction(true, x => x.ShouldBe(false), "x should be false but was true");
        Contradiction(false, x => x.ShouldBe(true), "x should be true but was false");
    }

    public void ShouldAssertNullableBools()
    {
        bool? nullableBool = null;

        nullableBool.ShouldBe(null);
        Contradiction(nullableBool, x => x.ShouldBe(false), "x should be false but was null");
        Contradiction(nullableBool, x => x.ShouldBe(true), "x should be true but was null");

        nullableBool = false;

        Contradiction(nullableBool, x => x.ShouldBe(null), "x should be null but was false");
        nullableBool.ShouldBe(false);
        Contradiction(nullableBool, x => x.ShouldBe(true), "x should be true but was false");

        nullableBool = true;

        Contradiction(nullableBool, x => x.ShouldBe(null), "x should be null but was true");
        Contradiction(nullableBool, x => x.ShouldBe(false), "x should be false but was true");
        nullableBool.ShouldBe(true);
    }
}