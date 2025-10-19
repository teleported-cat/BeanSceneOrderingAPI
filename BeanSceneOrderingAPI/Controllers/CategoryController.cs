using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BeanSceneOrderingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        public CategoryController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all menu categories in the database.
        /// </summary>
        /// <returns>Ok(collection) or NotFound()</returns>
        [HttpGet]
        public IActionResult Get()
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Category>("Categories").AsQueryable();
            return collection == null ? NotFound() : Ok(collection);
        }

        /// <summary>
        /// HTTP POST method which inserts a new category into the menu.
        /// </summary>
        /// <param name="item">Category to be inserted.</param>
        /// <returns>CreatedAtAction() or BadRequest()</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await client.GetDatabase(databaseName).GetCollection<Category>("Categories").InsertOneAsync(category);

            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        /// <summary>
        /// HTTP PUT method which updates an existing category.
        /// </summary>
        /// <param name="item">Category to be updated. The id must match one in the collection.</param>
        /// <returns>NotFound(), Ok() or StatusCode(500)</returns>
        [HttpPut]
        public async Task<IActionResult> Put(Category category)
        {
            try
            {
                var filter = Builders<Category>.Filter.Eq("_id", ObjectId.Parse(category.Id));

                if (filter == null)
                {
                    return NotFound(0);
                }

                var update = Builders<Category>.Update
                    .Set("name", category.Name);

                var result = await client.GetDatabase(databaseName)
                    .GetCollection<Category>("Categories")
                    .UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound("Category not found");
                }

                if (result.ModifiedCount == 0)
                {
                    return Ok("Category found but no changes were made");
                }

                return Ok("Category updated");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error updating category: {e.Message}");
            }
        }

        /// <summary>
        /// HTTP method which deletes a category from the collection.
        /// </summary>
        /// <param name="id">The id of the category to be deleted.</param>
        /// <returns>Ok() or NotFound()</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var filter = Builders<Category>.Filter.Eq("_id", ObjectId.Parse(id));

            if (filter == null)
            {
                return NotFound(0);
            }

            var result = await client.GetDatabase(databaseName)
                .GetCollection<Category>("Categories")
                .DeleteOneAsync(filter);

            return Ok("Category deleted");
        }
    }
}
