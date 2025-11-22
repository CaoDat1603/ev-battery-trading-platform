using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Abstractions;
using Catalog.Application.Contracts;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Application.Abstractions;
using OpenQA.Selenium;

namespace Catalog.Application.Services
{
    public class ProductCommands : IProductCommands
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IProductFileHandler _fileHandler;
        private readonly IProductImageHandler _imageHandler;
        private readonly IOrderClient _orderClient;

        public ProductCommands(
            IProductRepository repo,
            IUnitOfWork uow,
            IProductFileHandler fileHandler,
            IProductImageHandler imageHandler,
            IOrderClient orderClient)
        {
            _repo = repo;
            _uow = uow;
            _fileHandler = fileHandler;
            _imageHandler = imageHandler;
            _orderClient = orderClient;
        }

        public async Task<int> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Create the basic domain entity
            var product = Product.Create(dto.Title, dto.Price, dto.SellerId, dto.PickupAddress, quantity: 1, dto.saleMethod, isSpam: dto.IsSpam);
            //product.Approve(adminId: 1);

            // Save the main entity
            await _repo.AddAsync(product, ct);
            await _uow.SaveChangesAsync(ct);

            // Handle file upload
            string? fileUrl = null;
            if (dto.FileUrl != null)
                fileUrl = await _fileHandler.SaveDocumentAsync(dto.FileUrl, product.ProductId, ct);

            // Handle image upload
            string? imageUrl = null;
            if (dto.ImageUrl != null)
                imageUrl = await _imageHandler.SaveImageAsync(dto.ImageUrl, product.ProductId, ct);

            // Attach product detail (including file and image URLs)
            product.AddDetail(
                dto.ProductName,
                dto.Description,
                dto.ProductType,
                dto.RegistrationCard,
                fileUrl,
                imageUrl
            );

            // Update the entity with its details
            await _repo.UpdateAsync(product, ct);
            await _uow.SaveChangesAsync(ct);

            return product.ProductId;
        }


        public async Task<bool> UpdateStatusAsync(int productId, ProductStatus newStatus, CancellationToken ct = default)
        {
            var product = await _repo.GetByIdAsync(productId, ct);
            if (product == null) return false;

            product.ChangeStatus(newStatus);
            await _repo.UpdateAsync(product, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> UpdateSaleMethodAsync(int productId, SaleMethod newMethod, CancellationToken ct = default)
        {
            var product = await _repo.GetByIdAsync(productId, ct);
            if (product == null) return false;

            product.ChangesSaleMethod(newMethod);
            await _repo.UpdateAsync(product, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> MarkAsVerifiedAsync(int productId, CancellationToken ct = default)
        {
            await _repo.MarkAsVerified(productId, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }
        public async Task<bool> UnmarkAsVerifiedAsync(int productId, CancellationToken ct = default)
        {
            await _repo.UnmarkAsVerified(productId, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }
        public async Task<bool> MarkAsSpamAsync(int productId, CancellationToken ct = default)
        {
            await _repo.MarkAsSpam(productId, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }
        public async Task<bool> UnmarkAsSpamAsync(int productId, CancellationToken ct = default)
        {
            await _repo.UnmarkAsSpam(productId, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId, CancellationToken ct = default)
        {
            if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));

            var product = await _repo.GetByIdAsync(productId, ct);
            if (product == null) return false;

            // Perform soft delete
            await _repo.SoftDeleteAsync(productId, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> VerifyAndCompleteTransaction(
            int transactionId,
            int productId,
            CancellationToken ct)
        {
            var product = await _repo.GetByIdAsync(productId, ct);
            if (product == null)
                throw new NotFoundException("Product not found");

            var ok = await _orderClient.IsTransactionCompleted(
                transactionId,
                product,
                product.StatusProduct,
                ct
            );

            if (!ok)
                return false;

            // Update domain
            product.ChangeStatus(ProductStatus.SoldOut);
            await _repo.UpdateAsync(product, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
