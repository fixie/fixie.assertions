using System.Collections;
using System.Diagnostics;

namespace Tests;

class KeyValueTests
{
    public void ShouldSerializeDictionaryUsingKeyValuePairSyntax()
    {
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
    }

    public void ShouldSerializeIDictionaryAndSubtypesUsingKeyValuePairSyntax()
    {
        Serialize(new CustomDictionary(0)).ShouldBe("{}");
        Serialize(new CustomDictionary(1))
            .ShouldBe("""
                      {
                        [1] = "*"
                      }
                      """);
        Serialize(new CustomDictionary(3)).ShouldBe(Dictionary123);
        Serialize((ICustomDictionary)new CustomDictionary(3)).ShouldBe(Dictionary123);
        Serialize((IDictionary<int, string>)new CustomDictionary(3)).ShouldBe(Dictionary123);
    }

    public void ShouldSerializeIReadOnlyDictionaryAndSubtypesUsingKeyValuePairSyntax()
    {
        Serialize(new CustomReadOnlyDictionary(0)).ShouldBe("{}");
        Serialize(new CustomReadOnlyDictionary(1))
            .ShouldBe("""
                      {
                        [1] = "*"
                      }
                      """);
        Serialize(new CustomReadOnlyDictionary(3)).ShouldBe(Dictionary123);
        Serialize((ICustomReadOnlyDictionary)new CustomReadOnlyDictionary(3)).ShouldBe(Dictionary123);
        Serialize((IReadOnlyDictionary<int, string>)new CustomReadOnlyDictionary(3)).ShouldBe(Dictionary123);
    }

    public void ShouldSerializeAmbiguousCollectionsOfKeyValuePairsAsLists()
    {
        // Once it is unclear whether or not we are interacting with dictionary semantics,
        // the collection must be presented as an ordered list of pair objects.
        //
        // The publicly declared structural surface of a type might include a property
        // declared as IEnumerable<KeyValuePair<int, string>>, and for the purposes of
        // structural presentation and structural equality, we do not want to inspect the
        // runtime type for IDictionary<int, string>. That would leak hidden implementation
        // details in a way that substantively breaks a structural comparison where the
        // comparison object happens to use some other concrete type.

        Serialize((IEnumerable<KeyValuePair<string, string>>)new Dictionary<string, string>
        {
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value",
            ["Third Key"] = "Third Value"
        }).ShouldBe("""
                    [
                      {
                        Key = "First Key",
                        Value = "First Value"
                      },
                      {
                        Key = "Second Key",
                        Value = "Second Value"
                      },
                      {
                        Key = "Third Key",
                        Value = "Third Value"
                      }
                    ]
                    """);

        Serialize((IEnumerable<KeyValuePair<int,string>>)new CustomDictionary(3)).ShouldBe(List123);
        Serialize((IEnumerable<KeyValuePair<int,string>>)new CustomReadOnlyDictionary(3)).ShouldBe(List123);
    }

    public void ShouldSerializeNestedDictioniesRecursively()
    {
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
    }

    public void ShouldSerializeNullCollections()
    {
        Serialize((Dictionary<string, object>?)null).ShouldBe("null");
        Serialize((IDictionary<string, object>?)null).ShouldBe("null");
        Serialize((CustomDictionary?)null).ShouldBe("null");
        Serialize((ICustomDictionary?)null).ShouldBe("null");
        Serialize((CustomReadOnlyDictionary?)null).ShouldBe("null");
        Serialize((ICustomReadOnlyDictionary?)null).ShouldBe("null");
        Serialize((IEnumerable<KeyValuePair<int, string>>?)null).ShouldBe("null");
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

    interface ICustomDictionary : IDictionary<int, string>
    {
    }

    class CustomDictionary(byte size) : ICustomDictionary
    {
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            => new PairEnumerator(size);

        #region Members Irrelevant to the Serializer

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        string IDictionary<int, string>.this[int key]
        {
            get => throw new UnreachableException();
            set => throw new UnreachableException();
        }

        public ICollection<int> Keys => throw new UnreachableException();
        public ICollection<string> Values => throw new UnreachableException();
        public int Count => throw new UnreachableException();
        public bool IsReadOnly => throw new UnreachableException();
        public void Add(int key, string value) => throw new UnreachableException();
        public void Add(KeyValuePair<int, string> item) => throw new UnreachableException();
        public void Clear() => throw new UnreachableException();
        public bool Contains(KeyValuePair<int, string> item) => throw new UnreachableException();
        public bool ContainsKey(int key) => throw new UnreachableException();
        public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex) => throw new UnreachableException();
        public bool Remove(int key) => throw new UnreachableException();
        public bool Remove(KeyValuePair<int, string> item) => throw new UnreachableException();
        public bool TryGetValue(int key, out string value) => throw new UnreachableException();

        #endregion
    }

    interface ICustomReadOnlyDictionary : IReadOnlyDictionary<int, string>
    {
    }

    class CustomReadOnlyDictionary(byte size) : ICustomReadOnlyDictionary
    {
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            => new PairEnumerator(size);

        #region Members Irrelevant to the Serializer

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string this[int key] => throw new UnreachableException();
        public IEnumerable<int> Keys => throw new UnreachableException();
        public IEnumerable<string> Values => throw new UnreachableException();
        public int Count => throw new UnreachableException();
        public bool ContainsKey(int key) => throw new UnreachableException();
        public bool TryGetValue(int key, out string value) => throw new UnreachableException();
        
        #endregion
    }

    class PairEnumerator(byte size) : IEnumerator<KeyValuePair<int, string>>
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

    const string Dictionary123 =
        """
        {
          [1] = "*",
          [2] = "**",
          [3] = "***"
        }
        """;

    const string List123 =
        """
        [
          {
            Key = 1,
            Value = "*"
          },
          {
            Key = 2,
            Value = "**"
          },
          {
            Key = 3,
            Value = "***"
          }
        ]
        """;
}