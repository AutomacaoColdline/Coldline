using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Enum;

namespace ColdlineAPI.Domain.Entities
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("element")]
        public List<string> Element { get; set; } = new();

        [BsonElement("noteType")]
        public NoteType NoteType { get; set; }

    }
}
