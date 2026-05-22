using Moors.Data;

namespace Moors.Services
{
    public static class PosterSeeder
    {
        public static void SeedPosters(AppDbContext context)
        {
            var posterMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["American Psycho"] = "/images/posters/american-psycho.jpg",
                ["Babylon"] = "/images/posters/babylon.jpg",
                ["Black Swan"] = "/images/posters/black-swan.jpg",
                ["Clueless"] = "/images/posters/clueless.jpg",
                ["Die Hard"] = "/images/posters/die-hard.jpg",
                ["Eternal Sunshine of the Spotless Mind"] = "/images/posters/eternal-sunshine.jpg",
                ["Ex Machina"] = "/images/posters/ex-machina.jpg",
                ["Fight Club"] = "/images/posters/fight-club.jpg",
                ["Girl, Interrupted"] = "/images/posters/girl-interrupted.jpg",
                ["Gone Girl"] = "/images/posters/gone-girl.jpg",
                ["How to Lose a Guy in 10 Days"] = "/images/posters/how-to-lose-a-guy-in-10-days.jpg",
                ["Interstellar"] = "/images/posters/interstellar.jpg",
                ["Kill Bill: Vol. 1"] = "/images/posters/kill-bill.jpg",
                ["La La Land"] = "/images/posters/la-la-land.jpg",
                ["Legally Blonde"] = "/images/posters/legally-blonde.jpg",
                ["Men in Black"] = "/images/posters/men-in-black.jpg",
                ["Once Upon a Time... in Hollywood"] = "/images/posters/once-upon-a-time-in-hollywood.jpg",
                ["Pearl"] = "/images/posters/pearl.jpg",
                ["Poor Things"] = "/images/posters/poor-things.jpg",
                ["Scarface"] = "/images/posters/scarface.jpg",
                ["Taxi Driver"] = "/images/posters/taxi-driver.jpg",
                ["The Godfather"] = "/images/posters/the-godfather.jpg",
                ["The Hangover"] = "/images/posters/the-hangover.jpg",
                ["The Matrix"] = "/images/posters/the-matrix.jpg",
                ["The Sixth Sense"] = "/images/posters/the-sixth-sense.jpg",
                ["The Social Network"] = "/images/posters/the-social-network.jpg",
                ["The Usual Suspects"] = "/images/posters/the-usual-suspects.jpg",
                ["The Wolf of Wall Street"] = "/images/posters/the-wolf-of-wallstreet.jpg",
                ["Uptown Girls"] = "/images/posters/uptown-girls.jpg",
                ["Whiplash"] = "/images/posters/whiplash.jpg"
            };

            var movies = context.Movies.ToList();

            foreach (var movie in movies)
            {
                var normalizedTitle = movie.Title.Trim();

                if (normalizedTitle.Contains(" ("))
                {
                    normalizedTitle = normalizedTitle[..normalizedTitle.IndexOf(" (")];
                }

                if (string.IsNullOrWhiteSpace(movie.PosterUrl) &&
                    posterMap.TryGetValue(normalizedTitle, out var posterUrl))
                {
                    movie.PosterUrl = posterUrl;
                }
            }

            context.SaveChanges();
        }
    }
}