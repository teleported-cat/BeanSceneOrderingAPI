using MongoDB.Bson.Serialization.Attributes;

namespace BeanSceneOrderingAPI.Models
{
    public class Item
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        public string? Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("imagepath")]
        public string? ImagePath { get; set; }
        [BsonElement("price")]
        public decimal Price { get; set; }
        [BsonElement("available")]
        public bool? Available { get; set; } = false;
        [BsonElement("glutenfree")]
        public bool? GlutenFree { get; set; } = false;
        [BsonElement("diettype")]
        public string? DietType { get; set; }
        [BsonElement("allergens")]
        public string? Allergens { get; set; }
        [BsonElement("categoryname")]
        public string CategoryName { get; set; }

    }
}
