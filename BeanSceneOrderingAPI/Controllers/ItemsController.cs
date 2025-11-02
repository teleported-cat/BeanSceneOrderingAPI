using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BeanSceneOrderingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        public ItemsController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all menu items in the database.
        /// </summary>
        /// <returns>Ok(collection) or NotFound()</returns>
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
        /// HTTP GET method which returns a menu item that matches the given id.
        /// </summary>
        /// <param name="id">Id of a menu item.</param>
        /// <returns>Ok(item) or NotFound()</returns>
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Item>("Items").AsQueryable();
            var item = collection.First(i => i.Id == id);
            return item == null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// HTTP POST method which inserts a new item into the menu.
        /// </summary>
        /// <param name="item">Item to be inserted.</param>
        /// <returns>CreatedAtAction() or BadRequest()</returns>
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
        /// <param name="item">Item to be updated. The id must match one in the collection, the other fields are the new values.</param>
        /// <returns>NotFound(), Ok() or StatusCode(500)</returns>
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
        /// HTTP method which deletes an item from the collection.
        /// </summary>
        /// <param name="id">The id of the item to be deleted.</param>
        /// <returns>Ok() or NotFound()</returns>
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
