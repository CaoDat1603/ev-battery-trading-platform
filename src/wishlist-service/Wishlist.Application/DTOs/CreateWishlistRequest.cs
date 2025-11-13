using System.ComponentModel.DataAnnotations;

namespace Wishlist.Application.DTOs
{
    public class CreateWishlistRequest
    {
        //[Required] public int UserId { get; set; }
        [Required] public int ProductId { get; set; }
    }
}
