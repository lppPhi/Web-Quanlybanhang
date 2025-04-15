using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;
using SV21T1020578.Shop.AppCodes;
using System.Diagnostics;
using SV21T1020578.Shop.Models;

namespace SV21T1020578.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const int PAGE_SIZE = 16;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSeachCondition";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                };
            }
            return View(condition);
        }

        public IActionResult Search(ProductSearchInput condition)
        {
            var data = CommonDataService.ListProducts(
                out int rowCount,
                condition.Page,
                condition.PageSize,
                condition.SearchValue,
                condition.CategoryID,
                condition.SupplierID,
                condition.MinPrice,
                condition.MaxPrice
            );

            ProductSearchResult result = new()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data,
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);

            return View(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
