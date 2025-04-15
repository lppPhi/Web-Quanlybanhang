using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Models
{
    public class OrderSearchOutput : PaginationSearchOutput<Order>
    {
        public int Status { get; set; } = 0;
        public string TimeRange { get; set; } = "";
    }
}
