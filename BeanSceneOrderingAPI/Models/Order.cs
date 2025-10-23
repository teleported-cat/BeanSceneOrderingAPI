using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    public class OrderItemData
    {
        [BsonElement("_id")]
        [JsonPropertyName("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ItemId { get; set; }
        [BsonElement("quantity")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
    public class Order
    {
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        [BsonElement("tableno")]
        [JsonPropertyName("tableno")]
        public string TableNo { get; set; }
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [BsonElement("datetime")]
        [JsonPropertyName("datetime")]
        public string DateTime { get; set; }
        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [BsonElement("notes")]
        [JsonPropertyName("notes")]
        public string? Notes { get; set; } = "";
        [BsonElement("itemids")]
        [JsonPropertyName("itemids")]
        public List<OrderItemData> ItemData { get; set; } = new List<OrderItemData>();
    }
}
