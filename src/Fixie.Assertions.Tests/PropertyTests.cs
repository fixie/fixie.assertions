namespace Tests;

class PropertyTests
{
    public void ShouldSerializeAmbiguouslyUninterestingObjects()
    {
        Serialize(new object()).ShouldBe("{}");

        Serialize(new { }).ShouldBe("{}");

        Serialize(new Empty()).ShouldBe("{}");

        Serialize(new Sample("A")).ShouldBe("{}");
            
        Serialize(new SampleNullToString()).ShouldBe("{}");

        dynamic empty = new { };
        string serialized = Serialize(empty);
        serialized.ShouldBe("{}");
    }

    public void ShouldSerializeObjectState()
    {
        Serialize((object?)null).ShouldBe("null");
        Serialize((Person?)null).ShouldBe("null");
        Serialize((PointFields?)null).ShouldBe("null");
        Serialize((PointProperties?)null).ShouldBe("null");

        Serialize(
            new
            {
                Name = "Anonymous",
                Age = 64
            })
            .ShouldBe("""
                      {
                        Name = "Anonymous",
                        Age = 64
                      }
                      """);

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

        dynamic dynamic = new
        {
            Name = "Dynamic",
            Age = -1
        };
        string serialized = Serialize(dynamic);
        serialized.ShouldBe("""
                               {
                                 Name = "Dynamic",
                                 Age = -1
                               }
                               """);
    }

    public void ShouldSerializeNestedObjectStateRecursively()
    {
        Serialize(
            new
            {
                Name = "Anonymous",
                Age = 64,
                NestedReference = new Person("Alex", 32),
                NestedValue = new PointFields {
                    x = 1,
                    y = 2
                },
                NestedList = (HttpMethod[])[HttpMethod.Get, HttpMethod.Post],
                NestedPairs = (KeyValuePair<string, string>[]) [
                    new KeyValuePair<string, string>("A", "1"),
                    new KeyValuePair<string, string>("B", "2")
                ],
                NestedDynamic =  (dynamic) new {
                    Name = "Dynamic",
                    Age = -1
                }
            })
            .ShouldBe("""
                      {
                        Name = "Anonymous",
                        Age = 64,
                        NestedReference = {
                          Name = "Alex",
                          Age = 32
                        },
                        NestedValue = {
                          x = 1,
                          y = 2
                        },
                        NestedList = [
                          {
                            Method = "GET"
                          },
                          {
                            Method = "POST"
                          }
                        ],
                        NestedPairs = {
                          ["A"] = "1",
                          ["B"] = "2"
                        },
                        NestedDynamic = {
                          Name = "Dynamic",
                          Age = -1
                        }
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