namespace COS.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;
    using Data;
    using Models;

    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model); // Return to the checkout page with validation errors
            }

            // Create an Order
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                CustomerName = model.CustomerName,
                ShippingAddress = model.ShippingAddress,
                TotalAmount = CalculateTotalAmount() // Calculate the total amount from cart
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create Order Items
            var cartItems = _context.CartItems.ToList(); // Fetch cart items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = _context.Products.Find(cartItem.ProductId).Price
                };
                _context.OrderItems.Add(orderItem);
            }
            await _context.SaveChangesAsync();

            // Clear the cart
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Process Payment
            var paymentData = new Dictionary<string, string>
            {
                { "merchant_id", _configuration["CCAvenue:MerchantId"] },
                { "amount", order.TotalAmount.ToString("F2") },
                { "order_id", order.Id.ToString() },
                { "currency", "INR" },
                { "redirect_url", Url.Action("Callback", "Payment", null, Request.Scheme) },
                { "cancel_url", Url.Action("Cancel", "Payment", null, Request.Scheme) }
            };

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(paymentData);

            var response = await client.PostAsync("https://www.ccavenue.com/transaction/transaction.do?command=initiateTransaction", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                var redirectUrl = responseData.ContainsKey("redirect_url") ? responseData["redirect_url"] : null;

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    return View("PaymentFailure"); // Handle cases where redirect URL is missing
                }

                return Redirect(redirectUrl);
            }
            else
            {
                // Handle payment initiation failure
                return View("PaymentFailure");
            }
        }

        private decimal CalculateTotalAmount()
        {
            // Calculate the total amount from the cart items
            var cartItems = _context.CartItems.ToList();
            var totalAmount = cartItems.Sum(item => item.Quantity * _context.Products.Find(item.ProductId).Price);
            return totalAmount;
        }
    }
}
