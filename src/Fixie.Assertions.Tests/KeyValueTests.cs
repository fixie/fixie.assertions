using System.Collections;

namespace Tests;

class KeyValueTests
{
    public void ShouldSerializeKeyValuePairs()
    {
        var dictionary123 = """
                            {
                              [1] = "*",
                              [2] = "**",
                              [3] = "***"
                            }
                            """;

        Serialize((Dictionary<string, string>)[]).ShouldBe("{}");
        Serialize(new Dictionary<string, string>
        {
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value",
            ["Third Key"] = "Third Value"
        }).ShouldBe("""
                    {
                      ["First Key"] = "First Value",
                      ["Second Key"] = "Second Value",
                      ["Third Key"] = "Third Value"
                    }
                    """);

        Serialize(new Custom(0)).ShouldBe("{}");
        Serialize(new Custom(1))
            .ShouldBe("""
                      {
                        [1] = "*"
                      }
                      """);
        Serialize(new Custom(3)).ShouldBe(dictionary123);
        Serialize((ICustom)new Custom(3)).ShouldBe(dictionary123);

        Serialize((IEnumerable<KeyValuePair<int,string>>)new Custom(3)).ShouldBe(dictionary123);

        Serialize((Dictionary<string, SortedDictionary<int, bool>>)[]).ShouldBe("{}");
        Serialize(new Dictionary<string, SortedDictionary<int, bool>>
        {
            ["First Key"] = new()
            {
                [3] = true,
                [2] = false,
                [1] = true
            },
            ["Second Key"] = new()
            {
                [2] = false,
                [1] = true,
                [3] = true
            },
            ["Third Key"] = new()
            {
                [1] = true,
                [3] = true,
                [2] = false
            }
        }).ShouldBe("""
                    {
                      ["First Key"] = {
                        [1] = true,
                        [2] = false,
                        [3] = true
                      },
                      ["Second Key"] = {
                        [1] = true,
                        [2] = false,
                        [3] = true
                      },
                      ["Third Key"] = {
                        [1] = true,
                        [2] = false,
                        [3] = true
                      }
                    }
                    """);

        Serialize((IEnumerable<KeyValuePair<int, string>>?)null).ShouldBe("null");
        Serialize((Custom?)null).ShouldBe("null");
        Serialize((ICustom?)null).ShouldBe("null");
        Serialize((Dictionary<string, object>?)null).ShouldBe("null");
        Serialize((Dictionary<string, object>?)[]).ShouldBe("{}");
    }

    public void ShouldAssertKeyValuePairs()
    {
        Dictionary<int, string> empty = [];
        Dictionary<int, string> emptyNewlyAllocated = [];
        Dictionary<int, string> unsorted = new()
        {
            [1] = "*",
            [2] = "**",
            [3] = "***"
        };
        SortedDictionary<int, string> sorted = new()
        {
            [1] = "*",
            [2] = "**",
            [3] = "***"
        };;
        
        empty.ShouldBe(empty);

        unsorted.ShouldBe(unsorted);
        sorted.ShouldBe(sorted);

        Contradiction(empty, x => x.ShouldBe(new() { [1] = "*", [2] = "**", [3] = "***" }),
            """
            x should be
            
                {
                  [1] = "*",
                  [2] = "**",
                  [3] = "***"
                }

            but was
            
                {}
            """);

        Contradiction(empty, x => x.ShouldBe(emptyNewlyAllocated),
            """
            x should be
            
                {}

            but was
            
                {}
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        empty.ShouldMatch(emptyNewlyAllocated);

        Contradiction(unsorted, x => x.ShouldBe(new() { [1] = "*", [2] = "**", [3] = "***" }),
            """
            x should be
            
                {
                  [1] = "*",
                  [2] = "**",
                  [3] = "***"
                }

            but was
            
                {
                  [1] = "*",
                  [2] = "**",
                  [3] = "***"
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        unsorted.ShouldMatch(new() { [1] = "*", [2] = "**", [3] = "***" });

        Contradiction(sorted, x => x.ShouldBe(new() { [1] = "*", [2] = "**", [3] = "***" }),
            """
            x should be
            
                {
                  [1] = "*",
                  [2] = "**",
                  [3] = "***"
                }

            but was
            
                {
                  [1] = "*",
                  [2] = "**",
                  [3] = "***"
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        sorted.ShouldMatch(new() { [1] = "*", [2] = "**", [3] = "***" });
        ((IDictionary<int, string>)sorted).ShouldMatch(unsorted);
        ((IDictionary<int, string>)unsorted).ShouldMatch(sorted);

        Dictionary<string, Type> intBool = new()
        {
            ["int"] = typeof(int),
            ["bool"] = typeof(bool)
        };
        Dictionary<string, Type> intBoolString = new()
        {
            ["int"] = typeof(int),
            ["bool"] = typeof(bool),
            ["string"] = typeof(string)
        };;

        intBool.ShouldBe(intBool);
        intBoolString.ShouldBe(intBoolString);

        Contradiction(intBool, x => x.ShouldBe(new() { ["int"] = typeof(int), ["bool"] = typeof(bool) }),
            """
            x should be

                {
                  ["int"] = typeof(int),
                  ["bool"] = typeof(bool)
                }
            
            but was

                {
                  ["int"] = typeof(int),
                  ["bool"] = typeof(bool)
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        intBool.ShouldMatch(new() { ["int"] = typeof(int), ["bool"] = typeof(bool) });

        Contradiction(intBool, x => x.ShouldMatch(intBoolString),
            """
            x should match

                {
                  ["int"] = typeof(int),
                  ["bool"] = typeof(bool),
                  ["string"] = typeof(string)
                }
            
            but was

                {
                  ["int"] = typeof(int),
                  ["bool"] = typeof(bool)
                }
            """);

        Dictionary<int, string>? nullablePairs = null;
        nullablePairs.ShouldBe(null);
        nullablePairs.ShouldMatch(null);
        Contradiction(nullablePairs, x => x.ShouldBe([]),
            """
            x should be
            
                {}
            
            but was
            
                null
            """);
        Contradiction(nullablePairs, x => x.ShouldMatch([]),
            """
            x should match
            
                {}
            
            but was
            
                null
            """);
        
        nullablePairs = [];
        Contradiction(nullablePairs, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                {}
            """);
        Contradiction(nullablePairs, x => x.ShouldMatch(null),
            """
            x should match
            
                null
            
            but was
            
                {}
            """);
        Contradiction(nullablePairs, x => x.ShouldBe([]),
            """
            x should be
            
                {}
            
            but was
            
                {}
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        nullablePairs.ShouldMatch([]);
    }

    class Custom(byte size) : ICustom
    {
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            => new Enumerator(size);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        class Enumerator(byte size) : IEnumerator<KeyValuePair<int, string>>
        {
            int count = 0;

            public KeyValuePair<int, string> Current => new(count, new('*', count));

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                count++;

                return count <= size;
            }

            public void Reset()
                => count = 0;

            public void Dispose() { }
        }
    }

    interface ICustom : IEnumerable<KeyValuePair<int, string>>
    {
    }
}