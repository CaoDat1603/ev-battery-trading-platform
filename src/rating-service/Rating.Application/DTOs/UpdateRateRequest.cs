using System.ComponentModel.DataAnnotations;

namespace Rating.Application.DTOs
{
    public class UpdateRateRequest
    {
        [Range(1, 10)]
        public int? Score { get; set; }
        public string? Comment { get; set; }
    }
}
