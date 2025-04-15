using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;
using System.Security.Claims;

namespace SV21T1020578.Shop.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            // Lấy CustomerID từ claims của người dùng đã đăng nhập
            var userId = User.FindFirstValue(nameof(WebUserData.UserId));
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            int customerID = int.Parse(userId);

            // Lấy dữ liệu khách hàng từ cơ sở dữ liệu dựa trên CustomerID
            var customer = CommonDataService.CustomerDB.Get(customerID);

            if (customer == null)
            {
            
                return RedirectToAction("Login", "Account");
            }

            // Trả về dữ liệu khách hàng tới View
            return View(customer);
        }
        [HttpGet]
        public IActionResult Edit()
        {
            // Lấy CustomerID từ claims của người dùng đã đăng nhập
            var userId = User.FindFirstValue(nameof(WebUserData.UserId));
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            int customerID = int.Parse(userId); // Chuyển CustomerID từ chuỗi sang int

            // Lấy dữ liệu khách hàng từ cơ sở dữ liệu dựa trên CustomerID
            var customer = CommonDataService.CustomerDB.Get(customerID);

            if (customer == null)
            {
              
                return RedirectToAction("Login", "Account");
            }

            // Trả về dữ liệu khách hàng để chỉnh sửa
            return View(customer);
        }
        [HttpPost]
        public IActionResult Save(Customer customer)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                ModelState.AddModelError(nameof(customer.CustomerName), "Tên khách hàng không được để trống");

            if (string.IsNullOrWhiteSpace(customer.ContactName))
                ModelState.AddModelError(nameof(customer.ContactName), "Tên giao dịch không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Phone))
                ModelState.AddModelError(nameof(customer.Phone), "Vui lòng nhập điện thoại của khách hàng");

            if (string.IsNullOrWhiteSpace(customer.Email))
                ModelState.AddModelError(nameof(customer.Email), "Vui lòng nhập Email của khách hàng");

            if (string.IsNullOrWhiteSpace(customer.Address))
                ModelState.AddModelError(nameof(customer.Address), "Vui lòng nhập địa chỉ của khách hàng");

            if (string.IsNullOrWhiteSpace(customer.Province))
                ModelState.AddModelError(nameof(customer.Province), "Bạn chưa chọn tỉnh/thành của khách hàng");

            // Nếu có lỗi, trả lại View với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View("Edit", customer); // Trả lại dữ liệu cùng với các thông báo lỗi
            }

            // Thực hiện lưu hoặc cập nhật dữ liệu
            bool result = CommonDataService.CustomerDB.Update(customer);
            if (!result)
            {
                ModelState.AddModelError(nameof(customer.Email), "Email đã bị trùng. Vui lòng chọn email khác");
                return View("Edit", customer);
            }

            return RedirectToAction("Profile"); // Quay lại trang hồ sơ
        }




    }

}
