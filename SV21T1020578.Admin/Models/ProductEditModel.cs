namespace SV21T1020578.Admin.Models

{
    using SV21T1020578.DomainModels;
    public class ProductEditModel
    {

        public Product Product { get; set; } = new Product();
        public List<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
        public List<ProductPhoto> ProductPhotos { get; set; } = new List<ProductPhoto>();
    }
}
