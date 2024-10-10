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

        Contradiction(true, x => x.ShouldBe(false), Inequality("false", "true"));
        Contradiction(false, x => x.ShouldBe(true), Inequality("true", "false"));
    }

    public void ShouldAssertNullableBools()
    {
        bool? nullableBool = null;

        nullableBool.ShouldBe(null);
        Contradiction(nullableBool, x => x.ShouldBe(false), Inequality("false", "null"));
        Contradiction(nullableBool, x => x.ShouldBe(true), Inequality("true", "null"));

        nullableBool = false;

        Contradiction(nullableBool, x => x.ShouldBe(null), Inequality("null", "false"));
        nullableBool.ShouldBe(false);
        Contradiction(nullableBool, x => x.ShouldBe(true), Inequality("true", "false"));

        nullableBool = true;

        Contradiction(nullableBool, x => x.ShouldBe(null), Inequality("null", "true"));
        Contradiction(nullableBool, x => x.ShouldBe(false), Inequality("false", "true"));
        nullableBool.ShouldBe(true);
    }

    static string Inequality(string expectedLiteral, string actualLiteral) =>
        $"""
         x should be

             {expectedLiteral}

         but was

             {actualLiteral}
         """;
}