using BCrypt.Net;
using BeanSceneOrderingAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BeanSceneOrderingAPI.Authentication
{
    /// <summary>
    /// Class for initalising authorisation attributes in controllers to use basic authentication.
    /// </summary>
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IMongoCollection<Staff> _staffCollection;

        /// <summary>
        /// Constructor for the authentication handler.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="databaseSettings"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            IOptions<BeanSceneDatabaseSettings> databaseSettings,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
            string databaseName = databaseSettings.Value.DatabaseName;
            var client = new MongoClient(databaseSettings.Value.ConnectionString);
            var database = client.GetDatabase(databaseName);
            _staffCollection = database.GetCollection<Staff>("Staff");
        }

        /// <summary>
        /// Enforces 'authorization' header on authorised methods, validates credentials, and determines if the user has sufficient access.
        /// </summary>
        /// <returns>
        /// The result of the authentication; 
        /// fails if the header is missing in request, if the credentials are invalid or doesn't have the correct role,
        /// and succeeds otherwise.
        /// </returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if auth header exists
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            try 
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                var username = credentials[0];
                var password = credentials[1];

                var staff = await _staffCollection.Find(s => s.Username == username).FirstOrDefaultAsync();

                if (staff == null) 
                {
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }

                bool isValidPassword;

                isValidPassword = BCrypt.Net.BCrypt.Verify(password, staff.PasswordHash);

                if (!isValidPassword) 
                {
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, staff.Id),
                    new Claim(ClaimTypes.Name, staff.Username),
                    new Claim(ClaimTypes.Role, staff.Role)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            } 
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
