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

        Serialize((IEnumerable<bool>?)null).ShouldBe("null");
        Serialize((Custom?)null).ShouldBe("null");
        Serialize((ICustom?)null).ShouldBe("null");
        Serialize((int[]?)null).ShouldBe("null");
        Serialize((int[]?)[]).ShouldBe("[]");
    }

    public void ShouldAssertLists()
    {
        int[] empty = [];
        int[] emptyNewlyAllocated = {};
        int[] arrayThree = [1, 2, 3];
        List<int> listThree = [1, 2, 3];
        
        empty.ShouldBe(empty);
        empty.ShouldBe([]);

        arrayThree.ShouldBe(arrayThree);
        listThree.ShouldBe(listThree);

        Contradiction(empty, x => x.ShouldBe([1, 2, 3]),
            """
            x should be
            
                [
                    1,
                    2,
                    3
                ]

            but was
            
                []
            """);

        Contradiction(empty, x => x.ShouldBe(emptyNewlyAllocated),
            """
            x should be
            
                []

            but was
            
                []
            
            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        empty.ShouldMatch(emptyNewlyAllocated);

        Contradiction(arrayThree, x => x.ShouldBe([1, 2, 3]),
            """
            x should be
            
                [
                    1,
                    2,
                    3
                ]

            but was
            
                [
                    1,
                    2,
                    3
                ]

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        arrayThree.ShouldMatch([1, 2, 3]);

        Contradiction(listThree, x => x.ShouldBe([1, 2, 3]),
            """
            x should be
            
                [
                    1,
                    2,
                    3
                ]

            but was
            
                [
                    1,
                    2,
                    3
                ]

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        listThree.ShouldMatch([1, 2, 3]);

        Type[] intBool = [typeof(int), typeof(bool)];
        Type[] intBoolString = [typeof(int), typeof(bool), typeof(string)];

        intBool.ShouldBe(intBool);
        intBoolString.ShouldBe(intBoolString);

        Contradiction(intBool, x => x.ShouldBe([typeof(int), typeof(bool)]),
            """
            x should be

                [
                    typeof(int),
                    typeof(bool)
                ]
            
            but was

                [
                    typeof(int),
                    typeof(bool)
                ]

            These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
            """);
        intBool.ShouldMatch([typeof(int), typeof(bool)]);

        Contradiction(intBool, x => x.ShouldMatch(intBoolString),
            """
            x should match

                [
                    typeof(int),
                    typeof(bool),
                    typeof(string)
                ]
            
            but was

                [
                    typeof(int),
                    typeof(bool)
                ]
            """);

        int[]? nullableArray = null;
        nullableArray.ShouldBe(null);
        nullableArray.ShouldMatch(null);
        Contradiction(nullableArray, x => x.ShouldBe([]),
            """
            x should be
            
                []
            
            but was
            
                null
            """);
        Contradiction(nullableArray, x => x.ShouldMatch([]),
            """
            x should match
            
                []
            
            but was
            
                null
            """);
        
        nullableArray = [];
        Contradiction(nullableArray, x => x.ShouldBe(null),
            """
            x should be
            
                null
            
            but was
            
                []
            """);
        Contradiction(nullableArray, x => x.ShouldMatch(null),
            """
            x should match
            
                null
            
            but was
            
                []
            """);
        nullableArray.ShouldBe([]);
        nullableArray.ShouldMatch([]);
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