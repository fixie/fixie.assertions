using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests;

class CsonSerializerProtectionTests
{
    public void ShouldProtectFromDeepRecursion()
    {
        object[] nested = [];

        for (int i = 1; i <= 63; i++)
            nested = [nested];

        CsonSerializer.Serialize(nested).StartsWith('[').ShouldBe(true);

        var exceedDepthLimit = () => {
            nested = [nested];
 
            CsonSerializer.Serialize(nested);
        };

        exceedDepthLimit.ShouldThrow<CsonException>(
            "This type could not be serialized because the object graph is too deep.");
    }

    public void ShouldProtectFromCycles()
    {
        var founder = new Employee
        {
            Name = "Morgan",
            Manager = null
        };

        var supervisor = new Employee
        {
            Name = "Riley",
            Manager = founder
        };

        CsonSerializer.Serialize(founder)
            .ShouldBe("""
                      {
                        "Name": "Morgan",
                        "Manager": null
                      }
                      """);

        CsonSerializer.Serialize(supervisor)
            .ShouldBe("""
                      {
                        "Name": "Riley",
                        "Manager": {
                          "Name": "Morgan",
                          "Manager": null
                        }
                      }
                      """);

        var ouroboros = new Employee
        {
            Name = "Ouroboros",
            Manager = null
        };

        CsonSerializer.Serialize(ouroboros)
            .ShouldBe("""
                      {
                        "Name": "Ouroboros",
                        "Manager": null
                      }
                      """);

        var exceedDepthLimitDueToCycle = () => {
            ouroboros.Manager = ouroboros;

            CsonSerializer.Serialize(ouroboros);
        };

        exceedDepthLimitDueToCycle.ShouldThrow<CsonException>(
            "This type could not be serialized because the object graph is too deep.");
    }

    public void ShouldNotBeAffectedByJsonCustomizationAttributes()
    {
        var model = new JsonCustomizedModel();

        var indentJson = new JsonSerializerOptions { WriteIndented = true };

        JsonSerializer.Serialize(model, indentJson)
            .ShouldBe("""
                      {
                        "custom_name": "Property Value From JsonCustomizedName",
                        "JsonPrivateIncluded": "Property Value From JsonPrivateIncluded",
                        "JsonCustomConverted": "A Key/Value pair was obfuscated during JSON serialization.",
                        "JsonNotIgnoredBecauseNonNull": "Property Value From JsonNotIgnoredBecauseNonNull",
                        "A": 1,
                        "B": 2
                      }
                      """);

        // WARNING: This assertion was originally written as characterization coverage, and is
        //          NOT describing a desirable effect. This is expected to start failing the moment
        //          that CsonSerializer begins to take control over property iteration. Until then,
        //          ShouldMatch is at risk for behaving strangely on structurally-unequal objects
        //          that happen to have equal JSON representations, and on structurally-equal objects
        //          that happen to have unequal JSON representations (such as those spoiled by private
        //          inclusion).
        //
        //          Once this starts failing as expected, the point is to rewrite the assertion to
        //          affirm that the JSON customization attributes do NOT impact the output, in contrast
        //          to the assertions above.

        //TODO:     We experience an intermediate state while we handle dictionaries but not yet
        //          properites, where we get A and B wrapped in braces as expected but missing
        //          their respective property name. It appears to be a bug in JsonSerializer that it
        //          isn't throwing a malformed JSON exception, but we're going with it for now.

        CsonSerializer.Serialize(model)
            .ShouldBe("""
                      {
                        "JsonIgnored": "Property Value From JsonIgnored",
                        "JsonCustomizedName": "Property Value From JsonCustomizedName",
                        "JsonCustomConverted": {
                          "Key": "Key/Value Pair",
                          "Value": "From JsonCustomConverted"
                        },
                        "JsonNotIgnoredBecauseNonNull": "Property Value From JsonNotIgnoredBecauseNonNull",
                        "JsonIgnoredBecauseNull": null,
                        "JsonExtendedData": {
                          ["A"] = {
                            "ValueKind": System.Text.Json.JsonValueKind.Number
                          },
                          ["B"] = {
                            "ValueKind": System.Text.Json.JsonValueKind.Number
                          }
                        }
                      }
                      """);
    }

    class Employee
    {
        public required string Name { get; init; }
        public required Employee? Manager { get; set; }
    }

    class JsonCustomizedModel
    {
        [JsonIgnore]
        public string JsonIgnored
        {
            get => "Property Value From JsonIgnored";
        }

        [JsonPropertyName("custom_name")]
        public string JsonCustomizedName
        {
            get => "Property Value From JsonCustomizedName";
        }

        [JsonInclude]
        private string JsonPrivateIncluded
        {
            get => "Property Value From JsonPrivateIncluded";
        }

        [JsonConverter(typeof(MyCustomConverter))]
        public KeyValuePair<string, string> JsonCustomConverted
        {
            get => new("Key/Value Pair", "From JsonCustomConverted");
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JsonNotIgnoredBecauseNonNull
        {
            get => "Property Value From JsonNotIgnoredBecauseNonNull";
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JsonIgnoredBecauseNull
        {
            get => null;
        }

        [JsonExtensionData]
        public SortedDictionary<string, JsonElement> JsonExtendedData
        {
            get => new()
            {
                { "A", ToElement(1) },
                { "B", ToElement(2) }
            };
        }

        static JsonElement ToElement(int value)
            => JsonDocument.Parse(value.ToString()).RootElement;
    }

    class MyCustomConverter : JsonConverter<KeyValuePair<string, string>>
    {
        public override KeyValuePair<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new UnreachableException();

        public override void Write(Utf8JsonWriter writer, KeyValuePair<string, string> value, JsonSerializerOptions options)
            => writer.WriteStringValue("A Key/Value pair was obfuscated during JSON serialization.");
    }
}