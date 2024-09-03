namespace COS.Models
{
    using System.Collections.Generic;

    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
