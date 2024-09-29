namespace Tests;

class CsonScalarTests
{
    public void ShouldSerializeBools()
    {
        Serialize(true).ShouldBe("true");
        Serialize(false).ShouldBe("false");

        Serialize((bool?)null).ShouldBe("null");        
        Serialize((bool?)true).ShouldBe("true");
        Serialize((bool?)false).ShouldBe("false");
    }
}