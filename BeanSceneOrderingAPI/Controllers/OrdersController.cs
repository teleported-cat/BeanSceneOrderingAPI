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
    public class OrdersController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        public OrdersController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all orders in the database.
        /// </summary>
        /// <returns>Ok(collection) or NotFound()</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var orders = client.GetDatabase(databaseName).GetCollection<Order>("Orders").AsQueryable();
            if (orders == null) { return NotFound(); };
            var ordered = orders.OrderByDescending(o => o.DateTime);
            return Ok(ordered);
        }

        /// <summary>
        /// HTTP GET method which returns an order based on its id.
        /// </summary>
        /// <param name="id">Id of the order.</param>
        /// <returns>Ok(order) or NotFound()</returns>
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return BadRequest();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await client.GetDatabase(databaseName)
                .GetCollection<Order>("Orders")
                .InsertOneAsync(order);

            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        [Authorize]
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> Put(string id, [FromBody] string newStatus)
        {
            try 
            { 
                var filter = Builders<Order>.Filter.Eq("_id", ObjectId.Parse(id));

                if (filter == null)
                {
                    return NotFound(0);
                }

                var update = Builders<Order>.Update.Set("status", newStatus);

                var result = await client.GetDatabase(databaseName)
                    .GetCollection<Order>("Orders")
                    .UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound("Order not found");
                }

                if (result.ModifiedCount == 0)
                {
                    return Ok("Order found but no changes were made");
                }

                return Ok("Order updated");
            } catch (Exception e)
            {
                return StatusCode(500, $"Error updating order: {e.Message}");
            }
        }

        [Authorize]
        [HttpGet("{id}/Items")]
        public async Task<IActionResult> GetItems(string id)
        {
            // Get order
            var orders = client.GetDatabase(databaseName).GetCollection<Order>("Orders");
            var order = await orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null) { return NotFound(); }

            // Get items
            var itemCollection = client.GetDatabase(databaseName).GetCollection<BsonDocument>("Items");
            var itemsInOrder = new List<object>();

            var jsonSettings = new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson };

            // Check if each item in order is in collection
            foreach (var oid in order.ItemData)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(oid.ItemId));
                var current = await itemCollection.Find(filter).FirstOrDefaultAsync();

                if (current != null)
                {
                    current["quantity"] = oid.Quantity;
                    if (current["_id"].IsObjectId)
                    {
                        current["_id"] = current["_id"].AsObjectId.ToString();
                    }
                    if (current["price"].IsDecimal128)
                    {
                        current["price"] = current["price"].AsDecimal128.ToString();
                    }
                    var json = current.ToJson(jsonSettings);
                    itemsInOrder.Add(System.Text.Json.JsonSerializer.Deserialize<object>(json));
                } else
                {
                    var invalidItem = new
                    {
                        invalid = true,
                        _id = oid.ItemId,
                        quantity = oid.Quantity
                    };

                    itemsInOrder.Add(invalidItem);
                }
            }

            return Ok(itemsInOrder);
        }
    }
}
