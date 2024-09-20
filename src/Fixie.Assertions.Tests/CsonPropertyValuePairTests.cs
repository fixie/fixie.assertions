namespace Tests;

class CsonKeyValuePairTests
{
    public void ShouldSerializeAmbiguouslyUninterestingObjects()
    {
        Serialize(new object())
            .ShouldBe("{}");

        Serialize(new Empty())
            .ShouldBe("{}");

        Serialize(new PointFields { x = 1, y = 2 })
            .ShouldBe("{}");

        Serialize(new Sample("A"))
            .ShouldBe("{}");
            
        Serialize(new SampleNullToString())
            .ShouldBe("{}");
    }

    public void ShouldSerializeObjectProperties()
    {
        Serialize((object?)null)
            .ShouldBe("null");

        Serialize(new Person("Alex", 32))
            .ShouldBe("""
                      {
                        "Name": "Alex",
                        "Age": 32
                      }
                      """);

        Serialize(new PointProperties { X = 1, Y = 2 })
            .ShouldBe("""
                      {
                        "X": 1,
                        "Y": 2
                      }
                      """);

        Serialize(HttpMethod.Post)
            .ShouldBe("""
                      {
                        "Method": "POST"
                      }
                      """);
    }

    static string Serialize<T>(T value)
        => CsonSerializer.Serialize(value);

        struct Empty;
    
    struct PointFields
    {
        public int x;
        public int y;
        
        public override string ToString() => $"({x},{y})";
    }

    struct PointProperties
    {
        public int X { get; init; }
        public int Y { get; init; }
        
        public override string ToString() => $"({X},{Y})";
    }

    record Person(string Name, int Age);

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
    }

    class SampleNullToString
    {
        public override string? ToString() => null;
    }
}