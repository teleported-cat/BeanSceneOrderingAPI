using MongoDB.Bson.Serialization.Attributes;

namespace BeanSceneOrderingAPI.Models
{
    public class Staff
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        public string? _id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string passwordhash { get; set; }
        public string role { get; set; }
    }
}
