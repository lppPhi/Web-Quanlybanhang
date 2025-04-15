using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;
using SV21T1020578.Shop.AppCodes;
using System.Globalization;
using SV21T1020578.Shop.Models;

namespace SV21T1020578.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string SHOPPING_CART = "ShoppingCart";
        public const int PAGE_SIZE = 10;
        private const string ORDER_SEARCH_CONDITION = "OrderSearchCondition";
        private const int PRODUCT_PAGE_SIZE = 5;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchForSale";
  
        public IActionResult Index()

        {
            ViewBag.StatusList = new Dictionary<int, string>
            {
                { 0, "--    Trạng thái    --" },
                { 1, "Đơn hàng mới (chờ duyệt)" },
                { 2, "Đơn hàng đã duyệt (chờ chuyển hàng)" },
                { 3, "Đơn hàng đang được giao" },
                { 4, "Đơn hàng đã hoàn tất thành công" },
                { -1, "Đơn hàng bị hủy" },
                { -2, "Đơn hàng bị từ chối" }
            };
            var condition = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_CONDITION);
            if (condition == null)
            {
                var cultureInfo = new CultureInfo("en-GB");
                condition = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = 10,
                    SearchValue = "",
                    Status = 0,
                    TimeRange = $"{DateTime.Today.AddYears(-2).ToString("dd/MM/yyyy", cultureInfo)} - {DateTime.Today.ToString("dd/MM/yyy", cultureInfo)} "
                };

            }
            return View(condition);
        }
        public IActionResult Search(OrderSearchInput condition)
        {
            // Lấy thông tin khách hàng đã đăng nhập (CustomerID)
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return RedirectToAction("Login", "Account"); 
            }

            int customerID = int.Parse(userData.UserId);  // Lấy CustomerID của khách hàng đã đăng nhập

            int rowCount;
            // Lấy tất cả đơn hàng của khách hàng với phân trang
            var data = SaleDataService.ListOrdersByCustomer(out rowCount, customerID, condition.Status, condition.FromTime, condition.ToTime, condition.SearchValue ?? "", condition.Page, condition.PageSize);

            OrderSearchResult model = new OrderSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                Status = condition.Status,
                TimeRange = condition.TimeRange,
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(ORDER_SEARCH_CONDITION, condition);
            return View(model);
        }




        private List<CartItem> GetShoppingCart()
        {
            var shoppingCart = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART);
            if (shoppingCart == null)
            {
                shoppingCart = new List<CartItem>();
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            }
            return shoppingCart;
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity, decimal salePrice, string productName)
        {
            if (quantity <= 0)
                return Json("Số lượng không hợp lệ");

            var shoppingCart = GetShoppingCart();
            var product = CommonDataService.ProductDB.Get(productId);
            var existsProduct = shoppingCart.FirstOrDefault(m => m.ProductID == productId);

            if (existsProduct == null)
            {
                // Tạo mới nếu sản phẩm chưa có trong giỏ hàng
                var newItem = new CartItem
                {
                    ProductID = productId,
                    Quantity = quantity,
                    SalePrice = salePrice,
                    ProductName = productName,
                    Photo = product.Photo// Gán tên sản phẩm từ request
                };
                shoppingCart.Add(newItem);
            }
            else
            {
                existsProduct.Quantity += quantity;
            }

            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json(""); // Trả về rỗng nếu thành công
        }


        // Xóa sản phẩm khỏi giỏ hàng
        [HttpGet]
        public IActionResult RemoveFromCart(int id)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
                shoppingCart.RemoveAt(index);

            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json(""); // Thành công
        }

        // Xóa toàn bộ giỏ hàng
        [HttpGet]
        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json(""); // Thành công
        }

        // Hiển thị giỏ hàng
        public IActionResult ShoppingCart()
        {
            return View(GetShoppingCart());
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return Json("Số lượng không hợp lệ");
            }

            var shoppingCart = GetShoppingCart(); 
            var item = shoppingCart.FirstOrDefault(i => i.ProductID == id);

            if (item != null)
            {
                item.Quantity = quantity;
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);  
            }

            return Json("");  
        }

        public IActionResult Init(string deliveryProvince = "", string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
                return Json("Giỏ hàng trống. Vui lòng chọn mặt hàng bạn cần mua");

            if (string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return Json("Vui lòng nhập đầy đủ thông tin nơi giao hàng");

            // Lấy thông tin khách hàng từ user đã đăng nhập
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return Json("Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.");

            int customerID = int.Parse(userData.UserId);

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var item in shoppingCart)
            {
                orderDetails.Add(new OrderDetail()
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                });
            }

            int orderID = SaleDataService.InitOrderCustomer(customerID, deliveryProvince, deliveryAddress, orderDetails);
            ClearCart();
            return Json(orderID);
        }
        public IActionResult Details(int id = 0)
        {
            var order = SaleDataService.GetOrder(id);
            if (order == null)
                return RedirectToAction("Index");
            var details = SaleDataService.ListOrderDetails(id);
            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details
            };
            return View(model);
        }
        public IActionResult EditDetail(int id = 0, int productId = 0)
        {
            // Lấy chi tiết đơn hàng dựa trên OrderID và ProductID
            var orderDetail = SaleDataService.GetOrderDetail(id, productId);

            if (orderDetail == null)
            {
                return RedirectToAction("Index");
            }

            // Truyền chi tiết đơn hàng tới view
            return View(orderDetail);
        }
        [HttpPost]
        public ActionResult UpdateDetail(OrderDetail data)
        {


            // Kiểm tra số lượng
            if (data.Quantity < 1)
            {
                ModelState.AddModelError(nameof(data.Quantity), "Số lượng phải lớn hơn 0.");
            }


            // Nếu có lỗi, trả về trang chi tiết cùng với các thông báo lỗi
            if (!ModelState.IsValid)
            {
                // Lấy thông tin chi tiết đơn hàng hiện tại
                var orderDetails = SaleDataService.ListOrderDetails(data.OrderID);
                var order = SaleDataService.GetOrder(data.OrderID);

                // Trả về view "Details" cùng với dữ liệu lỗi và thông tin đơn hàng
                return View("Details", new OrderDetailModel
                {
                    Order = order,
                    Details = orderDetails
                });
            }

            // Lưu thông tin chi tiết đơn hàng nếu tất cả kiểm tra hợp lệ
            SaleDataService.SaveOrderDetail(data.OrderID, data.ProductID, data.Quantity, data.SalePrice);

            return RedirectToAction("Details", new { id = data.OrderID });
        }
        public IActionResult DeleteDetail(int id, int productId)
        {
            // Kiểm tra thông tin đơn hàng
            var order = SaleDataService.GetOrder(id);
            if (order == null)
            {
                ModelState.AddModelError("", "Đơn hàng không tồn tại.");
                return RedirectToAction("Details", new { id });
            }

            // Kiểm tra trạng thái đơn hàng trước khi cho phép xóa
            if (order.Status != Constants.ORDER_INIT /*&& order.Status != Constants.ORDER_ACCEPTED*/)


            {
                ModelState.AddModelError("", "Chỉ có thể xóa chi tiết đơn hàng khi đơn hàng ở trạng thái 'Init' hoặc 'Accepted'.");
                return RedirectToAction("Details", new { id });
            }

            // Xóa chi tiết đơn hàng
            bool success = SaleDataService.DeleteOrderDetail(id, productId);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể xóa chi tiết đơn hàng.");
            }

            // Quay lại trang chi tiết đơn hàng
            return RedirectToAction("Details", new { id });
        }
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            var order = SaleDataService.GetOrder(id);
            if (order == null)
            {
                ModelState.AddModelError("", "Đơn hàng không tồn tại.");
                return View("Details", new { id });
            }

            // Cập nhật trạng thái đơn hàng thành "Canceled"
            bool success = SaleDataService.CancelOrder(id);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể hủy đơn hàng.");
                return View("Details", new { id });
            }

            // Nếu thành công, quay lại trang Details
            return RedirectToAction("Details", new { id });
        }

    }

}
