namespace Tests;

class TextAssertionTests
{
    public void ShouldAssertChars()
    {
        'a'.ShouldBe('a');
        Contradiction('a', x => x.ShouldBe('z'), "x should be 'z' but was 'a'");

        ((char?)null).ShouldBe(null);
        Contradiction((char?)null, x => x.ShouldBe('z'), "x should be 'z' but was null");
        Contradiction((char?)'a', x => x.ShouldBe(null), "x should be null but was 'a'");
    }

    public void ShouldAssertStrings()
    {
        "a☺".ShouldBe("a☺");
        Contradiction("a☺", x => x.ShouldBe("z☺"),
            """
            x should be
            
                "z☺"
            
            but was
            
                "a☺"
            """);

        var newInstance1 = new string("abc");
        var newInstance2 = new string("abc");
        ReferenceEquals(newInstance1, newInstance2).ShouldBe(false);
        (newInstance1 == newInstance2).ShouldBe(true);
        newInstance1.ShouldBe(newInstance2);

        ((string?)null).ShouldBe(null);
        Contradiction((string?)null, x => x.ShouldBe("abc"),
            """
            x should be
            
                "abc"
            
            but was
            
                null
            """);
        Contradiction((string?)"abc", x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                "abc"
            """);

        var lines =
            """
            Line 1
            Line 2
            Line 3
            Line 4
            """;

        Contradiction(lines, x => x.ShouldBe("abc"),
            """"
            x should be

                "abc"
            
            but was

                """
                Line 1
                Line 2
                Line 3
                Line 4
                """
            """");

        Contradiction("abc", x => x.ShouldBe(lines),
            """"
            x should be

                """
                Line 1
                Line 2
                Line 3
                Line 4
                """

            but was

                "abc"
            """");
    }
}