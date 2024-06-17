using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using pz.Models;
using System.Data;

namespace pz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = "SELECT login, password, usertype FROM public.users WHERE \"login\" = @Username AND \"password\" = @Password";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", user.Login);
                command.Parameters.AddWithValue("@Password", user.Password);

                try
                {
                    connection.Open();
                    NpgsqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string userTypeFromDB = reader["usertype"].ToString();
                        return Ok(new { UserType = userTypeFromDB });
                    }
                    else
                    {
                        return Unauthorized("Invalid login or password");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error");
                }
            }
        }
    }
}
