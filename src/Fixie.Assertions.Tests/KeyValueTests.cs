using System.Collections;

namespace Tests;

class KeyValueTests
{
    public void ShouldSerializeDictionaries()
    {
        var dictionary123 = """
                            {
                              [1] = "*",
                              [2] = "**",
                              [3] = "***"
                            }
                            """;

        Serialize((Dictionary<string, object>?)null).ShouldBe("null");
        Serialize((IEnumerable<KeyValuePair<int, string>>?)null).ShouldBe("null");
        Serialize((Custom?)null).ShouldBe("null");
        Serialize((ICustom?)null).ShouldBe("null");

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