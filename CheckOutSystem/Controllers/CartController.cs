namespace COS.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

using Data;
using COS.Models;
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult AddToCart(int id)
    {
        var cartItem = new CartItem { ProductId = id, Quantity = 1 }; // Assume quantity is 1
        _context.CartItems.Add(cartItem);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        var cartItems = _context.CartItems.ToList();
        return View(cartItems);
    }
}
