using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    public class OrderItemData
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ItemId { get; set; }
        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
    public class OrderItem
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
    public class Order
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        [BsonElement("tableno")]
        public string TableNo { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("datetime")]
        public string DateTime { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
        [BsonElement("notes")]
        public string? Notes { get; set; } = "";
        [BsonElement("itemids")]
        [JsonIgnore]
        public List<OrderItemData> ItemData { get; set; } = new List<OrderItemData>();
        [BsonIgnore]
        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
