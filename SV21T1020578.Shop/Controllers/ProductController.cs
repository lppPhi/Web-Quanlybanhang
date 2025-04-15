using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;
using SV21T1020578.Shop.AppCodes;
using SV21T1020578.Shop.Models;


namespace SV21T1020578.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 16;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSeachCondition";
        //public IActionResult Index()
        //{
        //    PaginationSearchInput? condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
        //    if (condition == null)
        //    {
        //        condition = new ProductSearchInput()
        //        {
        //            Page = 1,
        //            PageSize = PAGE_SIZE,
        //            SearchValue = "",
        //            CategoryID = 0,
        //            SupplierID = 0,
        //        };
        //    }
        //    return View(condition);
        //}

        //public IActionResult Search(ProductSearchInput condition)
        //{
        //    var data = ProductDataService.ListProducts(
        //        out int rowCount,
        //        condition.Page,
        //        condition.PageSize,
        //        condition.SearchValue,
        //        condition.CategoryID,
        //        condition.SupplierID,
        //        condition.MinPrice,
        //        condition.MaxPrice
        //    );

        //    ProductSearchResult result = new()
        //    {
        //        Page = condition.Page,
        //        PageSize = condition.PageSize,
        //        SearchValue = condition.SearchValue,
        //        RowCount = rowCount,
        //        Data = data,
        //    };

        //    ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

        //    return View(result);
        //}
        public IActionResult Details(int id)
        {
            // Lấy thông tin sản phẩm theo ID
            Product? product = CommonDataService.ProductDB.Get(id);
            if (product == null)
            {
                // Trả về một thông báo lỗi chi tiết
                ViewBag.ErrorMessage = "Sản phẩm không tồn tại hoặc đã bị xóa."; 
                return View("Error");
            }

            return View(product); // Trả về view cùng với dữ liệu sản phẩm.
        }
    }
}