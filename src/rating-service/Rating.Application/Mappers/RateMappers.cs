using Rating.Application.DTOs;
using Rating.Domain.Entities;

namespace Rating.Application.Mappers
{
    public static class RateMappers
    {
        public static RateResponse ToDto(this Rate e)
        {
            return new RateResponse
            {
                RateId = e.RateId,
                FeedbackId = e.FeedBackId,
                UserId = e.UserId,
                ProductId = e.ProductId,
                RateBy = e.RateBy,
                Score = e.Score,
                Comment = e.Comment,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Images = e.Images?.Select(i => i.ToDto()).ToList() ?? new List<RateImageDto>()
            };
        }

        public static RateImageDto ToDto(this RateImage i)
        {
            return new RateImageDto
            {
                RateImageId = i.RateImageId,
                ImageUrl = i.ImageUrl,
                CreatedAt = i.CreatedAt
            };
        }
    }
}
