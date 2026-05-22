using System.Text.Json.Serialization;

namespace Moors.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Genre { get; set; }
        public string? PosterUrl { get; set; }
        public string? Overview { get; set; }
        public string? ReleaseDate { get; set; }
        public int? Runtime { get; set; }
        public int? TmdbId { get; set; }
        public List<UserRating> Ratings { get; set; } = new();
        [JsonIgnore]
        public List<WatchlistEntry> WatchlistEntries { get; set; } = new();
    }
}