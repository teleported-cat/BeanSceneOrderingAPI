using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BeanSceneOrderingAPI.Controllers
{
    /// <summary>
    /// Controller for API endpoints related to the menu item collection in the database.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        /// <summary>
        /// Constructor for the Items Controller.
        /// </summary>
        /// <param name="databaseSettings">The settings the database, including name and connection string</param>
        public ItemsController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all menu items in the database.
        /// </summary>
        /// <returns>OK (200) with list of items, or Not Found (404) if the collection isn't found.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Item>("Items").AsQueryable();
            if (collection == null) { return NotFound(); }
            var ordered = collection.OrderBy(i => i.CategoryName).ThenBy(i => i.Name);
            return Ok(ordered);
        }

        /// <summary>
        /// HTTP POST method which inserts a new item into the menu.
        /// </summary>
        /// <param name="item">Item data to be inserted</param>
        /// <returns>Created At Action (201) if successful, or Bad Request (400) if item data is invalid.</returns>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Item item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await client.GetDatabase(databaseName).GetCollection<Item>("Items").InsertOneAsync(item);

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        /// <summary>
        /// HTTP PUT method which updates an existing item.
        /// </summary>
        /// <param name="item">Item to be updated</param>
        /// <returns>OK (200) if successful, Not Found (404) if item isn't found, or Server Error (500) if an exception occurs.</returns>
        [Authorize(Roles = "Manager")]
        [HttpPut]
        public async Task<IActionResult> Put(Item item)
        {
            try
            {
                var filter = Builders<Item>.Filter.Eq("_id", ObjectId.Parse(item.Id));

                if (filter == null)
                {
                    return NotFound(0);
                }

                var update = Builders<Item>.Update
                    .Set("name", item.Name)
                    .Set("description", item.Description)
                    .Set("imagepath", item.ImagePath)
                    .Set("price", item.Price)
                    .Set("available", item.Available)
                    .Set("glutenfree", item.GlutenFree)
                    .Set("diettype", item.DietType)
                    .Set("allergens", item.Allergens)
                    .Set("categoryname", item.CategoryName)
                    ;

                var result = await client.GetDatabase(databaseName)
                    .GetCollection<Item>("Items")
                    .UpdateOneAsync(filter, update);

                if (result.MatchedCount ==  0) {
                    return NotFound("Item not found");
                }

                if (result.ModifiedCount == 0) {
                    return Ok("Item found but no changes were made");
                }

                return Ok("Item updated");
            } catch (Exception e) { 
                return StatusCode(500, $"Error updating item: {e.Message}");
            }
        }

        /// <summary>
        /// HTTP DELETE method which deletes an item from the collection.
        /// </summary>
        /// <param name="id">Id of the item to be deleted</param>
        /// <returns>OK (200) if an item is deleted or not.</returns>
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var filter = Builders<Item>.Filter.Eq("_id", ObjectId.Parse(id));

            if (filter == null)
            {
                return NotFound(0);
            }

            var result = await client.GetDatabase(databaseName)
                .GetCollection<Item>("Items")
                .DeleteOneAsync(filter);

            return Ok("Item deleted");
        }
    }
}
