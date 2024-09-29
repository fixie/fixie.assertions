namespace Tests;

class CsonKeyValueTests
{
    public void ShouldSerializeDictionaries()
    {
        Dictionary<string, object>? stringKeys = null;

        Serialize(stringKeys)
            .ShouldBe("null");

        stringKeys = new Dictionary<string, object>
        {
            ["First Key"] = "First Value",
            ["Second Key"] = "Second Value",
            ["Third Key"] = "Third Value"
        };

        Serialize(stringKeys)
            .ShouldBe("""
                      {
                        ["First Key"] = "First Value",
                        ["Second Key"] = "Second Value",
                        ["Third Key"] = "Third Value"
                      }
                      """);

        Dictionary<int, object>? numericKeys = null;

        Serialize(numericKeys)
            .ShouldBe("null");

        numericKeys = new Dictionary<int, object>
        {
            [1] = "First Value",
            [2] = "Second Value",
            [3] = "Third Value"
        };

        Serialize(numericKeys)
            .ShouldBe("""
                      {
                        [1] = "First Value",
                        [2] = "Second Value",
                        [3] = "Third Value"
                      }
                      """);
    }
}