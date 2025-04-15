using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class CustomerController : Controller
    {
        private const string CUSTOMER_SEARCH_INPUT = "CustomerSearchInput";

        /// <summary>
        /// GIao diện để nhập đầu vào tìm kiếm và hiển thị kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //kiểm tra trong session có điều kiện tìm kiếm được lưu lại hay không.
            //Nếu có thì sử dụng, nếu không thì tạo mới điều kiện tìm kiếm

            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH_INPUT);
            if (input == null)
            {

                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 20,
                    SearchValue = ""
                };
               
            }
            return View(input);
        }

        public IActionResult Search (PaginationSearchInput input)
        {
            PaginationSearchOutput<Customer> model = new PaginationSearchOutput<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = CommonDataService.CustomerDB.Count(input.SearchValue ?? ""),
                Data = CommonDataService.CustomerDB.List(input.Page, input.PageSize, input.SearchValue ?? "")
            };
            //Lưu lại điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_INPUT, input);
            return View(model);
        }
        /// <summary>
        /// Hiển thị giao diện để nhập khách hàng mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            //Tạo một đối tượng để lưu thông tin của khách hàng mới
            Customer model = new Customer()
            {
                CustomerId = 0,
                IsLocked = false
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Hiển thị giao diện để cập nhật thông tin của khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            //Lấy thông tin của khách hàng cần cập nhật
            Customer? model = CommonDataService.CustomerDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        /// <summary>
        /// Lưu dữ liệu khách hàng vào CSDL
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveData(Customer model)
        {
            try
            {
                ViewBag.Title = model.CustomerId == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";

                //Kiểm tra dữ liệu đầu vào, nếu dữ liệu không hợp lệ thì đưa các thông báo "lỗi"
                //vào trong ModelState
                if (string.IsNullOrWhiteSpace(model.CustomerName))
                    ModelState.AddModelError(nameof(model.CustomerName), "*");

                if (string.IsNullOrWhiteSpace(model.ContactName))
                    ModelState.AddModelError(nameof(model.ContactName), "*");

                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "*");
                else if (CommonDataService.CustomerDB.ExistsEmail(model.CustomerId, model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email bị trùng");

                if (string.IsNullOrWhiteSpace(model.Province))
                    ModelState.AddModelError(nameof(model.Province), "*");

                //Dựa vào thuộc tính IsValid của ModelState để biết có tồn tại trường hợp "lỗi" nào không?
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                if (model.CustomerId == 0)
                {
                    CommonDataService.CustomerDB.Add(model);
                }
                else
                {
                    CommonDataService.CustomerDB.Update(model);
                }
                return RedirectToAction("Index");
            }
            catch //(Exception ex)
            {
                //Ghi log của lỗi trong exception
                ModelState.AddModelError("Error", "Hệ thống tạm thời quá tải hoặc gián đoạn. Vui lòng thử lại sau");
                return View("Edit", model);
            }
        }
        /// <summary>
        /// Hiển thị và xóa khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            //Xóa khách hàng
            if (Request.Method == "POST")
            {
                CommonDataService.CustomerDB.Delete(id);
                return RedirectToAction("Index");
            }

            //Hiển thị thông tin khách hàng cần xóa
            Customer? model = CommonDataService.CustomerDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
