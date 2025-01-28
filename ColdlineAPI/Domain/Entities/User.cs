using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        public string UserType { get; set; } = string.Empty;

        [BsonElement("department")]
        public string Department { get; set; } = string.Empty;

        [BsonElement("identificationNumber")]
        public string IdentificationNumber { get; set; } = string.Empty;
    }
}
