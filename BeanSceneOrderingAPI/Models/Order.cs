using MongoDB.Bson.Serialization.Attributes;

namespace BeanSceneOrderingAPI.Models
{
    public class ItemIds
    {
        public string _id { get; set; }
        public string quantity { get; set; }
    }
    public class Order
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        public string? _id { get; set; }
        public string tableno { get; set; }
        public string name { get; set; }
        public string datetime { get; set; }
        public string status { get; set; }
        public List<ItemIds> itemsids { get; set; } = new List<ItemIds>();
    }
}
