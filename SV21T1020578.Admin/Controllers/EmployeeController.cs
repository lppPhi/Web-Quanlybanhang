using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class EmployeeController : Controller
    {
        private const string EMPLOYEE_SEARCH_INPUT = "EmployeeSearchInput";

        /// <summary>
        /// Tìm kiếm, hiển thị nhân viên dưới dạng phân trang
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(int page = 1, string searchValue = "")
        {
            //kiểm tra trong session có điều kiện tìm kiếm được lưu lại hay không.
            //Nếu có thì sử dụng, nếu không thì tạo mới điều kiện tìm kiếm

            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_INPUT);
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
        public IActionResult Search(PaginationSearchInput input)
        {
            PaginationSearchOutput<Employee> model = new PaginationSearchOutput<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = CommonDataService.EmployeeDB.Count(input.SearchValue ?? ""),
                Data = CommonDataService.EmployeeDB.List(input.Page, input.PageSize, input.SearchValue ?? "")
            };
            //Lưu lại điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_INPUT, input);
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                Photo = "nophoto.png"
            };
            return View("Edit", model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = CommonDataService.EmployeeDB.Get(id);
            if (model == null)
                return Redirect("Index");
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveData(Employee model, string birthday, IFormFile? uploadPhoto)
        {
            //TODO: Kiểm tra tính hợp lệ của dữ liệu

            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(model.FullName), "Tên nhân viên không được để trống");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError(nameof(model.Address), "Địa chỉ nhân viên không được để trống");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Vui lòng nhập Email của nhân viên");
            if (string.IsNullOrWhiteSpace(birthday))
                ModelState.AddModelError(nameof(model.BirthDate), "Vui lòng nhập ngày sinh của nhân viên");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError(nameof(model.Phone), "Vui lòng nhập số điện thoại của nhân viên");
            //Xử lý cho ngày sinh
            DateTime? d = birthday.ToDateTime();
            if (d == null)
                ModelState.AddModelError(nameof(model.BirthDate), "Ngày sinh không hợp lệ");
            else
                model.BirthDate = d.Value;

            //Xử lý cho ảnh
            if (uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WebRootPath, @"images\employees", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }
                model.Photo = fileName;
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            if (model.EmployeeID == 0)
            {
                CommonDataService.EmployeeDB.Add(model);
            }
            else
            {
                CommonDataService.EmployeeDB.Update(model);
            }
            return Redirect("Index");
        }


        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.EmployeeDB.Delete(id);
                return RedirectToAction("Index");
            }

            Employee? model = CommonDataService.EmployeeDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
