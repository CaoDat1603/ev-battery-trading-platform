using Complaints.Application.DTOs;
using Complaints.Domain.Enums;

namespace Complaints.Application.Mappers
{
    public static class ComplaintStatusMapper
    {
        public static ComplaintStatus ToDomain(this ComplaintStatusDto dto)
        {
            return dto switch
            {
                ComplaintStatusDto.Pending => ComplaintStatus.Pending,
                ComplaintStatusDto.InReview => ComplaintStatus.InReview,
                ComplaintStatusDto.Resolved => ComplaintStatus.Resolved,
                ComplaintStatusDto.Cancelled => ComplaintStatus.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, "Invalid ComplaintStatusDto value")
            };
        }

        public static ComplaintStatusDto ToDto(this ComplaintStatus domain)
        {
            return domain switch
            {
                ComplaintStatus.Pending => ComplaintStatusDto.Pending,
                ComplaintStatus.InReview => ComplaintStatusDto.InReview,
                ComplaintStatus.Resolved => ComplaintStatusDto.Resolved,
                ComplaintStatus.Cancelled => ComplaintStatusDto.Cancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, "Invalid ComplaintStatus value")
            };
        }

        public static ComplaintStatus? ToDomainNullable(this ComplaintStatusDto? dto)
            => dto.HasValue ? dto.Value.ToDomain() : null;

        public static ComplaintStatusDto? ToDtoNullable(this ComplaintStatus? domain)
            => domain.HasValue ? domain.Value.ToDto() : null;
    }
}
