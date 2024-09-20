namespace Tests;

class GeneralAssertionTests
{
    public void ShouldAssertEquatables()
    {
        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get), "x should be GET but was POST");
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
            $"x should be null but was {FullName<SampleB>()}");
        Contradiction(objectB, x => x.ShouldBe((SampleB?)null),
            $"x should be null but was {FullName<SampleB>()}");

        Contradiction((object)objectB, x => x.ShouldBe((object)objectA),
            $"x should be {FullName<SampleA>()} but was {FullName<SampleB>()}");
        Contradiction((object)objectA, x => x.ShouldBe((object)objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleA>()}");

        Contradiction(nonNullObjectWithNullToString, x => x.ShouldBe(objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleNullToString>()}");
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
            $"x should be {FullName<SampleA>()} but was null");
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            $"x should be null but was {FullName<SampleA>()}");
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            "x should not be null but was null");
    }

    public void ShouldAssertTypeEquality()
    {
        typeof(int).ShouldBe(typeof(int));
        
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(Tests.Utility)");
        Contradiction(typeof(bool), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(bool)");
        
        Contradiction(typeof(object), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(object)");
        Contradiction(typeof(GeneralAssertionTests), x => x.ShouldBe(typeof(object)), $"x should be typeof(object) but was typeof({FullName<GeneralAssertionTests>()})");

        Contradiction((Type?)null, x => x.ShouldBe(typeof(object)), $"x should be typeof(object) but was null");
        Contradiction((Type?)null, x => x.ShouldBe(typeof(Type)), $"x should be typeof(System.Type) but was null");
        ((Type?)null).ShouldBe(null);
    }

    public void ShouldAssertValueHasType()
    {
        1.ShouldBe<int>();

        Contradiction(new SampleA(), x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof({FullName<SampleA>()})");
        Contradiction(true, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(bool)");
        
        Contradiction(new object(), x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(object)");
        new GeneralAssertionTests().ShouldBe<object>();

        // Just like with the `is` keyword, although expressions may have some known compile time type, null values do not have a type.
        Contradiction((int?)null, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was null");
        Contradiction((Exception?)null, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was null");
        Contradiction((GeneralAssertionTests?)null, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was null");

        Exception exception = new DivideByZeroException();
        Exception exceptionAsAbstraction = exception.ShouldBe<Exception>();
        exceptionAsAbstraction.ShouldBe(exception);
        DivideByZeroException exceptionAsConcretion = exception.ShouldBe<DivideByZeroException>();
        exceptionAsConcretion.ShouldBe(exception);
    }

    public void ShouldAssertStructs()
    {
        var guidA = Guid.NewGuid();
        var guidB = Guid.NewGuid();

        guidA.ShouldBe(guidA);
        guidB.ShouldBe(guidB);
        
        Contradiction(guidA, x => x.ShouldBe(guidB), $"x should be {guidB} but was {guidA}");

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
        
        Contradiction(origin, x => x.ShouldBe(positionA), $"x should be (1,2) but was (0,0)");
    }

    public void ShouldAssertLists()
    {
        Contradiction((object)new int[]{}, x => x.ShouldBe((int[])[]),
            """
            x should be [] but was []

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
                    Sample B,
                    Sample A
                ]

            but was
                [
                    Sample A,
                    Sample B
                ]
            """);
    }

    public void ShouldAssertExpressions()
    {
        Contradiction(3, value => value.Should(x => x > 4), "value should be > 4 but was 3");
        Contradiction(4, value => value.Should(y => y > 4), "value should be > 4 but was 4");
        5.Should(x => x > 4);

        Contradiction(3, value => value.Should(abc => abc >= 4), "value should be >= 4 but was 3");
        4.Should(x => x >= 4);
        5.Should(x => x >= 4);

        Func<int, bool> someExpression = x => x >= 4;
        Contradiction(3, value => value.Should(someExpression), "value should be someExpression but was 3");

        var a1 = new SampleA();
        var a2 = new SampleA();

        a1.Should(x => x == a1);
        Contradiction(a1, value => value.Should(x => x == a2), $"value should be == a2 but was {FullName<SampleA>()}");

        object? nullObject = null;
        nullObject.Should(_ => _ == null);
        Contradiction(nullObject, value => value.Should(_ => _ != null), $"value should be != null but was null");

        Contradiction('x', value => value.Should(_ => _ != 'x', expectationBody: null), $"expectationBody should not be null but was null");
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

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
    }

    class SampleNullToString
    {
        public override string? ToString() => null;
    }
}