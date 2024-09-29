using System.Collections;

namespace Tests;

class ListTests
{
    public void ShouldSerializeLists()
    {
        var list123 = """
                      [
                        1,
                        2,
                        3
                      ]
                      """;

        Serialize((int[]?)null).ShouldBe("null");
        Serialize((IEnumerable<bool>?)null).ShouldBe("null");
        Serialize((Custom?)null).ShouldBe("null");
        Serialize((ICustom?)null).ShouldBe("null");

        Serialize((int[])[]).ShouldBe("[]");
        Serialize((int[])[1, 2, 3]).ShouldBe(list123);

        Serialize((int[][])[]).ShouldBe("[]");
        Serialize((int[][])[[1 , 2], [3 , 4, 5]])
            .ShouldBe("""
                      [
                        [
                          1,
                          2
                        ],
                        [
                          3,
                          4,
                          5
                        ]
                      ]
                      """);

        Serialize(new Custom(0)).ShouldBe("[]");
        Serialize(new Custom(1))
            .ShouldBe("""
                      [
                        1
                      ]
                      """);
        Serialize(new Custom(3)).ShouldBe(list123);
        Serialize((ICustom)new Custom(3)).ShouldBe(list123);

        Serialize((IEnumerable<int>)[1, 2, 3]).ShouldBe(list123);

        Serialize((List<string[]>)[]).ShouldBe("[]");
        Serialize((List<string[]>)[["ABC", "123"], ["DEF", "456"]])
            .ShouldBe("""
                      [
                        [
                          "ABC",
                          "123"
                        ],
                        [
                          "DEF",
                          "456"
                        ]
                      ]
                      """);
    }

    class Custom(byte size) : ICustom
    {
        public IEnumerator<int> GetEnumerator()
            => new Enumerator(size);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        class Enumerator(byte size) : IEnumerator<int>
        {
            int count = 0;

            public int Current => count;

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

    interface ICustom : IEnumerable<int>
    {
    }
}