// BillsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using pz.Models;
using System;
using System.Collections.Generic;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BillsController> _logger;

        public BillsController(IConfiguration configuration, ILogger<BillsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // GET: api/bills
        [HttpGet]
        public ActionResult<IEnumerable<Bill>> GetBills()
        {
            List<Bill> bills = new List<Bill>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "SELECT billid, productid, name, quantity, price FROM bills";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Bill bill = new Bill
                                {
                                    billid = Convert.ToInt32(reader["billid"]),
                                    ProductId = Convert.ToInt32(reader["productid"]),
                                    Name = reader["name"].ToString(),
                                    Quantity = Convert.ToInt32(reader["quantity"]),
                                    Price = Convert.ToDecimal(reader["price"])
                                };
                                bills.Add(bill);
                            }
                        }
                    }
                }

                return Ok(bills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving bills.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/bills/1
        [HttpGet("{id}")]
        public ActionResult<Bill> GetBill(int id)
        {
            Bill bill = null;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "SELECT billid, productid, name, quantity, price FROM bills WHERE billid = @BillId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BillId", id);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bill = new Bill
                                {
                                    billid = Convert.ToInt32(reader["billid"]),
                                    ProductId = Convert.ToInt32(reader["productid"]),
                                    Name = reader["name"].ToString(),
                                    Quantity = Convert.ToInt32(reader["quantity"]),
                                    Price = Convert.ToDecimal(reader["price"])
                                };
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }

                return Ok(bill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving bill details.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/bills
        [HttpPost]
        public ActionResult<Bill> CreateBill(Bill bill)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "INSERT INTO bills (productid, name, quantity, price) VALUES (@ProductId, @Name, @Quantity, @Price) RETURNING billid";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", bill.ProductId);
                        command.Parameters.AddWithValue("@Name", bill.Name);
                        command.Parameters.AddWithValue("@Quantity", bill.Quantity);
                        command.Parameters.AddWithValue("@Price", bill.Price);

                        object result = command.ExecuteScalar();
                        int newbillId = Convert.ToInt32(result);
                        bill.billid = newbillId;
                    }
                }

                return CreatedAtAction(nameof(GetBill), new { id = bill.billid }, bill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new bill.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/bills/1
        [HttpPut("{id}")]
        public IActionResult UpdateBill(int id, Bill bill)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "UPDATE bills SET productid = @ProductId, name = @Name, quantity = @Quantity, price = @Price WHERE billid = @BillId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BillId", id);
                        command.Parameters.AddWithValue("@ProductId", bill.ProductId);
                        command.Parameters.AddWithValue("@Name", bill.Name);
                        command.Parameters.AddWithValue("@Quantity", bill.Quantity);
                        command.Parameters.AddWithValue("@Price", bill.Price);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound();
                        }
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the bill.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/bills/1
        [HttpDelete("{id}")]
        public IActionResult DeleteBill(int id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "DELETE FROM bills WHERE billid = @BillId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BillId", id);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound();
                        }
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the bill.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
