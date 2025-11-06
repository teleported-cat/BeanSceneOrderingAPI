using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    /// <summary>
    /// Model for representing the item collection.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// An object ID, used as a primary key.
        /// </summary>
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        /// <summary>
        /// The name of the item.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; }
        /// <summary>
        /// The description of the item.
        /// </summary>
        [BsonElement("description")]
        public string Description { get; set; }
        /// <summary>
        /// The image path of the item's image. File type required at the end.
        /// </summary>
        [BsonElement("imagepath")]
        public string? ImagePath { get; set; } = "";
        /// <summary>
        /// The price of the item in dollars.
        /// </summary>
        [BsonElement("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// If the item is available to order or not.
        /// </summary>
        [BsonElement("available")]
        public bool? Available { get; set; } = false;
        /// <summary>
        /// If the item is gluten-free or not.
        /// </summary>
        [BsonElement("glutenfree")]
        public bool? GlutenFree { get; set; } = false;
        /// <summary>
        /// Whether the item is vegan, vegetarian, or contains meat.
        /// Can only be 'vegan', 'vegetarian', or 'neither'.
        /// </summary>
        [BsonElement("diettype")]
        public string? DietType { get; set; } = "neither";
        /// <summary>
        /// The allergens present within the item.
        /// </summary>
        [BsonElement("allergens")]
        public string? Allergens { get; set; } = "";
        /// <summary>
        /// The name of the category the item is in.
        /// </summary>
        [BsonElement("categoryname")]
        public string CategoryName { get; set; }

    }
}
