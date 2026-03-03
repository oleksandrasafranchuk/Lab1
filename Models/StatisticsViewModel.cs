using System;
using System.Collections.Generic;

namespace Lab1_Project.Models
{
    public class StatisticsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public Dictionary<string, int> BookingsByType { get; set; } = new();
        public Dictionary<string, decimal> RevenueByWorkspace { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}