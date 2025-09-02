using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models.Filter
{
    public class NoteFilterModel
    {
        public string? Name { get; set; }
        public string? Element { get; set; }
        public NoteType? NoteType { get; set; }

    }
}
