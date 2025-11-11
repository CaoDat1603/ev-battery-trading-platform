using System.ComponentModel.DataAnnotations;

namespace Wishlist.Application.DTOs
{
    public class WishlistResponse
    {
        [Required] public int WishlistId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public int ProductId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
