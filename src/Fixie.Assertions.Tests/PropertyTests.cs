namespace Tests;

class PropertyTests
{
    public void ShouldSerializeStatelessObjectsAsPresentButEmpty()
    {
        Serialize(new object()).ShouldBe("{}");

        Serialize(new { }).ShouldBe("{}");

        Serialize(new EmptyValue()).ShouldBe("{}");

        Serialize(new EmptyReference()).ShouldBe("{}");

        dynamic empty = new { };
        string serialized = Serialize(empty);
        serialized.ShouldBe("{}");
    }

    public void ShouldSerializeStatefulButStructurallyOpaqueObjectsAsPresentButEmpty()
    {
        Serialize(new StatefulViaPrimaryConstructor("A")).ShouldBe("{}");
        Serialize(new FieldStatefulButStructurallyOpaque()).ShouldBe("{}");
        Serialize(new PropertyStatefulButStructurallyOpaque()).ShouldBe("{}");
    }

    public void ShouldSerializeTreatingPrivateAccessorsAsStructurallyOpaque()
    {
        Serialize(new PublicStateAllAccessorsPrivate()).ShouldBe("{}");
        Serialize(new PublicStateSomeAccessorsPrivate())
            .ShouldBe("""
                      {
                        StructuralProperty = 4
                      }
                      """);
    }

    public void ShouldSerializeTreatingMissingAccessorsAsStructurallyOpaque()
    {
        Serialize(new PublicStateAllAccessorsMissing()).ShouldBe("{}");
        Serialize(new PublicStateSomeAccessorsMissing())
            .ShouldBe("""
                      {
                        StructuralProperty = 4
                      }
                      """);
    }

    public void ShouldSerializeObjectState()
    {
        Serialize((object?)null).ShouldBe("null");
        Serialize((Person?)null).ShouldBe("null");
        Serialize((PointFieldsValue?)null).ShouldBe("null");
        Serialize((PointPropertiesValue?)null).ShouldBe("null");

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

        var pointFields = new PointFieldsValue
        {
            X = 1,
            Y = 2
        };
        
        Serialize(pointFields)
            .ShouldBe(""""
                      {
                        X = 1,
                        Y = 2
                      }
                      """");

        var pointProperties = new PointPropertiesValue
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

    public void ShouldSerializeInheritedStructuralState()
    {
        var sampleInheritance = new SampleInheritance
        {
            ParentField = 1,
            ParentProperty = 'A',
            ChildField = 'B',
            ChildProperty = 2
        };

        Serialize(sampleInheritance)
            .ShouldBe("""
                      {
                        ChildField = 'B',
                        ParentField = 1,
                        ChildProperty = 2,
                        ParentProperty = 'A'
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
                NestedValue = new PointFieldsValue {
                    X = 1,
                    Y = 2
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
                          X = 1,
                          Y = 2
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
        var point = new PointPropertiesValue
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
        Serialize((PointPropertiesValue?)null).ShouldBe("null");
        Serialize((PointPropertiesValue?)point).ShouldBe(pointSerialized);
    }

    public void ShouldAssertUnrecognizedValueTypes()
    {
        PointFieldsValue origin = new();
        PointFieldsValue originNewlyInitialized = new();
        PointFieldsValue fields = new(1, 2);
        PointPropertiesValue properties = new(1, 2);
        
        origin.ShouldBe(origin);
        origin.ShouldBe(originNewlyInitialized);
        fields.ShouldBe(fields);
        properties.ShouldBe(properties);

        Contradiction(origin, x => x.ShouldBe(new(1, 2)),
            """
            x should be
            
                {
                  X = 1,
                  Y = 2
                }

            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);

        origin.ShouldBe(originNewlyInitialized);
        origin.ShouldMatch(originNewlyInitialized);

        fields.ShouldBe(new(1,2));
        fields.ShouldMatch(new(1,2));

        properties.ShouldBe(new(1,2));
        properties.ShouldMatch(new(1,2));

        ((object)properties).ShouldMatch(fields);
        ((object)fields).ShouldMatch(properties);

        PointFieldsValue? nullablePoint = null;
        nullablePoint.ShouldBe(null);
        nullablePoint.ShouldMatch(null);
        Contradiction(nullablePoint, x => x.ShouldBe(new()),
            """
            x should be
            
                {
                  X = 0,
                  Y = 0
                }
            
            but was
            
                null
            """);
        Contradiction(nullablePoint, x => x.ShouldMatch(new()),
            """
            x should match
            
                {
                  X = 0,
                  Y = 0
                }
            
            but was
            
                null
            """);
        
        nullablePoint = new();
        Contradiction(nullablePoint, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);
        Contradiction(nullablePoint, x => x.ShouldMatch(null),
            """
            x should match
            
                null
            
            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);
        nullablePoint.ShouldBe(new());
        nullablePoint.ShouldMatch(new());
    }

    public void ShouldAssertUnrecognizedReferenceTypes()
    {
        PointFieldsReference origin = new();
        PointFieldsReference originNewlyInitialized = new();
        PointFieldsReference fields = new(1, 2);
        PointPropertiesReference properties = new(1, 2);
        
        origin.ShouldBe(origin);
        fields.ShouldBe(fields);
        properties.ShouldBe(properties);

        Contradiction(origin, x => x.ShouldBe(new(1, 2)),
            """
            x should be
            
                {
                  X = 1,
                  Y = 2
                }

            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);

        Contradiction(origin, x => x.ShouldBe(originNewlyInitialized),
            """
            x should be
            
                {
                  X = 0,
                  Y = 0
                }

            but was
            
                {
                  X = 0,
                  Y = 0
                }
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        origin.ShouldMatch(originNewlyInitialized);

        Contradiction(fields, x => x.ShouldBe(new(1,2)),
            """
            x should be
            
                {
                  X = 1,
                  Y = 2
                }

            but was
            
                {
                  X = 1,
                  Y = 2
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        fields.ShouldMatch(new(1,2));

        Contradiction(properties, x => x.ShouldBe(new(1,2)),
            """
            x should be
            
                {
                  X = 1,
                  Y = 2
                }

            but was
            
                {
                  X = 1,
                  Y = 2
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        properties.ShouldMatch(new(1,2));

        ((object)properties).ShouldMatch(fields);
        ((object)fields).ShouldMatch(properties);

        PointFieldsReference? nullablePoint = null;
        nullablePoint.ShouldBe(null);
        nullablePoint.ShouldMatch(null);
        Contradiction(nullablePoint, x => x.ShouldBe(new()),
            """
            x should be
            
                {
                  X = 0,
                  Y = 0
                }
            
            but was
            
                null
            """);
        Contradiction(nullablePoint, x => x.ShouldMatch(new()),
            """
            x should match
            
                {
                  X = 0,
                  Y = 0
                }
            
            but was
            
                null
            """);
        
        nullablePoint = new();
        Contradiction(nullablePoint, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);
        Contradiction(nullablePoint, x => x.ShouldMatch(null),
            """
            x should match
            
                null
            
            but was
            
                {
                  X = 0,
                  Y = 0
                }
            """);
        Contradiction(nullablePoint, x => x.ShouldBe(new()),
            """
            x should be
            
                {
                  X = 0,
                  Y = 0
                }
            
            but was
            
                {
                  X = 0,
                  Y = 0
                }
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        nullablePoint.ShouldMatch(new());
    }

    public void ShouldAssertUnrecognizedEmptyTypes()
    {
        EmptyValue emptyValue = new();
        EmptyReference emptyReference = new();

        emptyValue.ShouldBe(emptyValue);
        emptyValue.ShouldBe(new());
        emptyValue.ShouldMatch(new());

        emptyReference.ShouldBe(emptyReference);
        Contradiction(emptyReference, x => x.ShouldBe(new()),
            """
            x should be
            
                {}
            
            but was

                {}
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        emptyReference.ShouldMatch(new());
        
        ((object)emptyValue).ShouldMatch(emptyReference);
    }

    struct EmptyValue;
    class EmptyReference;
    
    struct PointFieldsValue(int x, int y)
    {
        private string? ignored = null;
        public int X = x;
        public int Y = y;
        
        public override string ToString() => $"({X},{Y}{ignored})";
    }

    struct PointPropertiesValue(int x, int y)
    {
        private string? Ignored { get; set; }
        public int X { get; init; } = x;
        public int Y { get; init; } = y;

        public override string ToString() => $"({X},{Y})";
    }

    class PointFieldsReference(int x = 0, int y = 0)
    {
        private string? ignored = null;
        public int X = x;
        public int Y = y;
        
        public override string ToString() => $"({X},{Y}{ignored})";
    }

    class PointPropertiesReference(int x, int y)
    {
        private string? Ignored { get; set; }
        public int X { get; init; } = x;
        public int Y { get; init; } = y;

        public override string ToString() => $"({X},{Y})";
    }

    record Person(string Name, int Age);

    class StatefulViaPrimaryConstructor(string name)
    {
        public override string ToString() => $"Sample {name}";
    }

    class SampleWithIndexer
    {
        public required string Name { get; init; }
        public required int Age { get; init; }
        public int this[int index] => 1;
    }

    abstract class SampleInheritanceBase
    {
        public int ParentField;
        public char ParentProperty { get; set; }
    }

    class SampleInheritance : SampleInheritanceBase
    {
        public char ChildField;
        public int ChildProperty { get; set; }
    }

    class FieldStatefulButStructurallyOpaque
    {
        private int field = 1;

        public override string ToString() => field.ToString();
    }

    class PropertyStatefulButStructurallyOpaque
    {
        private int Property { get; set; } = 1;
    }

    class PublicStateAllAccessorsPrivate
    {
        // These get accessors appear to reflection on this type as
        // present but private, but appear to reflection on inherited
        // types as missing.

        public int OpaquePropertyA { private get; set; } = 1;
        public int OpaquePropertyB { private get; set; } = 2;
    }

    class PublicStateSomeAccessorsPrivate : PublicStateAllAccessorsPrivate
    {
        // Inherited private accessors appear to reflection on this
        // type as missing, so we also include a private accessor at
        // this level to demonstrate the full inspection.

        public int OpaquePropertyC { private get; set; } = 3;
        public int StructuralProperty { get; set; } = 4;
    }

    class PublicStateAllAccessorsMissing
    {
        int fieldA = 1;
        int fieldB = 2;

        public int OpaquePropertyA { set { fieldA = value; } }
        public int OpaquePropertyB { set { fieldB = value; } }
    }

    class PublicStateSomeAccessorsMissing : PublicStateAllAccessorsMissing
    {
        int fieldC = 3;

        public int OpaquePropertyC { set { fieldC = value; } }
        public int StructuralProperty { get; set; } = 4;
    }
}