using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NuGetGood
{
    public record VersionList
    {
        [JsonPropertyName("versions")]
        public ImmutableArray<string> Versions { get; init; }
    }
}
