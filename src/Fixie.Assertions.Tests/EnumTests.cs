namespace Tests;

class EnumTests
{
    public void ShouldSerializeEnums()
    {
        var prefix = "System.StringSplitOptions";

        Serialize(StringSplitOptions.None).ShouldBe(prefix+".None");
        Serialize(StringSplitOptions.RemoveEmptyEntries).ShouldBe(prefix+".RemoveEmptyEntries");
        Serialize(StringSplitOptions.TrimEntries).ShouldBe(prefix+".TrimEntries");

        Serialize((StringSplitOptions)int.MinValue).ShouldBe($"({prefix})(-2147483648)");
        Serialize((StringSplitOptions)int.MaxValue).ShouldBe($"({prefix})2147483647");

        Serialize((StringSplitOptions?)null).ShouldBe("null");
        Serialize((StringSplitOptions?)StringSplitOptions.None).ShouldBe(prefix+".None");
        Serialize((StringSplitOptions?)StringSplitOptions.RemoveEmptyEntries).ShouldBe(prefix+".RemoveEmptyEntries");
        Serialize((StringSplitOptions?)StringSplitOptions.TrimEntries).ShouldBe(prefix+".TrimEntries");
    }

    public void ShouldAssertEnums()
    {
        StringSplitOptions.None.ShouldBe(StringSplitOptions.None);
        StringSplitOptions.RemoveEmptyEntries.ShouldBe(StringSplitOptions.RemoveEmptyEntries);
        StringSplitOptions.TrimEntries.ShouldBe(StringSplitOptions.TrimEntries);
        ((StringSplitOptions)int.MaxValue).ShouldBe((StringSplitOptions)int.MaxValue);

        Contradiction(StringSplitOptions.None, x => x.ShouldBe(StringSplitOptions.RemoveEmptyEntries),
            """
            x should be

                System.StringSplitOptions.RemoveEmptyEntries
            
            but was
            
                System.StringSplitOptions.None
            """);
        Contradiction((StringSplitOptions)int.MaxValue, x => x.ShouldBe((StringSplitOptions)int.MinValue),
            """
            x should be
            
                (System.StringSplitOptions)(-2147483648)
            
            but was
            
                (System.StringSplitOptions)2147483647
            """);

        ((StringSplitOptions?)null).ShouldBe(null);
        Contradiction((StringSplitOptions?)null, x => x.ShouldBe(StringSplitOptions.None),
            """
            x should be
            
                System.StringSplitOptions.None
            
            but was
            
                null
            """);
        Contradiction((StringSplitOptions?)StringSplitOptions.TrimEntries, x => x.ShouldBe(null),
            """
            x should be

                null
            
            but was
            
                System.StringSplitOptions.TrimEntries
            """);
    }
}