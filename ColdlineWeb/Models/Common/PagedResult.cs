namespace ColdlineWeb.Models.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
