using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
            var orders = client
                .GetDatabase(databaseName)
                .GetCollection<Order>("Orders")
                .AsQueryable()
                .ToList();

            var allItemIds = orders
                .SelectMany(o => o.ItemData.Select(item => item.ItemId))
                .Distinct()
                .ToList();

            var items = client
                .GetDatabase(databaseName)
                .GetCollection<Item>("Items")
                .Find(i => allItemIds.Contains(i.Id))
                .ToList();

            var itemLookup = items.ToDictionary(i => i.Id);

            foreach (var order in orders)
            {
                order.Items = order.ItemData.Select(itemData => new OrderItem
                {
                    Item = itemLookup.TryGetValue(itemData.ItemId, out var item) ? item : null,
                    Quantity = itemData.Quantity
                }).Where(oi => oi.Item != null) // Filter out items that weren't found
                .ToList();
            }

            return Ok(orders);
        }

        /// <summary>
        /// HTTP GET method which returns an order based on its id.
        /// </summary>
        /// <param name="id">Id of the order.</param>
        /// <returns>Ok(order) or NotFound()</returns>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var order = client.GetDatabase(databaseName)
                .GetCollection<Order>("Orders")
                .Find(o => o.Id == id)
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound("Order not found");
            }

            var itemIds = order.ItemData.Select(oi => oi.ItemId).ToList();

            var items = client.GetDatabase(databaseName)
                .GetCollection<Item>("Items")
                .Find(i => itemIds.Contains(i.Id))
                .ToList();

            var itemLookup = items.ToDictionary(i => i.Id);

            order.Items = order.ItemData.Select(itemData => new OrderItem
            {
                Item = itemLookup.TryGetValue(itemData.ItemId, out var item) ? item : null,
                Quantity = itemData.Quantity
            })
            .Where(oi => oi.Item != null)
            .ToList();

            return Ok(order);
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
    }
}
