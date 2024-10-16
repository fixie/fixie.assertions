using System.Collections;
using System.Diagnostics;

namespace Tests;

class KeyValueTests
{
    public void ShouldSerializeDictionaryUsingKeyValuePairSyntax()
    {
        Dictionary<string, string> unsorted = new()
        {
            ["Third Key"] = "Third Value",
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value"
        };

        // Although this assertion is technically brittle, dictionary behavior
        // for small numbers of items that are not changing is predictable in
        // practice. If this assertion begins to fail simply due to the chaotic
        // nature of undefined behavior, it can be removed. Until then, it helps
        // to prove that the serialization process is performing a stabilizing sort
        // rather than a particularly lucky bogosort accomplishing it for us.
        unsorted.Keys.ToArray().ShouldMatch(["Third Key", "First Key", "Second Key"]);

        Serialize((Dictionary<string, string>)[]).ShouldBe("{}");
        Serialize(unsorted).ShouldBe(FirstSecondThird);
    }

    public void ShouldSerializeIDictionaryAndSubtypesUsingKeyValuePairSyntax()
    {
        Serialize((SortedDictionary<string, string>)[]).ShouldBe("{}");
        Serialize(new SortedDictionary<string, string>
        {
            ["Third Key"] = "Third Value",
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value"
        }).ShouldBe(FirstSecondThird);

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
        // Once it is unclear whether or not we are interacting with dictionary
        // semantics, the collection must be presented as a meaningfully ordered
        // list of pair objects.
        //
        // The publicly declared structural surface of a type might include a
        // property declared as IEnumerable<KeyValuePair<int, string>>, and for
        // the purposes of structural presentation and structural equality, we
        // do not want to inspect the runtime type for IDictionary<int, string>.
        // That would leak hidden implementation details in a way that
        // substantively breaks a structural comparison where the comparison
        // object happens to use some other concrete type.

        Serialize((IEnumerable<KeyValuePair<string, string>>)new Dictionary<string, string>
        {
            ["Third Key"] = "Third Value",
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value"
        }).ShouldBe("""
                    [
                        {
                            Key = "Third Key",
                            Value = "Third Value"
                        },
                        {
                            Key = "First Key",
                            Value = "First Value"
                        },
                        {
                            Key = "Second Key",
                            Value = "Second Value"
                        }
                    ]
                    """);

        Serialize((IEnumerable<KeyValuePair<int,string>>)new CustomDictionary(3)).ShouldBe(List321);
        Serialize((IEnumerable<KeyValuePair<int,string>>)new CustomReadOnlyDictionary(3)).ShouldBe(List321);
    }

    public void ShouldSerializeNestedDictioniesRecursively()
    {
        Serialize((Dictionary<string, Dictionary<int, char>>)[]).ShouldBe("{}");
        Serialize(new Dictionary<string, Dictionary<int, char>>
        {
            ["Third Key"] = new()
            {
                [1] = 'A',
                [3] = 'B',
                [2] = 'C'
            },
            ["First Key"] = new()
            {
                [3] = 'A',
                [1] = 'B',
                [2] = 'C'
            },
            ["Second Key"] = new()
            {
                [2] = 'A',
                [1] = 'B',
                [3] = 'C'
            }
        }).ShouldBe("""
                    {
                        ["First Key"] = {
                            [1] = 'B',
                            [2] = 'C',
                            [3] = 'A'
                        },
                        ["Second Key"] = {
                            [1] = 'B',
                            [2] = 'A',
                            [3] = 'C'
                        },
                        ["Third Key"] = {
                            [1] = 'A',
                            [2] = 'C',
                            [3] = 'B'
                        }
                    }
                    """);
    }

    public void ShouldSerializeNullCollections()
    {
        Serialize((Dictionary<string, object>?)null).ShouldBe("null");
        Serialize((IDictionary<string, object>?)null).ShouldBe("null");
        Serialize((SortedDictionary<string, object>?)null).ShouldBe("null");
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
            [2] = "**",
            [3] = "***",
            [1] = "*"
        };
        SortedDictionary<int, string> sorted = new()
        {
            [3] = "***",
            [2] = "**",
            [1] = "*"
        };;
        
        empty.ShouldBe(empty);

        unsorted.ShouldBe(unsorted);
        sorted.ShouldBe(sorted);

        Contradiction(empty, x => x.ShouldBe(new() {[3] = "***", [1] = "*", [2] = "**" }),
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

        Contradiction(unsorted, x => x.ShouldBe(new() { [3] = "***", [1] = "*", [2] = "**" }),
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
        unsorted.ShouldMatch(new() { [3] = "***", [1] = "*", [2] = "**" });

        Contradiction(sorted, x => x.ShouldBe(new() { [3] = "***", [1] = "*", [2] = "**" }),
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
        sorted.ShouldMatch(new() { [3] = "***", [1] = "*", [2] = "**" });
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
                    ["bool"] = typeof(bool),
                    ["int"] = typeof(int)
                }
            
            but was

                {
                    ["bool"] = typeof(bool),
                    ["int"] = typeof(int)
                }

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        intBool.ShouldMatch(new() { ["int"] = typeof(int), ["bool"] = typeof(bool) });

        Contradiction(intBool, x => x.ShouldMatch(intBoolString),
            """
            x should match

                {
                    ["bool"] = typeof(bool),
                    ["int"] = typeof(int),
                    ["string"] = typeof(string)
                }
            
            but was

                {
                    ["bool"] = typeof(bool),
                    ["int"] = typeof(int)
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

    public void AllowsSerializationWithWarningCommentsForUnsortableKeyTypes()
    {
        Dictionary<UnsortableKey, string> empty = new();
        Dictionary<UnsortableKey, string> single = new()
        {
            [new(1)] = "Single Value",
        };
        Dictionary<UnsortableKey, string> unsortable = new()
        {
            [new(3)] = "Third Value",
            [new(1)] = "First Value",
            [new(2)] = "Second Value"
        };

        // Although this assertion is technically brittle, dictionary behavior
        // for small numbers of items that are not changing is predictable in
        // practice. If this assertion begins to fail simply due to the chaotic
        // nature of undefined behavior, it will need to be updated.

        Serialize(empty).ShouldBe("{}");
        Serialize(single).ShouldBe(
            """
            {
                [{
                    Value = 1
                }] = "Single Value"
            }
            """);
        Serialize(unsortable).ShouldBe(
            """
            {
                // Entries could not be sorted by key, so their order here may be unstable.
                [{
                    Value = 3
                }] = "Third Value",
                [{
                    Value = 1
                }] = "First Value",
                [{
                    Value = 2
                }] = "Second Value"
            }
            """
            );
    }

    interface ICustomDictionary : IDictionary<int, string>
    {
    }

    class CustomDictionary(byte size) : ICustomDictionary
    {
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            => new PairEnumerator(size);

        #region Members Needed by the Serializer's Attempt to Sort.

        public int Count => size;

        public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex)
        {
            foreach (var entry in this)
                array[arrayIndex++] = new KeyValuePair<int, string>(entry.Key, entry.Value);
        }

        #endregion

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
        public bool IsReadOnly => throw new UnreachableException();
        public void Add(int key, string value) => throw new UnreachableException();
        public void Add(KeyValuePair<int, string> item) => throw new UnreachableException();
        public void Clear() => throw new UnreachableException();
        public bool Contains(KeyValuePair<int, string> item) => throw new UnreachableException();
        public bool ContainsKey(int key) => throw new UnreachableException();
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
        int countdown = size + 1; // MoveNext() will be called for the first time before Current.

        public KeyValuePair<int, string> Current => new(countdown, new('*', countdown));

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            countdown--;

            return countdown >= 1;
        }

        public void Reset()
            => countdown = size + 1;

        public void Dispose() { }
    }

    // This class fulfills the necessary requirements for use as a dictionary key,
    // but is not compatible with Comparer<>.Default used during the serializer's
    // key sorting.
    public class UnsortableKey(int value)
    {
        public int Value => value;

        public override bool Equals(object? obj)
        {
            if (obj is UnsortableKey other)
                return Value == other.Value;

            return false;
        }

        public override int GetHashCode()
            => Value.GetHashCode();
    }

    const string FirstSecondThird =
        """
        {
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value",
            ["Third Key"] = "Third Value"
        }
        """;

    const string Dictionary123 =
        """
        {
            [1] = "*",
            [2] = "**",
            [3] = "***"
        }
        """;

    const string List321 =
        """
        [
            {
                Key = 3,
                Value = "***"
            },
            {
                Key = 2,
                Value = "**"
            },
            {
                Key = 1,
                Value = "*"
            }
        ]
        """;
}