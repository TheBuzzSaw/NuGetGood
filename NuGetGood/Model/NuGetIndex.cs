using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NuGetGood
{
    public record NuGetIndex
    {
        [JsonPropertyName("version")]
        public string Version { get; init; }
        [JsonPropertyName("resources")]
        public ImmutableArray<NuGetResource> Resources { get; init; }
    }
}
