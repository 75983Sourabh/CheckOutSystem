namespace COS.Models
{
    using System.ComponentModel.DataAnnotations;

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Customer Name is required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Shipping Address is required")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        // Add other properties here if needed, such as payment info
    }
}
