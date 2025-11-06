using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    /// <summary>
    /// Subclass used to represent the ID and quantity of each item in an order.
    /// </summary>
    public class OrderItemData
    {
        /// <summary>
        /// The ID of the item.
        /// </summary>
        [BsonElement("_id")]
        [JsonPropertyName("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ItemId { get; set; }
        /// <summary>
        /// The quantity of the item.
        /// </summary>
        [BsonElement("quantity")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
    /// <summary>
    /// Model for representing the order collection.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// An object ID, used as a primary key.
        /// </summary>
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        /// <summary>
        /// The code for table the order is for.
        /// </summary>
        [BsonElement("tableno")]
        [JsonPropertyName("tableno")]
        public string TableNo { get; set; }
        /// <summary>
        /// The name of the customer who ordered.
        /// </summary>
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// The time and date when the order was place.
        /// </summary>
        [BsonElement("datetime")]
        [JsonPropertyName("datetime")]
        public string DateTime { get; set; }
        /// <summary>
        /// Current status of the order.
        /// Can only be 'Pending', 'In Progress', 'Completed', or 'Cancelled'.
        /// </summary>
        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        /// Any additional notes for the kitchen staff about the order.
        /// </summary>
        [BsonElement("notes")]
        [JsonPropertyName("notes")]
        public string? Notes { get; set; } = "";
        /// <summary>
        /// The list of items ordered and their quantities.
        /// </summary>
        [BsonElement("itemids")]
        [JsonPropertyName("itemids")]
        public List<OrderItemData> ItemData { get; set; } = new List<OrderItemData>();
    }
}
