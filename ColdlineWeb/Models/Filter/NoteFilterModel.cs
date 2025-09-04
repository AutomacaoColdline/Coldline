using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models.Filter
{
    public class NoteFilterModel
    {
        public string? Name { get; set; }
        public string? Element { get; set; }
        public NoteType? NoteType { get; set; }

         // paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // ordenação (opcional, combine com o backend)
        public string? SortBy { get; set; }       // "name" | "notetype"
        public bool SortDesc { get; set; } = true;

    }
}
