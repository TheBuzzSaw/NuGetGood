using System.Text.Json.Serialization;

namespace NuGetGood
{
    public record NuGetResource
    {
        [JsonPropertyName("@id")]
        public string Id { get; init; }
        [JsonPropertyName("@type")]
        public string Type { get; init; }
        [JsonPropertyName("comment")]
        public string Comment { get; init; }
    }
}
