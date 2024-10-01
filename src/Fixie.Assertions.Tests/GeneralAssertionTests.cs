namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new EmptyReferenceA();

        nullObject.ShouldBe(null);
        nonNullObject.ShouldNotBeNull();

        Contradiction((object?)null, x => x.ShouldBe(nonNullObject),
            """
            x should be
            
                {}
            
            but was
            
                null
            """);
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                {}
            """);
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            """
            x should not be null but was null.
            """);
    }

    public void ShouldAssertExpressions()
    {
        Contradiction(3, value => value.Should(x => x > 4),
            """
            value should be
            
                > 4
            
            but was
            
                3
            """);
        Contradiction(4, value => value.Should(y => y > 4),
            """
            value should be
            
                > 4
            
            but was
            
                4
            """);
        5.Should(x => x > 4);

        Contradiction(3, value => value.Should(abc => abc >= 4),
            """
            value should be
            
                >= 4
            
            but was
            
                3
            """);
        4.Should(x => x >= 4);
        5.Should(x => x >= 4);

        Func<int, bool> someExpression = x => x >= 4;
        Contradiction(3, value => value.Should(someExpression),
            """
            value should be
            
                someExpression
            
            but was
            
                3
            """);

        var a1 = new EmptyReferenceA();
        var a2 = new EmptyReferenceA();

        a1.Should(x => x == a1);
        Contradiction(a1, value => value.Should(x => x == a2),
            """
            value should be
            
                == a2
            
            but was
            
                {}
            """);

        object? nullObject = null;
        nullObject.Should(_ => _ == null);
        Contradiction(nullObject, value => value.Should(_ => _ != null),
            """
            value should be
            
                != null
            
            but was
            
                null
            """);

        Contradiction('x', value => value.Should(_ => _ != 'x', expectationBody: null!),
            "expectationBody should not be null but was null.");
    }

    class EmptyReferenceA;
}