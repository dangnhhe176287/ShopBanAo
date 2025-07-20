namespace EcommerceBackend.API.Dtos
{
    public class RatingRequestDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }
    }
} 