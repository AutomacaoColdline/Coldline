namespace ColdlineAPI.Application.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public long Total { get; set; }                  // total de itens no filtro
        public int Page { get; set; }                    // página atual (1-based)
        public int PageSize { get; set; }                // itens por página
        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
