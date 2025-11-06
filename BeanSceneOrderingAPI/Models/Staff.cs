using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BeanSceneOrderingAPI.Models
{
    /// <summary>
    /// Model for representing the staff collection.
    /// </summary>
    public class Staff
    {
        /// <summary>
        /// The ID of the item.
        /// </summary>
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        /// <summary>
        /// First name of the staff member.
        /// </summary>
        [BsonElement("firstname")]
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of staff member.
        /// </summary>
        [BsonElement("lastname")]
        public string LastName { get; set; }
        /// <summary>
        /// Username of the staff member.
        /// </summary>
        [BsonElement("username")]
        public string Username { get; set; }
        /// <summary>
        /// Email of the staff member.
        /// </summary>
        [BsonElement("email")]
        public string Email { get; set; }
        /// <summary>
        /// Password hash of the staff member.
        /// Salt and hashed using BCrypt.
        /// </summary>
        [BsonElement("passwordhash")]
        public string PasswordHash { get; set; }
        /// <summary>
        /// Role of the staff member.
        /// Can only be 'Staff' or 'Manager'.
        /// </summary>
        [BsonElement("role")]
        public string Role { get; set; }
    }
    /// <summary>
    /// Data transfer object for staff which excludes the password hash.
    /// </summary>
    public class StaffDto
    {
        /// <summary>
        /// The ID of the item.
        /// </summary>
        [BsonId] // Primary Key
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // Pass the object id as string
        [JsonPropertyName("_id")]
        public string? Id { get; set; }
        /// <summary>
        /// First name of the staff member.
        /// </summary>
        [BsonElement("firstname")]
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of staff member.
        /// </summary>
        [BsonElement("lastname")]
        public string LastName { get; set; }
        /// <summary>
        /// Username of the staff member.
        /// </summary>
        [BsonElement("username")]
        public string Username { get; set; }
        /// <summary>
        /// Email of the staff member.
        /// </summary>
        [BsonElement("email")]
        public string Email { get; set; }
        /// <summary>
        /// Role of the staff member.
        /// Can only be 'Staff' or 'Manager'.
        /// </summary>
        [BsonElement("role")]
        public string Role { get; set; }
    }
}
