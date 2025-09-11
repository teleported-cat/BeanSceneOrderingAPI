using MongoDB.Bson.Serialization.Attributes;

namespace BeanSceneOrderingAPI.Models
{
    public class Category
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        public string? _id { get; set; }
        public string name { get; set; }
    }
}
