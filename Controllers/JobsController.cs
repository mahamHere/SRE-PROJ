using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Data.SqlClient;

namespace Project.Controllers
{
    public class JobsController : Controller
    {
        private readonly ProjectContext _context;
        private readonly string _connectionString;

        public JobsController(ProjectContext context, string connectionString)
        {
            _context = context;
            _connectionString = connectionString;
        }

        public IActionResult GetVerification()
        {
            TempData["action"] = "Index";
            TempData["controller"] = "Jobs";
            return RedirectToAction("Index", "Admin");
        }

        // GET: Jobs
        public async Task<IActionResult> Index()
        {
            List<JobsModel> jobs = await GetJobsFromDatabase();
            return View(jobs);
        }

        private async Task<List<JobsModel>> GetJobsFromDatabase()
        {
            List<JobsModel> jobs = new List<JobsModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Jobs";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    if (dataReader.HasRows)
                    {
                        while (await dataReader.ReadAsync())
                        {
                            JobsModel job = new JobsModel();
                            job.Id = dataReader.GetInt32(dataReader.GetOrdinal("Id"));
                            job.JobTitle = dataReader.GetString(dataReader.GetOrdinal("JobTitle"));
                            job.Minsalary = dataReader.GetDecimal(dataReader.GetOrdinal("MinSalary"));
                            job.Maxsalary = dataReader.GetDecimal(dataReader.GetOrdinal("MaxSalary"));
                            jobs.Add(job);
                        }
                    }
                    dataReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return jobs;
        }

        // GET: Jobs/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JobTitle,Minsalary,Maxsalary")] JobsModel job)
        {
            if (ModelState.IsValid)
            {
                await AddJobToDatabase(job);
                return RedirectToAction(nameof(Index));
            }
            return View(job);
        }

        private async Task AddJobToDatabase(JobsModel job)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Jobs VALUES(@jtitle, @minsal, @maxsal)";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@jtitle", job.JobTitle);
                command.Parameters.AddWithValue("@minsal", job.Minsalary);
                command.Parameters.AddWithValue("@maxsal", job.Maxsalary);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        // Other CRUD actions (Edit, Delete, Details) can be similarly refactored for improved readability and adherence to SRP.
    }
}
