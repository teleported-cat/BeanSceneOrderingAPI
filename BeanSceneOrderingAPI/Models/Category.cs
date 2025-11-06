using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    /// <summary>
    /// Model for representing the category collection.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// An object ID, used as a primary key.
        /// </summary>
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        /// <summary>
        /// Name of the category. Items refer to their category using the name.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; }
    }
}
