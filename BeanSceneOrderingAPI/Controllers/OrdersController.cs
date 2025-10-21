using BeanSceneOrderingAPI.Models;
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
        [HttpGet]
        public IActionResult Get()
        {
            //var orders = client
            //    .GetDatabase(databaseName)
            //    .GetCollection<Order>("Orders")
            //    .AsQueryable()
            //    .ToList();

            //var allItemIds = orders
            //    .SelectMany(o => o.ItemData.Select(item => item.ItemId))
            //    .Distinct()
            //    .ToList();

            //var items = client
            //    .GetDatabase(databaseName)
            //    .GetCollection<Item>("Items")
            //    .Find(i => allItemIds.Contains(i.Id))
            //    .ToList();

            //var itemLookup = items.ToDictionary(i => i.Id);

            //foreach (var order in orders)
            //{
            //    order.Items = order.ItemData.Select(itemData => new OrderItem
            //    {
            //        Item = itemLookup.TryGetValue(itemData.ItemId, out var item) ? item : null,
            //        Quantity = itemData.Quantity
            //    }).Where(oi => oi.Item != null) // Filter out items that weren't found
            //    .ToList();
            //}

            //return Ok(orders);
            var orders = client.GetDatabase(databaseName).GetCollection<Order>("Orders").AsQueryable();
            if (orders == null) { return NotFound(); };
            var ordered = orders.OrderBy(o => o.DateTime);
            return Ok(ordered);
        }

        /// <summary>
        /// HTTP GET method which returns an order based on its id.
        /// </summary>
        /// <param name="id">Id of the order.</param>
        /// <returns>Ok(order) or NotFound()</returns>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            //var order = client.GetDatabase(databaseName)
            //    .GetCollection<Order>("Orders")
            //    .Find(o => o.Id == id)
            //    .FirstOrDefault();

            //if (order == null)
            //{
            //    return NotFound("Order not found");
            //}

            //var itemIds = order.ItemData.Select(oi => oi.ItemId).ToList();

            //var items = client.GetDatabase(databaseName)
            //    .GetCollection<Item>("Items")
            //    .Find(i => itemIds.Contains(i.Id))
            //    .ToList();

            //var itemLookup = items.ToDictionary(i => i.Id);

            //order.Items = order.ItemData.Select(itemData => new OrderItem
            //{
            //    Item = itemLookup.TryGetValue(itemData.ItemId, out var item) ? item : null,
            //    Quantity = itemData.Quantity
            //})
            //.Where(oi => oi.Item != null)
            //.ToList();

            //return Ok(order);
            return BadRequest();
        }

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

            // Check if each item in order is in collection
            foreach (var oid in order.ItemData)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(oid.ItemId));
                var current = await itemCollection.Find(filter).FirstOrDefaultAsync();

                if (current != null)
                {
                    current["quantity"] = oid.Quantity;
                    var json = current.ToJson();
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
