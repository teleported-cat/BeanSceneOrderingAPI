using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using BCrypt.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace BeanSceneOrderingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StaffController : ControllerBase
    {
        MongoClient client;
        string databaseName;

        public StaffController(IOptions<BeanSceneDatabaseSettings> databaseSettings)
        {
            databaseName = databaseSettings.Value.DatabaseName;
            client = new MongoClient(databaseSettings.Value.ConnectionString);
        }

        /// <summary>
        /// HTTP GET method which returns all staff members in the database.
        /// </summary>
        /// <returns>Ok(collection) or NotFound()</returns>
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult Get()
        {
            var collection = client.GetDatabase(databaseName).GetCollection<Staff>("Staff").AsQueryable();
            if (collection == null) { return NotFound(); }
            var ordered = collection.OrderBy(i => i.Role).ThenBy(i => i.LastName).ThenBy(i => i.FirstName);

            var dtoData = ordered.Select(s => new StaffDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Username = s.Username,
                Email = s.Email,
                Role = s.Role,
            }).ToList();

            return Ok(dtoData);
        }

        /// <summary>
        /// HTTP POST method which returns a staff DTO if username & password match, or returns unauthorised if they don't.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] Dictionary<string, string> credentials)
        {

            if (!credentials.ContainsKey("username") || !credentials.ContainsKey("password"))
            {
                return BadRequest("Malformed body! It must have 'username' and 'password'!");
            }

            var filter = Builders<Staff>.Filter.Eq("username", credentials["username"]);
            var account = client.GetDatabase(databaseName).GetCollection<Staff>("Staff").Find(filter).FirstOrDefault();
            
            if (account == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(credentials["password"], account.PasswordHash);

            if (!isValidPassword)
            {
                return Unauthorized("Invalid credentials.");
            }

            var dtoAccount = new StaffDto
            {
                Id = account.Id,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Username = account.Username,
                Email = account.Email,
                Role = account.Role,
            };

            return Ok(dtoAccount);
        }

        /// <summary>
        /// HTTP POST method which inserts a new staff member.
        /// </summary>
        /// <param name="item">Staff member to be inserted.</param>
        /// <returns>CreatedAtAction() or BadRequest()</returns>
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Staff staff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(staff.PasswordHash);

            var hashedAccount = new Staff
            {
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Username = staff.Username,
                Email = staff.Email,
                PasswordHash = hashedPassword,
                Role = staff.Role,
            };

            await client.GetDatabase(databaseName).GetCollection<Staff>("Staff").InsertOneAsync(hashedAccount);

            return CreatedAtAction(nameof(Get), new { id = hashedAccount.Id }, hashedAccount);
        }

        /// <summary>
        /// HTTP PUT method which updates an existing staff member. Doesn't include password.
        /// </summary>
        /// <param name="item">Staff member to be updated. The id must match one in the collection.</param>
        /// <returns>NotFound(), Ok() or StatusCode(500)</returns>
        [Authorize(Roles = "Manager")]
        [HttpPut]
        public async Task<IActionResult> Put(StaffDto staff)
        {
            try
            {
                var filter = Builders<Staff>.Filter.Eq("_id", ObjectId.Parse(staff.Id));

                if (filter == null)
                {
                    return NotFound(0);
                }

                var update = Builders<Staff>.Update
                    .Set("firstname", staff.FirstName)
                    .Set("lastname", staff.LastName)
                    .Set("username", staff.Username)
                    .Set("email", staff.Email)
                    .Set("role", staff.Role)
                    ;

                var result = await client.GetDatabase(databaseName)
                    .GetCollection<Staff>("Staff")
                    .UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound("Staff not found");
                }

                if (result.ModifiedCount == 0)
                {
                    return Ok("Staff found but no changes were made");
                }

                return Ok("Staff updated");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error updating staff: {e.Message}");
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id}/UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(string id, [FromBody] string newPassword)
        {
            try
            {
                var filter = Builders<Staff>.Filter.Eq("_id", ObjectId.Parse(id));

                if (filter == null)
                {
                    return NotFound(0);
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                var update = Builders<Staff>.Update
                    .Set("passwordhash", hashedPassword);

                var result = await client.GetDatabase(databaseName)
                    .GetCollection<Staff>("Staff")
                    .UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound("Staff not found");
                }

                if (result.ModifiedCount == 0)
                {
                    return Ok("Staff found but no changes were made");
                }

                return Ok("Staff password updated");

            } catch (Exception e) 
            {
                return StatusCode(500, $"Error updating staff: {e.Message}");
            }
        }

        /// <summary>
        /// HTTP method which deletes a staff member from the collection.
        /// </summary>
        /// <param name="id">The id of the staff member to be deleted.</param>
        /// <returns>Ok() or NotFound()</returns>
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var filter = Builders<Staff>.Filter.Eq("_id", ObjectId.Parse(id));

            if (filter == null)
            {
                return NotFound(0);
            }

            var result = await client.GetDatabase(databaseName)
                .GetCollection<Staff>("Staff")
                .DeleteOneAsync(filter);

            return Ok("Staff deleted");
        }
    }
}
