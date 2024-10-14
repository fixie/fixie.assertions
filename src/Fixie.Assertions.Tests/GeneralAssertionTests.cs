namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertNullVsNotNull()
    {
        Sample? nullableReference = null;
        char? nullableValue = null;

        nullableReference.ShouldBe(null);
        nullableValue.ShouldBe(null);

        Contradiction(nullableReference, x => x.ShouldNotBeNull(), "x should not be null but was null.");
        Contradiction(nullableValue, x => x.ShouldNotBeNull(), "x should not be null but was null.");

        nullableReference = new(1);
        nullableValue = 'A';

        Sample nonNullReference = nullableReference.ShouldNotBeNull();
        (nonNullReference != null).ShouldBe(true);
        nullableReference.ShouldBe(nonNullReference);

        char nonNullValue = nullableValue.ShouldNotBeNull();
        nullableValue.ShouldBe(nonNullValue);

        Contradiction(nullableReference, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was

                {
                    Value = 1
                }
            """);

        Contradiction(nullableValue, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was

                'A'
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

    public void ShouldAssertStructuralEquality()
    {
        var one = new Sample(1);
        var two = new Sample(2);

        one.ShouldMatch(one);
        one.ShouldMatch(new Sample(1));

        two.ShouldMatch(two);
        two.ShouldMatch(new Sample(2));

        one.ShouldMatch(new
        {
            Value = 1
        });

        Contradiction(one, x => x.ShouldMatch(two),
            """
            x should match

                {
                    Value = 2
                }

            but was

                {
                    Value = 1
                }
            """);

        Contradiction(one, x => x.ShouldMatch(new { Value = 2 }),
            """
            x should match

                {
                    Value = 2
                }

            but was

                {
                    Value = 1
                }
            """);

        Contradiction(one, x => x.ShouldMatch(new { Value = 1, Extra = 'A' }),
            """
            x should match

                {
                    Extra = 'A',
                    Value = 1
                }

            but was

                {
                    Value = 1
                }
            """);
    }

    class Sample(int value)
    {
        public int Value => value;
    };
}