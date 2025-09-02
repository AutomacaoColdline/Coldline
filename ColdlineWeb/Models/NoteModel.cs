using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models
{
    public class NoteModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Element { get; set; } = new();
        public NoteType NoteType { get; set; }
    }

}
