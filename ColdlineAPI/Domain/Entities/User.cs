using MongoDB.Bson; 
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("userType")]
        public ReferenceEntity UserType { get; set; } = new ReferenceEntity(); 

        [BsonElement("department")]
        public ReferenceEntity Department { get; set; } = new ReferenceEntity(); 

        [BsonElement("currentProcess")]
        [BsonIgnoreIfNull] // 🔹 Se for NULL, MongoDB ignora esse campo
        public ReferenceEntity? CurrentProcess { get; set; } = null;

        [BsonElement("currentOccurrence")]
        [BsonIgnoreIfNull] // 🔹 Se for NULL, MongoDB ignora esse campo
        public ReferenceEntity? CurrentOccurrence { get; set; } = null;

        [BsonElement("identificationNumber")]
        public string IdentificationNumber { get; set; } = string.Empty;
        [BsonElement("urlPhoto")]
        public string UrlPhoto { get; set; } = string.Empty;
    }
}
