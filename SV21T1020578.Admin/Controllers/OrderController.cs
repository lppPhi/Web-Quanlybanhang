using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class OrderController : Controller
    {
        private const string ORDER_SEARCH_INPUT = "OrderSearchInput";
        public IActionResult Index(int page = 1, string searchValue = "")
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_INPUT);
            if (input == null)
            {
                var cultureInfo = new CultureInfo("en-GB");
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = 10,
                    SearchValue = "",
                    Status = 0,
                    TimeRange = $"{DateTime.Today.AddYears(-2).ToString("dd/MM/yyyy", cultureInfo)} - {DateTime.Today.ToString("dd/MM/yyy", cultureInfo)} "
                };

            }
            return View(input);
        }

        public IActionResult Search(OrderSearchInput input)
        {

            int rowCount;
            var data = SaleDataService.ListOrders(out rowCount, input.Page, input.PageSize, input.Status, input.FromTime, input.ToTime, input.SearchValue ?? "");

            OrderSearchOutput model = new OrderSearchOutput()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                Status = input.Status,
                TimeRange = input.TimeRange,
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(ORDER_SEARCH_INPUT, input);
            return View(model);
        }

        public IActionResult Details(int id = 0)
        {
            var order = SaleDataService.GetOrder(id);
            if (order == null)
                return RedirectToAction("Index");
            var details = SaleDataService.ListOrderDetails(id);
            var model = new OrderDetaiModel()
            {
                Order = order,
                Details = details
            };
            return View(model);
        }


        //Số mặt hàng được hiển thị trên một trang khi tìm kiếm mặt hàng để đưa vào đơn hàng
        private const int PRODUCT_PAGE_SIZE = 5;
        //Tên biến session lưu điều kiện tìm kiếm mặt hàng khi lập đơn hàng
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchForSale";
        //Tên biến session lưu giỏ hàng
        private const string SHOPPING_CART = "ShoppingCart";


        /// <summary>
        /// Giao diện trang lập đơn hàng mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }
        /// <summary>
        /// Tìm kiếm mặt hàng cần bán
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IActionResult SearchProduct(ProductSearchInput condition)
        {
            var model = new ProductSearchOutput()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = CommonDataService.ProductDB.Count(condition.SearchValue ?? ""),
                Data = CommonDataService.ProductDB.List(condition.Page, condition.PageSize, condition.SearchValue ?? "")
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }
        /// <summary>
        /// Lấy giỏ hàng hiện đang có (lưu trong session)
        /// </summary>
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

        /// <summary>
        /// Bổ sung thêm hàng vào giỏ
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IActionResult AddToCart(CartItem item)
        {
            if (item.SalePrice < 0 || item.Quantity <= 0)
                return Json("Giá bán và số lượng không hợp lệ");

            var shoppingCart = GetShoppingCart();
            var existsProduct = shoppingCart.FirstOrDefault(m => m.ProductID == item.ProductID);
            if (existsProduct == null)
            {
                shoppingCart.Add(item);
            }
            else
            {
                existsProduct.Quantity += item.Quantity;
                existsProduct.SalePrice = item.SalePrice;
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        /// <summary>
        /// Xóa một mặt hàng ra khỏi giỏ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult RemoveFromCart(int id = 0)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
                shoppingCart.RemoveAt(index);
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        /// <summary>
        /// Khởi tạo đơn hàng
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        public IActionResult Init(int customerID = 0, string deliveryProvince = "", string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
                return Json("Giỏ hàng trống. Vui lòng chọn mặt hàng cần bán");

            if (customerID == 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
                return Json("Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao hàng");

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var item in shoppingCart)
            {
                orderDetails.Add(new OrderDetail()
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });
            }
            Order data = new Order()
            {
                OrderID = 0,
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                EmployeeID = null
            };
            int orderID = SaleDataService.AddOrder(data);
            if (orderID > 0)
            {
                foreach (var item in shoppingCart)
                {
                    SaleDataService.SaveOrderDetail(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }
                ClearCart();
                return Json(orderID);
            }
            else
            {
                return Json("Không lập được đơn hàng");
            }
        }
        /// <summary>
        /// Trang hiển thị giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ShoppingCart()
        {
            return View(GetShoppingCart());
        }
        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Accept(int id = 0)
        {
            bool result = SaleDataService.AcceptOrder(id, Convert.ToInt32(User.GetUserData()?.UserId));
            if (!result)
                TempData["Message"] = "Không thể duyệt đơn hàng này";

            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Hiển thị giao diện để chọn người giao hàng cho đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Shipping(int id = 0)
        {
            var order = SaleDataService.GetOrder(id);
            return View(order);
        }
        /// <summary>
        /// Ghi nhận chuyển đơn hàng cho người giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Shipping(int id = 0, int shipperID = 0)
        {
            if (shipperID <= 0)
                return Json("Vui lòng chọn người giao hàng");

            bool result = SaleDataService.ShipOrder(id, shipperID);
            if (!result)
                return Json("Đơn hàng không cho phép chuyển cho người giao hàng");

            return Json("");
        }
        /// <summary>
        /// Ghi nhận hoàn tất đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Finish(int id = 0)
        {
            bool result = SaleDataService.FinishOrder(id);
            if (!result)
                TempData["Message"] = "Không thể ghi nhận trạng thái kết thúc cho đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Hủy bỏ đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Cancel(int id = 0)
        {
            bool result = SaleDataService.CancelOrder(id);
            if (!result)
                TempData["Message"] = "Không thể hủy đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Reject(int id = 0)
        {
            bool result = SaleDataService.RejectOrder(id, Convert.ToInt32(User.GetUserData()?.UserId));
            if (!result)
                TempData["Message"] = "Không thể từ chối đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            bool result = SaleDataService.DeleteOrder(id);
            if (!result)
                TempData["Message"] = "Không được phép xóa đơn hàng này";

            return RedirectToAction("Details", new { id });
        }
        [HttpGet]
        public IActionResult EditDetail(int id = 0, int productId = 0)
        {
            if (id <= 0 || productId <= 0)
            {
                return RedirectToAction("Details", new { id });
            }

            // Lấy thông tin chi tiết đơn hàng
            var orderDetail = SaleDataService.GetOrderDetail(id, productId);
            if (orderDetail == null)
            {
                return RedirectToAction("Details", new { id });
            }

            return View(orderDetail);
        }


        [HttpPost]
        public IActionResult UpdateDetail(OrderDetail model)
        {
            if (model == null || model.OrderID <= 0 || model.ProductID <= 0)
            {
                return RedirectToAction("Details", new { id = model?.OrderID });
            }

            // Kiểm tra dữ liệu đầu vào
            if (model.Quantity <= 0)
            {
                ModelState.AddModelError(nameof(model.Quantity), "Số lượng phải lớn hơn 0.");
            }

            if (model.SalePrice < 0)
            {
                ModelState.AddModelError(nameof(model.SalePrice), "Giá bán phải lớn hơn 0.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Cập nhật đơn hàng
            bool success = SaleDataService.SaveOrderDetail(model.OrderID, model.ProductID, model.Quantity, model.SalePrice);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể cập nhật chi tiết đơn hàng.");
                return View(model);
            }

            // Quay lại trang chi tiết đơn hàng nếu thành công
            return Json("");
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
            if (order.Status != Constants.ORDER_INIT && order.Status != Constants.ORDER_ACCEPTED)
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
    }
}
