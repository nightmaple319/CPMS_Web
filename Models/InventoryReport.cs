using System;

namespace CPMS_Web.Models
{
    public class InventoryReport
    {
        public DateTime TransactionDate { get; set; }
        public string SparePartId { get; set; }
        public string SparePartName { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string Remarks { get; set; }
    }
} 