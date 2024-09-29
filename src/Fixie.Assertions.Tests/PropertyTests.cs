namespace Tests;

class PropertyTests
{
    public void ShouldSerializeAmbiguouslyUninterestingObjects()
    {
        Serialize(new object())
            .ShouldBe("{}");

        Serialize(new Empty())
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
                        Name = "Alex",
                        Age = 32
                      }
                      """);

        var pointFields = new PointFields
        {
            x = 1,
            y = 2
        };
        
        Serialize(pointFields)
            .ShouldBe(""""
                      {
                        x = 1,
                        y = 2
                      }
                      """");

        var pointProperties = new PointProperties
        {
            X = 1,
            Y = 2
        };

        Serialize(pointProperties)
            .ShouldBe("""
                      {
                        X = 1,
                        Y = 2
                      }
                      """);

        Serialize(HttpMethod.Post)
            .ShouldBe("""
                      {
                        Method = "POST"
                      }
                      """);

        var sampleWithIndexer = new SampleWithIndexer
        {
            Name = "Alex",
            Age = 32
        };

        Serialize(sampleWithIndexer)
            .ShouldBe("""
                      {
                        Name = "Alex",
                        Age = 32
                      }
                      """);
    }

    public void ShouldSerializeUnrecognizedNullableValueTypes()
    {
        var point = new PointProperties
        {
            X = 1,
            Y = 2
        };

        var pointSerialized =
            """
            {
              X = 1,
              Y = 2
            }
            """;

        Serialize(point).ShouldBe(pointSerialized);
        Serialize((PointProperties?)null).ShouldBe("null");
        Serialize((PointProperties?)point).ShouldBe(pointSerialized);
    }

    struct Empty;
    
    struct PointFields
    {
        public PointFields() { ignored = null; }

        private string? ignored;
        public int x;
        public int y;
        
        public override string ToString() => $"({x},{y}{ignored})";
    }

    struct PointProperties
    {
        private string? Ignored { get; set; }
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

    class SampleWithIndexer
    {
        public required string Name { get; init; }
        public required int Age { get; init; }
        public int this[int index] => 1;
    }
}