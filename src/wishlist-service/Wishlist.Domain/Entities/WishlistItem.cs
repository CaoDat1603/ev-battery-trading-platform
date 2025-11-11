using System.ComponentModel.DataAnnotations;

namespace Wishlist.Domain.Entities
{
    public class WishlistItem
    {
        [Key]
        public int WishlistId { get; private set; }

        [Required]
        public int UserId { get; private set; }

        [Required]
        public int ProductId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }


        // Private constructor for EF Core
        private WishlistItem() { }

        /// <summary>
        /// Factory method to create a new WishlistItem
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        public static WishlistItem Create(int userId, int productId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            if (productId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productId));

            return new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Soft delete the wishlist item
        /// </summary>
        public void Delete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}