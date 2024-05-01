using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project.Data;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context, ILogger<TransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var transactions = _context.Transactions.ToList();
            return View(transactions);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            var purchases = _context.Purchases.Where(p => p.TransactionId == id).ToList();
            var products = purchases.Select(p => _context.Products.FirstOrDefault(pr => pr.Id == p.ProductId)).ToList();

            var cart = purchases.Zip(products, (p, pr) => new { Purchase = p, Product = pr }).ToList();
            ViewBag.Cart = cart;

            ViewBag.Change = transaction.Payment - transaction.Cost;

            return View(transaction);
        }

        public IActionResult Search(DateTime? date)
        {
            if (date == null)
            {
                return View(new List<Transaction>());
            }

            var transactions = _context.Transactions.Where(t => t.TransactionDate.Date == date.Value.Date).ToList();

            return View(transactions);
        }
    }
}
