namespace COS.Models;
public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string PaymentStatus { get; set; }
}
