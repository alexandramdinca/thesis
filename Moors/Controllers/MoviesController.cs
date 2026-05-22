using Microsoft.AspNetCore.Mvc;
using Moors.Data;
using Moors.Models;

namespace Moors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetMovies()
        {
            var movies = _context.Movies.ToList();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpGet("search")]
        public IActionResult SearchMovies([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Ok(new List<Movie>());

            var normalizedTitle = title.Trim().ToLower();

            var movies = _context.Movies
                .Where(m => m.Title.ToLower().Contains(normalizedTitle))
                .ToList();

            return Ok(movies);
        }
    }
}