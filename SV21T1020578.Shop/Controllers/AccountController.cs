using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;

using SV21T1020578.DomainModels;
using System.Security.Claims;
using static SV21T1020578.BusinessLayers.UserAccountService;


namespace SV21T1020578.Shop.Controllers
{

        [Authorize]
        public class AccountController : Controller
        {
            [AllowAnonymous]
            [HttpGet]
            public IActionResult Login()
            {
                return View();
            }

            [AllowAnonymous]
            [ValidateAntiForgeryToken]
            [HttpPost]
            public async Task<IActionResult> Login(string userName, string password)
            {
                ViewBag.UserName = userName;

                //Kiểm tra thông tin đầu vào
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("Error", "Nhập đầy đủ tên và mật khẩu");
                    return View();
                }

                var userAccount = Authentiate(UserTypes.Customer, userName, password);
                if (userAccount == null)
                {
                    ModelState.AddModelError("Error", "Đăng nhập thất bại");
                    return View();
                }
                //Đăng nhập thành công
                WebUserData userData = new WebUserData()
                {
                    UserId = userAccount.UserID,
                    UserName = userAccount.UserName,
                    DisplayName = userAccount.FullName,
                    Photo = userAccount.Photo,
                    Roles = userAccount.RoleNames.Split(',').ToList()
                };

                //2.Ghi nhận trạng thái đăng nhập
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

                //3. Quay về trang chủ
                return RedirectToAction("Index", "Home");
            }
            public async Task<IActionResult> Logout()
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }


            [HttpGet]
            public IActionResult ChangePassword()
            {
                return View();
            }

            // Xử lý đổi mật khẩu
            [HttpPost]
            public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
            {
                var username = User.FindFirstValue(nameof(WebUserData.UserName));

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("Password", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                    return View();
                }

                if (string.IsNullOrEmpty(username))
                {
                    ModelState.AddModelError("", "Không tìm thấy tên người dùng.");
                    return View();
                }

                // Xác thực người dùng
                var userType = UserTypes.Customer;
                var userAccount = Authentiate(userType, username, oldPassword);
                if (userAccount == null)
                {
                    ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng.");
                    return View();
                }

                // Thay đổi mật khẩu
                bool isPasswordChanged = UserAccountService.ChangePassword(userType, username, oldPassword, newPassword);
                if (isPasswordChanged)
                {
                    ViewBag.Message = "Đổi mật khẩu thành công!";
                    return View();
                    /*return View("Login");*/
                }
                else
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại.");
                    return View();
                }
            }
            [AllowAnonymous]
            [HttpGet]
            public IActionResult Register()
            {
                return View();
            }

            [AllowAnonymous]
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Register(string userName, string password, string confirmPassword, string displayName, string customerName, string contactName, string province, string address, string phone)
            {
                // Kiểm tra thông tin đầu vào
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(displayName))
                {
                    ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin.");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ViewData["PasswordError"] = "Mật khẩu và xác nhận mật khẩu không khớp.";
                    return View();
                }

                // Kiểm tra xem email đã tồn tại trong bảng Customer chưa
                if (UserAccountService.IsEmailExist(userName))
                {
                    ViewData["EmailError"] = "Email này đã được sử dụng. Vui lòng chọn email khác.";
                    return View();
                }

                // Tạo đối tượng Customer từ thông tin đăng ký
                var customer = new Customer
                {
                    CustomerName = customerName,
                    ContactName = contactName,
                    Province = province,
                    Address = address,
                    Phone = phone,
                    Email = userName 
                };

                // Đăng ký tài khoản cho người dùng
                var userType = UserTypes.Customer; // Dùng Customer khi đăng ký cho khách hàng
                bool isRegistered = UserAccountService.Register(userType, userName, password, displayName, customer);

                if (isRegistered)
                {
                    ViewBag.Message = "Đăng ký tài khoản thành công!";
                    return RedirectToAction("Login"); // Điều hướng người dùng đến trang đăng nhập
                }
                else
                {
                    ModelState.AddModelError("Error", "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
                    return View();
                }
            }




            public IActionResult ForgotPassword()
            {
                return View();
            }
            public IActionResult AccessDenined()
            {
                return View();
            }



    }


}
