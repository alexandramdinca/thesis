namespace Moors.Models
{
    public class WatchlistEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
    }
}