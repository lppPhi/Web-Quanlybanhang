using SV21T1020578.DataLayers;
using SV21T1020578.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng để giao tiếp với các đối tượng dữ liệu
    /// liên quan đến: Khách hàng, Nhà cung cấp, Người giao hàng, Nhân viên và Loại hàng
    /// </summary>
    public static class CommonDataService
    {
        /// <summary>
        /// 
        /// </summary>
        static CommonDataService()
        {
            //string connectionString = @"server=.;user id=sa;password=123456;database=LiteCommerceDB;TrustServerCertificate=true";
            string connectionString = Configuration.ConnectionString;

            ProvinceDB = new ProvinceDAL(connectionString);
            CustomerDB = new CustomerDAL(connectionString);
            EmployeeDB = new EmployeeDAL(connectionString);
            SupplierDB = new SupplierDAL(connectionString);
            ShipperDB = new ShipperDAL(connectionString);
            CategoryDB = new CategoryDAL(connectionString);
            ProductDB = new ProductDAL(connectionString);
            OrderDB = new OrderDAL(connectionString);
            //ProductAttributeDB = new ProductDAL(connectionString);
            //ProductPhotoDB = new ProductDAL(connectionString);

        }

        /// <summary>
        /// Khách hàng
        /// </summary>
        public static ProvinceDAL ProvinceDB { get; private set; }

        /// <summary>
        /// Khách hàng
        /// </summary>
        public static CustomerDAL CustomerDB { get; private set; }
        /// <summary>
        /// Nhân viên
        /// </summary>
        public static EmployeeDAL EmployeeDB { get; private set; }
        /// <summary>
        /// Nhà cung cấp
        /// </summary>
        public static SupplierDAL SupplierDB { get; private set; }

        /// <summary>
        /// Người giao hàng
        /// </summary>
        public static ShipperDAL ShipperDB { get; private set; }

        /// <summary>
        /// Loại hàng
        /// </summary>
        public static CategoryDAL CategoryDB { get; private set; }
        /// <summary>
        /// Mặt hàng
        /// </summary>
        public static ProductDAL ProductDB { get; private set; }
        /// <summary>
        /// Lập đơn hàng
        /// </summary>
        public static OrderDAL OrderDB { get; private set; }

        public static List<Product> ListProducts(string searchValue = "")
        {
            return ProductDB.List(searchValue: searchValue);
        }

        public static List<Product> ListProducts(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "", int categoryId = 0, int supplierId = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            rowCount = ProductDB.Count(searchValue, categoryId, supplierId, minPrice, maxPrice);
            return ProductDB.List(page, pageSize, searchValue, categoryId, supplierId, minPrice, maxPrice);
        }
    }
}
