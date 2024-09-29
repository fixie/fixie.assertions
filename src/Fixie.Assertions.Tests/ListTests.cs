namespace Tests;

class ListTests
{
    public void ShouldSerializeLists()
    {
        Serialize((int[]?)null)
            .ShouldBe("null");

        Serialize((int[])[])
            .ShouldBe("[]");

        Serialize((int[])[1, 2, 3])
            .ShouldBe("""
                      [
                        1,
                        2,
                        3
                      ]
                      """);

        Serialize((IEnumerable<int>)[1, 2, 3])
            .ShouldBe("""
                      [
                        1,
                        2,
                        3
                      ]
                      """);

        //TODO: The indentation of lists became incorrect once the contained item type, in this case
        //      strings, started to be written using WriteRawValue. Apparently the preceding expected
        //      whitespace gets skipped as a consequence of WriteRawValue.
        Serialize((List<string>)["ABC", "123"])
            .ShouldBe("""
                      [
                        "ABC",
                        "123"
                      ]
                      """);
    }
}