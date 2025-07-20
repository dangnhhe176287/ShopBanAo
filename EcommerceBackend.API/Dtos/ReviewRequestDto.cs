namespace EcommerceBackend.API.Dtos
{
    public class ReviewRequestDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
    }
} 