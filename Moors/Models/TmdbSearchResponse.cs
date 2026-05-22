using System.Text.Json.Serialization;

namespace Moors.Models.Tmdb
{
    public sealed class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbSearchMovie> Results { get; set; } = new();
    }

    public sealed class TmdbSearchMovie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }
    }
}