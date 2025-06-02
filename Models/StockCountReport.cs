using System;

namespace CPMS_Web.Models
{
    public class StockCountReport
    {
        public DateTime CountDate { get; set; }
        public string CountNumber { get; set; }
        public string CounterName { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }
        public int SystemQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int Variance { get; set; }
    }
} 