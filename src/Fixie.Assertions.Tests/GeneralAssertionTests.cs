namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertEquatables()
    {
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

    public void ShouldAssertStructs()
    {
        var emptyA = new Empty();
        var emptyB = new Empty();

        emptyA.ShouldBe(emptyA);
        emptyB.ShouldBe(emptyB);
        emptyA.ShouldBe(emptyB);

        var origin = new Point();
        var positionA = new Point { x = 1, y = 2 };
        var positionB = new Point { x = 1, y = 2 };

        origin.ShouldBe(origin);
        positionA.ShouldBe(positionA);
        positionB.ShouldBe(positionB);
        positionA.ShouldBe(positionB);

        Contradiction(origin, x => x.ShouldBe(positionA),
            """
            x should be
            
                {
                  x = 1,
                  y = 2
                }
            
            but was
            
                {
                  x = 0,
                  y = 0
                }
            """);

        var nameA = new Name { Given = "Alice", Family = "Smith" };
        var nameB = new Name { Given = "Bob", Family = "Jones" };
        
        ((Name?)null).ShouldBe(null);
        Contradiction((Name?)null, x => x.ShouldBe(nameA),
            """
            x should be

                {
                  Given = "Alice",
                  Family = "Smith"
                }

            but was

                null
            """);
        Contradiction((Name?)nameB, x => x.ShouldBe(null),
            """
            x should be

                null

            but was

                {
                  Given = "Bob",
                  Family = "Jones"
                }
            """);
    }

    public void ShouldAssertLists()
    {
        Contradiction((object)new int[]{}, x => x.ShouldBe((int[])[]),
            """
            x should be
            
                []
            
            but was
            
                []

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);

        new int[]{}.ShouldMatch([]);

        Contradiction(new[] { 0 }, x => x.ShouldMatch([]),
            """
            x should be

                []

            but was

                [
                  0
                ]
            """);

        Contradiction(new int[] { }, x => x.ShouldMatch([0]),
            """
            x should be

                [
                  0
                ]

            but was

                []
            """);

        new[] { false, true, false }.ShouldMatch([false, true, false]);

        Contradiction(new[] { false, true, false }, x => x.ShouldMatch([false, true]),
            """
            x should be

                [
                  false,
                  true
                ]

            but was

                [
                  false,
                  true,
                  false
                ]
            """);
        
        new[] { 'A', 'B', 'C' }.ShouldMatch(['A', 'B', 'C']);
        
        Contradiction(new[] { 'A', 'B', 'C' }, x => x.ShouldMatch(['A', 'C']),
            """
            x should be

                [
                  'A',
                  'C'
                ]

            but was

                [
                  'A',
                  'B',
                  'C'
                ]
            """);

        new[] { "A", "B", "C" }.ShouldMatch(["A", "B", "C"]);

        Contradiction(new[] { "A", "B", "C" }, x => x.ShouldMatch(["A", "C"]),
            """
            x should be

                [
                  "A",
                  "C"
                ]

            but was

                [
                  "A",
                  "B",
                  "C"
                ]
            """);

        new[] { typeof(int), typeof(bool) }.ShouldMatch([typeof(int), typeof(bool)]);

        Contradiction(new[] { typeof(int), typeof(bool) }, x => x.ShouldMatch([typeof(bool), typeof(int)]),
            """
            x should be

                [
                  typeof(bool),
                  typeof(int)
                ]
            
            but was

                [
                  typeof(int),
                  typeof(bool)
                ]
            """);

        var sampleA = new Sample("A");
        var sampleB = new Sample("B");

        new[] { sampleA, sampleB }.ShouldMatch([sampleA, sampleB]);

        Contradiction(new[] { sampleA, sampleB }, x => x.ShouldMatch([sampleB, sampleA]),
            """
            x should be

                [
                  {},
                  {}
                ]

            but was

                [
                  {},
                  {}
                ]

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
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

        Contradiction('x', value => value.Should(_ => _ != 'x', expectationBody: null),
            "expectationBody should not be null but was null.");
    }

    class SampleA;
    class SampleB;
    struct Empty;
    struct Point
    {
        public int x;
        public int y;
        
        public override string ToString() => $"({x},{y})";
    }
    struct Name
    {
        public string Given { get; init; }
        public string Family { get; init; }
    }

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
    }

    class SampleNullToString
    {
        public override string? ToString() => null;
    }
}