using CsvHelper;
using CsvHelper.Configuration;
using Moors.Data;
using Moors.Models;
using System.Globalization;

namespace Moors.Services
{
    public static class CsvMovieImporter
    {
        private class MovieCsvRow
        {
            public int movieId { get; set; }
            public string title { get; set; } = "";
            public string genres { get; set; } = "";
        }

        public static void ImportMovies(AppDbContext context, string csvPath)
        {
            if (!File.Exists(csvPath))
                return;

            if (context.Movies.Any())
                return;

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var rows = csv.GetRecords<MovieCsvRow>().ToList();

            var movies = rows.Select(r => new Movie
            {
                Id = r.movieId,
                Title = r.title,
                Genre = r.genres
            }).ToList();

            context.Movies.AddRange(movies);
            context.SaveChanges();
        }
    }
}