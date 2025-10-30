﻿using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Domain.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; private set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; private set; } = default!;

        [Required]
        public decimal Price { get; private set; }

        [Required]
        public int SellerId { get; private set; } // ID of the seller

        public ProductStatus StatusProduct { get; private set; }

        [Required]
        public int Quantity { get; private set; } // Quantity available for sale

        public int Sold { get; private set; } // Quantity already sold

        public DateTimeOffset? ModerationAt { get; private set; } // Time when the product was reviewed

        public int? ModeratedBy { get; private set; } // ID of the admin who approved or moderated the product

        [Required]
        [MaxLength(300)]
        public string PickupAddress { get; private set; } = default!; // Pickup location for the product

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? DeletedAt { get; private set; }

        private readonly List<ProductDetail> _details = new();
        public IReadOnlyList<ProductDetail> Details => _details;

        // Private constructor for EF Core
        private Product() { }

        /// <summary>
        /// Factory method for creating a new Product entity.
        /// </summary>
        public static Product Create(string title, decimal price, int sellerId, string pickupAddress, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price));
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            return new Product
            {
                Title = title,
                Price = price,
                SellerId = sellerId,
                PickupAddress = pickupAddress,
                Quantity = quantity,
                StatusProduct = ProductStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Marks the product as approved by an admin.
        /// </summary>
        public void Approve(int adminId)
        {
            StatusProduct = ProductStatus.Available;
            ModeratedBy = adminId;
            ModerationAt = DateTimeOffset.UtcNow;
            UpdatedAt = ModerationAt;
        }

        /// <summary>
        /// Adds detailed information for the product (such as images, files, description, etc.).
        /// </summary>
        public ProductDetail AddDetail(
            string productName,
            string description,
            int productType,
            string registrationCard,
            string fileUrl,
            string imageUrl,
            string? status = "Listed")
        {
            var detail = new ProductDetail(ProductId, productName, description, productType, registrationCard, fileUrl, imageUrl, status);
            _details.Add(detail);
            return detail;
        }

        /// <summary>
        /// Changes the current status of the product.
        /// </summary>
        public void ChangeStatus(ProductStatus newStatus)
        {
            if (!Enum.IsDefined(typeof(ProductStatus), newStatus))
                throw new ArgumentException($"Invalid status value: '{newStatus}'");

            if (StatusProduct == newStatus)
                return;

            StatusProduct = newStatus;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Soft deletes the product (marks it as deleted without removing it from the database).
        /// </summary>
        public void Delete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DeletedAt;
        }
    }
}
