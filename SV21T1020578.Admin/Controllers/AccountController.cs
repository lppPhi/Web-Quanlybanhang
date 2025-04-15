using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.BusinessLayers;
using System.Security.Claims;
using static SV21T1020578.BusinessLayers.UserAccountService;
namespace SV21T1020578.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện trang login
        /// </summary>
        /// <returns></returns>
        /// 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// Thực hiện yêu cầu login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username = "", string password = "")
        {
            ViewBag.Username = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập email và mật khẩu");
                return View();
            }

            var userAccount = UserAccountService.EmployeeAccountDB.Authentiate(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            //Nếu đăng nhập thành công
            //1. Tạo ra đối tượng chứa thông tin sẽ lưu vào Cookie
            var webUserData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };
            //2. Ghi nhận phiên đăng nhập, trả cookie về cho Client
            await HttpContext.SignInAsync(webUserData.CreatePrincipal());
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LogoutAsync()
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
            var userType = UserTypes.Employee;
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

        public IActionResult AccessDenined()
        {
            return View();
        }
    }
}
