namespace COS.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using COS.Models;


using Data;
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Details(int id)
    {
        var order = _context.Orders.Find(id);
         if (order == null)
    {
        // Handle the case where the order is not found
        return NotFound();
    }
        var orderItems = _context.OrderItems.Where(oi => oi.OrderId == id).ToList();

        var model = new OrderDetailsViewModel
        {
            Order = order,
            OrderItems = orderItems
        };

        return View(model);
    }
}
