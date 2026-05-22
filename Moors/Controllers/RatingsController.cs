using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moors.Data;
using Moors.Models;

namespace Moors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllRatings()
        {
            var ratings = _context.UserRatings.ToList();
            return Ok(ratings);
        }

        [HttpGet("movie/{movieId}")]
        public IActionResult GetRatingsForMovie(int movieId)
        {
            var ratings = _context.UserRatings
                .Where(r => r.MovieId == movieId)
                .ToList();

            return Ok(ratings);
        }

        [HttpGet("movie/{movieId}/average")]
        public IActionResult GetAverageRatingForMovie(int movieId)
        {
            var movieExists = _context.Movies.Any(m => m.Id == movieId);
            if (!movieExists)
            {
                return NotFound("Movie does not exist.");
            }

            var ratings = _context.UserRatings
                .Where(r => r.MovieId == movieId)
                .ToList();

            if (!ratings.Any())
            {
                return Ok(new { MovieId = movieId, AverageRating = 0.0, RatingsCount = 0 });
            }

            var average = ratings.Average(r => r.Score);

            return Ok(new
            {
                MovieId = movieId,
                AverageRating = average,
                RatingsCount = ratings.Count
            });
        }

        [HttpPost]
        public IActionResult AddRating([FromBody] CreateRatingDto dto)
        {
            if (dto.Score < 1 || dto.Score > 5)
            {
                return BadRequest("Score must be between 1 and 5.");
            }

            var movieExists = _context.Movies.Any(m => m.Id == dto.MovieId);
            if (!movieExists)
            {
                return BadRequest("Movie does not exist.");
            }

            var existingRating = _context.UserRatings
                .FirstOrDefault(r => r.UserId == dto.UserId && r.MovieId == dto.MovieId);

            if (existingRating != null)
            {
                existingRating.Score = dto.Score;
                _context.SaveChanges();
                return Ok(existingRating);
            }

            var rating = new UserRating
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId,
                Score = dto.Score
            };

            _context.UserRatings.Add(rating);
            _context.SaveChanges();

            return Ok(rating);
        }
    }
}