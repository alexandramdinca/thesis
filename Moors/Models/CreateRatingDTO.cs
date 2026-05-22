namespace Moors.Models
{
    public class CreateRatingDto
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public int Score { get; set; }
    }
}