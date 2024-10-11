using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests;

class SerializerProtectionTests
{
    public void ShouldProtectFromDeepRecursion()
    {
        object[] nested = [];

        for (int i = 1; i <= 31; i++)
            nested = [nested];

        Serializer.Serialize(nested).StartsWith('[').ShouldBe(true);

        var exceedDepthLimit = () => {
            nested = [nested];
 
            Serializer.Serialize(nested);
        };

        exceedDepthLimit
            .ShouldThrow<SerializationDepthException>(ExpectedDeepRecursionExceptionMessage);
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

        Serializer.Serialize(founder)
            .ShouldBe("""
                      {
                        Manager = null,
                        Name = "Morgan"
                      }
                      """);

        Serializer.Serialize(supervisor)
            .ShouldBe("""
                      {
                        Manager = {
                          Manager = null,
                          Name = "Morgan"
                        },
                        Name = "Riley"
                      }
                      """);

        var ouroboros = new Employee
        {
            Name = "Ouroboros",
            Manager = null
        };

        Serializer.Serialize(ouroboros)
            .ShouldBe("""
                      {
                        Manager = null,
                        Name = "Ouroboros"
                      }
                      """);

        var exceedDepthLimitDueToCycle = () => {
            ouroboros.Manager = ouroboros;

            Serializer.Serialize(ouroboros);
        };

        exceedDepthLimitDueToCycle
            .ShouldThrow<SerializationDepthException>(ExpectedCycleExceptionMessage);
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

        Serializer.Serialize(model)
            .ShouldBe("""
                      {
                        JsonCustomConverted = {
                          Key = "Key/Value Pair",
                          Value = "From JsonCustomConverted"
                        },
                        JsonCustomizedName = "Property Value From JsonCustomizedName",
                        JsonExtendedData = {
                          ["A"] = {
                            ValueKind = System.Text.Json.JsonValueKind.Number
                          },
                          ["B"] = {
                            ValueKind = System.Text.Json.JsonValueKind.Number
                          }
                        },
                        JsonIgnored = "Property Value From JsonIgnored",
                        JsonIgnoredBecauseNull = null,
                        JsonNotIgnoredBecauseNonNull = "Property Value From JsonNotIgnoredBecauseNonNull"
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

    const string ExpectedDeepRecursionExceptionMessage =
        """
        A value could not be serialized because its object graph is too deep. Below is the start of the message that was interrupted:

        [
          [
            [
              [
                [
                  [
                    [
                      [
                        [
                          [
                            [
                              [
                                [
                                  [
                                    [
                                      [
                                        [
                                          [
                                            [
                                              [
                                                [
                                                  [
                                                    [
                                                      [
                                                        [
                                                          [
                                                            [
                                                              [
                                                                [
                                                                  [
                                                                    [
                                                                      [

        """;

    const string ExpectedCycleExceptionMessage =
        """
        A value could not be serialized because its object graph is too deep. Below is the start of the message that was interrupted:
    
        {
          Manager = {
            Manager = {
              Manager = {
                Manager = {
                  Manager = {
                    Manager = {
                      Manager = {
                        Manager = {
                          Manager = {
                            Manager = {
                              Manager = {
                                Manager = {
                                  Manager = {
                                    Manager = {
                                      Manager = {
                                        Manager = {
                                          Manager = {
                                            Manager = {
                                              Manager = {
                                                Manager = {
                                                  Manager = {
                                                    Manager = {
                                                      Manager = {
                                                        Manager = {
                                                          Manager = {
                                                            Manager = {
                                                              Manager = {
                                                                Manager = {
                                                                  Manager = {
                                                                    Manager = {
                                                                      Manager = {

        """;
}