namespace Tests;

class GuidTests
{
    public void ShouldSerializeGuids()
    {
        Serialize(Guid.Parse("1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"))
            .ShouldBe(
            """
            "1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"
            """);

        Serialize((Guid?)null)
            .ShouldBe("null");

        Serialize((Guid?)Guid.Parse("2fc7251f-a8d0-4573-87a5-d12408231e77"))
            .ShouldBe(
            """
            "2fc7251f-a8d0-4573-87a5-d12408231e77"
            """);
    }

    public void ShouldAssertGuids()
    {
        var guidA = Guid.NewGuid();
        var guidB = Guid.NewGuid();

        guidA.ShouldBe(guidA);
        guidB.ShouldBe(guidB);
        
        Contradiction(guidA, x => x.ShouldBe(guidB),
            $"""
             x should be
             
                 "{guidB}"
             
             but was
             
                 "{guidA}"
             """);

        ((Guid?)null).ShouldBe(null);
        Contradiction((Guid?)null, x => x.ShouldBe(guidA),
            $"""
             x should be
             
                 "{guidA}"
             
             but was
             
                 null
             """);
        Contradiction((Guid?)guidB, x => x.ShouldBe(null),
            $"""
             x should be
             
                 null
             
             but was
             
                 "{guidB}"
             """);
    }
}