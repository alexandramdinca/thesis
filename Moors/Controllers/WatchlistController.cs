using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moors.Data;
using Moors.Models;

namespace Moors.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WatchlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserWatchlist(int userId)
        {
            var watchlist = _context.WatchlistEntries
                .Include(w => w.Movie)
                .Where(w => w.UserId == userId)
                .ToList();

            return Ok(watchlist);
        }

        [HttpPost]
        public IActionResult AddToWatchlist([FromBody] CreateWatchlistDto dto)
        {
            var movieExists = _context.Movies.Any(m => m.Id == dto.MovieId);
            if (!movieExists)
            {
                return BadRequest("Movie does not exist.");
            }

            var existingEntry = _context.WatchlistEntries
                .FirstOrDefault(w => w.UserId == dto.UserId && w.MovieId == dto.MovieId);

            if (existingEntry != null)
            {
                return BadRequest("Movie is already in the user's watchlist.");
            }

            var entry = new WatchlistEntry
            {
                UserId = dto.UserId,
                MovieId = dto.MovieId
            };

            _context.WatchlistEntries.Add(entry);
            _context.SaveChanges();

            return Ok(entry);
        }

        [HttpDelete("{userId}/{movieId}")]
        public IActionResult RemoveFromWatchlist(int userId, int movieId)
        {
            var entry = _context.WatchlistEntries
                .FirstOrDefault(w => w.UserId == userId && w.MovieId == movieId);

            if (entry == null)
            {
                return NotFound("Watchlist entry not found.");
            }

            _context.WatchlistEntries.Remove(entry);
            _context.SaveChanges();

            return Ok("Movie removed from watchlist.");
        }
    }
}