using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Services
{
    public class ProductCommands : IProductCommands
    {
        private readonly IProductRepository _repo;
        private readonly IUnitOfWork _uow;
        public ProductCommands(IProductRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

        public async Task<int> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
        {

            var product = Product.Create(dto.Title, dto.Price, dto.SellerId, dto.PickupAddress, 1);
            product.Approve(adminId: 1);
            await _repo.AddAsync(product, ct);
            await _uow.SaveChangesAsync(ct);
            product.AddDetail(dto.ProductName, dto.Description, productType: dto.ProductType, registrationCard: dto.RegistrationCard, fileUrl: dto.FileUrl, imageUrl: dto.ImageUrl);
            await _uow.SaveChangesAsync(ct);
            return product.ProductId;
        }

        public async Task<bool> UpdateStatusAsync(int productId, ProductStatus newStatus, CancellationToken ct = default)
        {
            var product = await _repo.GetByIdAsync(productId, ct);
            if (product == null)
                return false;
            Console.WriteLine($"Before: {product.StatusProduct}");
            product.ChangeStatus(newStatus);
            _repo.Update(product);
            await _uow.SaveChangesAsync(ct);
            Console.WriteLine($"After: {product.StatusProduct}");
            return true;
        }
    }
}
