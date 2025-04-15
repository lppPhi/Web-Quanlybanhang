using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Models
{
    public class OrderDetaiModel
    {
        public Order? Order { get; set; }
        public required List<OrderDetail> Details { get; set; }
    }
}
