using System.ComponentModel.DataAnnotations;

namespace Wishlist.Application.DTOs
{
    public class CreateWishlistDto
    {
        [Required] public int WishlistId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public int ProductId { get; set; }
    }
}
