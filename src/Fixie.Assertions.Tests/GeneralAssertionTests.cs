namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertObjects()
    {
        var objectA = new SampleA();
        var objectB = new SampleB();
        object nonNullObjectWithNullToString = new SampleNullToString();

        objectA.ShouldBe(objectA);
        objectB.ShouldBe(objectB);
        nonNullObjectWithNullToString.ShouldBe(nonNullObjectWithNullToString);

        Contradiction(objectB, x => x.ShouldBe((object?)null),
            """
            x should be
            
                null
            
            but was
            
                {}
            """);
        Contradiction(objectB, x => x.ShouldBe((SampleB?)null),
            """
            x should be
            
                null
            
            but was
            
                {}
            """);

        var trivialSimilarity =
            """
            x should be
            
                {}
            
            but was
            
                {}

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """;

        Contradiction((object)objectB, x => x.ShouldBe((object)objectA), trivialSimilarity);
        Contradiction((object)objectA, x => x.ShouldBe((object)objectB), trivialSimilarity);

        Contradiction(nonNullObjectWithNullToString, x => x.ShouldBe(objectB), trivialSimilarity);
    }

    public void ShouldAssertReferenceTypesByTheirNaturalEqualityComparer()
    {
        var uniqueInstanceA = new string("abc");
        var uniqueInstanceB = new string("abc");

        ReferenceEquals(uniqueInstanceA, uniqueInstanceB).ShouldBe(false);
        uniqueInstanceA.ShouldBe(uniqueInstanceB);

        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get),
            """
            x should be

                {
                  Method = "GET"
                }
            
            but was

                {
                  Method = "POST"
                }
            """);
    }

    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new SampleA();

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

        var a1 = new SampleA();
        var a2 = new SampleA();

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

    class SampleA;
    class SampleB;

    class SampleNullToString
    {
        public override string? ToString() => null;
    }
}