namespace ColdlineWeb.Models
{
    public class PagedResult<T>
    {
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
