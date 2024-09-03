

namespace COS.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Linq;



using Data;
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // GET: Payment/Initiate
    public IActionResult Initiate(int orderId)
    {
        // Fetch order details based on orderId
        var order = _context.Orders.Find(orderId);
        if (order == null)
        {
            return NotFound();
        }

        // Prepare data for payment gateway
        var paymentData = new
        {
            // Example data, adjust as per CCAvenue API requirements
            Amount = order.TotalAmount,
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            Email = "customer@example.com", // Use actual customer email
            Mobile = "1234567890", // Use actual customer mobile number
            RedirectUrl = Url.Action("Callback", "Payment", null, Request.Scheme),
            CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme)
        };

        // Pass data to view for creating payment request
        return View(paymentData);
    }

    // POST: Payment/Process
    [HttpPost]
    public async Task<IActionResult> Process()
    {
        // This should be triggered after receiving payment details from the front-end
        var request = HttpContext.Request;

        // Extract payment details from request
        // You need to adjust this part based on how the payment gateway requires you to send the data

        var paymentDetails = new Dictionary<string, string>
        {
            { "merchant_id", _configuration["CCAvenue:MerchantId"] },
            { "amount", request.Form["amount"] },
            { "order_id", request.Form["order_id"] },
            { "currency", "INR" },
            { "redirect_url", Url.Action("Callback", "Payment", null, Request.Scheme) },
            { "cancel_url", Url.Action("Cancel", "Payment", null, Request.Scheme) }
        };

        // Prepare request to payment gateway
        var client = new HttpClient();
        var content = new FormUrlEncodedContent(paymentDetails);

        var response = await client.PostAsync("https://www.ccavenue.com/transaction/transaction.do?command=initiateTransaction", content);

        // Handle response and redirect user accordingly
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Redirect(responseContent); // Assuming response contains a redirect URL
        }

        return View("Error");
    }

    // GET: Payment/Callback
    public IActionResult Callback()
    {
        // Handle the payment gateway callback
        var paymentStatus = HttpContext.Request.Query["status"];
        var orderId = HttpContext.Request.Query["order_id"];

        if (paymentStatus == "success")
        {
            // Update order status in database
            var order = _context.Orders.Find(int.Parse(orderId));
            if (order != null)
            {
                order.Status = "Paid";
                _context.SaveChanges();
            }

            return View("PaymentSuccess");
        }

        return View("PaymentFailure");
    }

    // GET: Payment/Cancel
    public IActionResult Cancel()
    {
        // Handle payment cancellation
        return View("PaymentCancelled");
    }
}
