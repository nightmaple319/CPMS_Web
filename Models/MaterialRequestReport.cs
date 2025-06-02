using System;

namespace CPMS_Web.Models
{
    public class MaterialRequestReport
    {
        public DateTime RequestDate { get; set; }
        public string RequestNumber { get; set; }
        public string RequesterName { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 