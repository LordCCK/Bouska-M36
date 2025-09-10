using System.Collections.Generic;

namespace M36Backend.Models
{
    public class Order
    {
        public string Number { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class OrderDetails
    {
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PCN { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class TestResult
    {
        public string Order { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public double Setpoint { get; set; }
        public double Leak { get; set; }
        public string Result { get; set; } = string.Empty;
        public string PCN { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class PrintRequest
    {
        public string Type { get; set; } = string.Empty; // "all" nebo "single"
        public string PartNumber { get; set; } = string.Empty;
        public OrderDetails? Order { get; set; }
    }
}
