using System.ComponentModel.DataAnnotations;

namespace Catalog.Domain.Entities
{
    public class ProductDetail
    {
        [Key]
        public int ProductDetailId { get; private set; }

        [Required]
        public int ProductId { get; private set; }

        [Required]
        public int ProductType { get; private set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; private set; } = default!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; private set; } = default!;

        [Required]
        [MaxLength(100)]
        public string RegistrationCard { get; private set; } // Product registration or certification code

        [Required]
        [MaxLength(300)]
        public string FileUrl { get; private set; } // URL of the product's detailed document

        [Required]
        [MaxLength(300)]
        public string ImageUrl { get; private set; } // URL of the product's image

        [MaxLength(50)]
        public string? Status { get; private set; } // Product detail status (e.g., "Listed", "Delisted", "Sold", "Removed")

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }

        // Private constructor for EF Core
        private ProductDetail() { }

        /// <summary>
        /// Constructor for creating a new ProductDetail entity.
        /// </summary>
        /// <param name="productId">The ID of the associated product.</param>
        /// <param name="productName">The name of the product detail.</param>
        /// <param name="description">Description of the product.</param>
        /// <param name="productType">Product type identifier.</param>
        /// <param name="registrationCard">Registration or certification number.</param>
        /// <param name="fileUrl">URL of the detailed document file.</param>
        /// <param name="imageUrl">URL of the product image.</param>
        /// <param name="status">Status of the product detail (default: "Listed").</param>
        public ProductDetail(
            int productId,
            string productName,
            string description,
            int productType,
            string registrationCard,
            string fileUrl,
            string imageUrl,
            string? status = "Listed")
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name is required.", nameof(productName));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL is required.", nameof(fileUrl));
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL is required.", nameof(imageUrl));
            if (productId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productId));
            if (productType <= 0)
                throw new ArgumentOutOfRangeException(nameof(productType));

            ProductId = productId;
            ProductName = productName;
            Description = description;
            ProductType = productType;
            RegistrationCard = registrationCard;
            FileUrl = fileUrl;
            ImageUrl = imageUrl;
            Status = status;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Updates the product detail with new values (if provided).
        /// </summary>
        public void Update(string? productName, string? description, string? imageUrl, string? fileUrl, string? status)
        {
            if (!string.IsNullOrWhiteSpace(productName))
                ProductName = productName;
            if (!string.IsNullOrWhiteSpace(description))
                Description = description;
            if (!string.IsNullOrWhiteSpace(imageUrl))
                ImageUrl = imageUrl;
            if (!string.IsNullOrWhiteSpace(fileUrl))
                FileUrl = fileUrl;
            if (!string.IsNullOrWhiteSpace(status))
                Status = status;

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Sets the product detail's status (e.g., "Listed", "Sold").
        /// </summary>
        public void SetStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty.", nameof(status));

            Status = status;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
