namespace Rating.Application.DTOs
{
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

}
