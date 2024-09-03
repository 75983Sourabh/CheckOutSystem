namespace COS.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

using Data;
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult List()
    {
        var products = _context.Products.ToList();
        return View(products);
    }
}
