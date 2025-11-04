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
        /// HTTP GET method which returns all orders in the database ordered by its time & date.
        /// </summary>
        /// <returns>OK (200) with list of orders, or Not Found (404) if the collection isn't found.</returns>
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
        /// HTTP POST method which inserts a new order into the database.
        /// </summary>
        /// <param name="order">The order data being inserted.</param>
        /// <returns>Created At Action (201) if successful, or Bad Request (400) if order data is invalid.</returns>
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

        /// <summary>
        /// HTTP PUT method which updates the status of an order in the system.
        /// </summary>
        /// <param name="id">Id of the order</param>
        /// <param name="newStatus">The new status of the order</param>
        /// <returns>
        /// OK (200) if updated successfully, 
        /// Not Found (404) if order isn't found, 
        /// or Server Error (500) if an exception occurs.
        /// </returns>
        [Authorize]
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string newStatus)
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

        /// <summary>
        /// HTTP GET method which returns all items in an order, including their quantities.
        /// </summary>
        /// <param name="id">Id of the order</param>
        /// <returns>OK (200) with an list of items or Not Found (404) if order isn't found.</returns>
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
