using BeanSceneOrderingAPI.Models;
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

        [HttpGet]
        public IActionResult Get()
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Item>("Items").AsQueryable();
            return collection == null ? NotFound() : Ok(collection);
        }
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Item>("Items").AsQueryable();
            var item = collection.First(i => i.Id == id);
            return item == null ? NotFound() : Ok(item);
        }
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
        [HttpDelete]
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
