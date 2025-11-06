using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BeanSceneOrderingAPI.Controllers
{
    /// <summary>
    /// Controller for API endpoints related to the menu category collection in the database.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        /// <summary>
        /// Constructor for the Category Controller.
        /// </summary>
        /// <param name="databaseSettings">The settings the database, including name & connection string</param>
        public CategoryController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all menu categories in the database.
        /// </summary>
        /// <returns>OK (200) with list of categories or Not Found (404) if the collection isn't found.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Category>("Categories").AsQueryable();
            return collection == null ? NotFound() : Ok(collection);
        }

        /// <summary>
        /// HTTP POST method which inserts a new category into the menu.
        /// </summary>
        /// <param name="item">Category data to be inserted</param>
        /// <returns>Created At Action (201) if successful, or Bad Request (400) if category data is invalid.</returns>
        [Authorize(Roles = "Manager")]
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
        /// HTTP DELETE method which deletes a category from the collection.
        /// </summary>
        /// <param name="id">Id of the category to be deleted</param>
        /// <returns>OK (200) if a category is deleted or not.</returns>
        [Authorize(Roles = "Manager")]
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
