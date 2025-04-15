using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Models
{
    public class ProductSearchOutput : PaginationSearchOutput<Product>
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
    }
}
