using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;

namespace Project.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProjectContext _context;
        private readonly string _connectionString;

        public ProductsController(ProjectContext context, string connectionString)
        {
            _context = context;
            _connectionString = connectionString;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<ProductsModel> products = new List<ProductsModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Products";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    if (dataReader.HasRows)
                    {
                        while (await dataReader.ReadAsync())
                        {
                            ProductsModel product = MapDataReaderToProduct(dataReader);
                            products.Add(product);
                        }
                    }

                    dataReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return View(products);
        }

        private ProductsModel MapDataReaderToProduct(SqlDataReader dataReader)
        {
            return new ProductsModel
            {
                Id = (int)dataReader["Id"],
                Name = dataReader["Name"].ToString(),
                Category_id = (int)dataReader["Category_id"],
                Buying_Price = (decimal)dataReader["Buying_Price"],
                Selling_Price = (decimal)dataReader["Selling_Price"],
                Profit_Margin = (decimal)dataReader["Profit_Margin"],
                Quantity = (int)dataReader["Quantity"],
                Entrydate = (DateTime)dataReader["EntryDate"],
                Department_id = (int)dataReader["Department_id"]
            };
        }

        // Other controller actions...

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Category_id,Price,Quantity,Entrydate,Department_id")] ProductsModel product)
        {
            if (ModelState.IsValid)
            {
                await _context.AddAsync(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // Other controller actions...

        private bool ProductsModelExists(int id)
        {
            return _context.ProductsModel.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Search(string product_name)
        {
            List<ProductsModel> products = new List<ProductsModel>();

            if (product_name is null)
                return View(products);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Products WHERE Name LIKE @name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", "%" + product_name + "%");

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    if (dataReader.HasRows)
                    {
                        while (await dataReader.ReadAsync())
                        {
                            ProductsModel product = MapDataReaderToProduct(dataReader);
                            products.Add(product);
                        }
                    }

                    dataReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            return View(products);
        }
    }
}
