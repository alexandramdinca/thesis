using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Moors.Data;
using Moors.Models.Tmdb;

namespace Moors.Services
{
    public sealed class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task EnrichMissingMoviesAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            var movies = await context.Movies
                .Where(m => string.IsNullOrWhiteSpace(m.PosterUrl) || string.IsNullOrWhiteSpace(m.Overview))
                .ToListAsync(cancellationToken);

            foreach (var movie in movies)
            {
                try
                {
                    var searchMatch = await SearchBestMatchAsync(movie.Title, cancellationToken);

                    if (searchMatch is null)
                        continue;

                    var details = await GetMovieDetailsAsync(searchMatch.Id, cancellationToken);

                    if (details is null)
                        continue;

                    movie.TmdbId ??= details.Id;

                    if (string.IsNullOrWhiteSpace(movie.PosterUrl) && !string.IsNullOrWhiteSpace(details.PosterPath))
                    {
                        movie.PosterUrl = BuildPosterUrl(details.PosterPath);
                    }

                    if (string.IsNullOrWhiteSpace(movie.Overview))
                    {
                        movie.Overview = details.Overview;
                    }

                    if (string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        movie.ReleaseDate = details.ReleaseDate;
                    }

                    if (movie.Runtime is null)
                    {
                        movie.Runtime = details.Runtime;
                    }

                    if (string.IsNullOrWhiteSpace(movie.Genre) && details.Genres.Count > 0)
                    {
                        movie.Genre = string.Join("|", details.Genres
                            .Where(g => !string.IsNullOrWhiteSpace(g.Name))
                            .Select(g => g.Name!.Trim()));
                    }
                }
                catch
                {
                    // leave empty for now so one failed movie doesn't crash the whole import
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task<TmdbSearchMovie?> SearchBestMatchAsync(string title, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["Tmdb:ApiKey"];
            var baseUrl = _configuration["Tmdb:BaseUrl"];

            var normalizedTitle = NormalizeTitle(title);
            var extractedYear = ExtractYear(title);

            var url = $"{baseUrl}search/movie?api_key={Uri.EscapeDataString(apiKey!)}&query={Uri.EscapeDataString(normalizedTitle)}";

            if (!string.IsNullOrWhiteSpace(extractedYear))
            {
                url += $"&year={Uri.EscapeDataString(extractedYear)}";
            }

            var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url, cancellationToken);

            if (response?.Results is null || response.Results.Count == 0)
                return null;

            var exactTitleMatch = response.Results.FirstOrDefault(r =>
                !string.IsNullOrWhiteSpace(r.Title) &&
                string.Equals(r.Title.Trim(), normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.PosterPath));

            if (exactTitleMatch is not null)
                return exactTitleMatch;

            var firstWithPoster = response.Results.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.PosterPath));
            if (firstWithPoster is not null)
                return firstWithPoster;

            return response.Results.FirstOrDefault();
        }

        private async Task<TmdbMovieDetailsResponse?> GetMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["Tmdb:ApiKey"];
            var baseUrl = _configuration["Tmdb:BaseUrl"];

            var url = $"{baseUrl}movie/{tmdbId}?api_key={Uri.EscapeDataString(apiKey!)}";

            return await _httpClient.GetFromJsonAsync<TmdbMovieDetailsResponse>(url, cancellationToken);
        }

        private string BuildPosterUrl(string posterPath)
        {
            var imageBaseUrl = _configuration["Tmdb:ImageBaseUrl"]?.TrimEnd('/');

            return $"{imageBaseUrl}{posterPath}";
        }

        private static string NormalizeTitle(string title)
        {
            var normalized = title.Trim();
            var yearMarker = normalized.LastIndexOf(" (", StringComparison.Ordinal);

            if (yearMarker > 0 && normalized.EndsWith(")"))
            {
                normalized = normalized[..yearMarker];
            }

            return normalized;
        }

        private static string? ExtractYear(string title)
        {
            var start = title.LastIndexOf(" (", StringComparison.Ordinal);

            if (start > 0 && title.EndsWith(")"))
            {
                var year = title[(start + 2)..^1];
                return year.Length == 4 ? year : null;
            }

            return null;
        }
    }
}