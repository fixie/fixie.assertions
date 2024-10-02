﻿namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertNullVsNotNull()
    {
        object? o = null;
        o.ShouldBe(null);
        Contradiction(o, x => x.ShouldNotBeNull(),
            """
            x should not be null but was null.
            """);

        o = new object();
        o.ShouldNotBeNull();
        Contradiction(o, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was

                {}
            """);
    }

    public void ShouldAssertExpressions()
    {
        Contradiction(3, value => value.ShouldSatisfy(x => x > 4),
            """
            value should satisfy
            
                > 4
            
            but was
            
                3
            """);
        Contradiction(4, value => value.ShouldSatisfy(y => y > 4),
            """
            value should satisfy
            
                > 4
            
            but was
            
                4
            """);
        5.ShouldSatisfy(x => x > 4);

        Contradiction(3, value => value.ShouldSatisfy(abc => abc >= 4),
            """
            value should satisfy
            
                >= 4
            
            but was
            
                3
            """);
        4.ShouldSatisfy(x => x >= 4);
        5.ShouldSatisfy(x => x >= 4);

        Func<int, bool> someExpression = x => x >= 4;
        Contradiction(3, value => value.ShouldSatisfy(someExpression),
            """
            value should satisfy
            
                someExpression
            
            but was
            
                3
            """);

        var a1 = new object();
        var a2 = new object();

        a1.ShouldSatisfy(x => x == a1);
        Contradiction(a1, value => value.ShouldSatisfy(x => x == a2),
            """
            value should satisfy
            
                == a2
            
            but was
            
                {}
            """);

        object? nullObject = null;
        nullObject.ShouldSatisfy(_ => _ == null);
        Contradiction(nullObject, value => value.ShouldSatisfy(_ => _ != null),
            """
            value should satisfy
            
                != null
            
            but was
            
                null
            """);
    }
}