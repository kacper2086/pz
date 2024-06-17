using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using pz.Models;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TasksController> _logger;

        public TasksController(IConfiguration configuration, ILogger<TasksController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public ActionResult<IEnumerable<Task>> GetTasks()
        {
            List<Task> tasks = new List<Task>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "SELECT taskid, \"TaskName\", \"Operator\", \"Date\" FROM task";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Task task = new Task
                                {
                                    TaskId = Convert.ToInt32(reader["taskid"]),
                                    TaskName = reader["TaskName"].ToString(),
                                    Operator = reader["Operator"].ToString(),
                                    Date = Convert.ToDateTime(reader["Date"])
                                };
                                tasks.Add(task);
                            }
                        }
                    }
                }

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tasks.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/tasks/1
        [HttpGet("{id}")]
        public ActionResult<Task> GetTask(int id)
        {
            Task task = null;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "SELECT taskid, \"TaskName\", \"Operator\", \"Date\" FROM task WHERE taskid = @TaskId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", id);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                task = new Task
                                {
                                    TaskId = Convert.ToInt32(reader["taskid"]),
                                    TaskName = reader["TaskName"].ToString(),
                                    Operator = reader["operator"].ToString(),
                                    Date = Convert.ToDateTime(reader["date"])
                                };
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving task details.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/tasks
        [HttpPost]
        public ActionResult<Task> CreateTask(Task task)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "INSERT INTO task (\"TaskName\", \"Operator\", \"Date\") VALUES (@TaskName, @Operator, @Date) RETURNING taskid";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskName", task.TaskName);
                        command.Parameters.AddWithValue("@Operator", task.Operator);
                        command.Parameters.AddWithValue("@Date", task.Date);

                        object result = command.ExecuteScalar();
                        int newTaskId = Convert.ToInt32(result);
                        task.TaskId = newTaskId;
                    }
                }

                return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new task.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/tasks/1
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, Task task)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    
                    string query = "UPDATE task SET \"TaskName\" = @TaskName, \"Operator\" = @Operator, \"Date\" = @Date WHERE taskid = @TaskId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", id);
                        command.Parameters.AddWithValue("@TaskName", task.TaskName);
                        command.Parameters.AddWithValue("@Operator", task.Operator);
                        command.Parameters.AddWithValue("@Date", task.Date);

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
                _logger.LogError(ex, "An error occurred while updating the task.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/tasks/1
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    string query = "DELETE FROM task WHERE taskid = @TaskId";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", id);

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
                _logger.LogError(ex, "An error occurred while deleting the task.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
