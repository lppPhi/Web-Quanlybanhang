using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class CategoryController : Controller
    {

        private const string CATEGORY_SEARCH_INPUT = "CategorySearchInput";
        /// <summary>
        /// Tìm kiếm, hiển thị loại hàng dưới dạng phân trang
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //kiểm tra trong session có điều kiện tìm kiếm được lưu lại hay không.
            //Nếu có thì sử dụng, nếu không thì tạo mới điều kiện tìm kiếm

            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_INPUT);
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
            PaginationSearchOutput<Category> model = new PaginationSearchOutput<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = CommonDataService.CategoryDB.Count(input.SearchValue ?? ""),
                Data = CommonDataService.CategoryDB.List(input.Page, input.PageSize, input.SearchValue ?? "")
            };
            //Lưu lại điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_INPUT, input);
            return View(model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            //Tạo một đối tượng để lưu thông tin của loại hàng mới
            Category model = new Category()
            {
                CategoryID = 0,

            };
            return View("Edit", model);
        }
        /// <summary>
        /// Hiển thị giao diện để cập nhật thông tin của loại hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin loại hàng";
            //Lấy thông tin của loại hàng cần cập nhật
            Category? model = CommonDataService.CategoryDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        /// <summary>
        /// Lưu dữ liệu loại hàng vào CSDL
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveData(Category model)
        {
            try
            {
                ViewBag.Title = model.CategoryID == 0 ? "Bổ sung loại hàng" : "Cập nhật thông tin loại hàng";

                //Kiểm tra dữ liệu đầu vào, nếu dữ liệu không hợp lệ thì đưa các thông báo "lỗi"
                //vào trong ModelState
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                    ModelState.AddModelError(nameof(model.CategoryName), "*");

                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError(nameof(model.Description), "*");


                //Dựa vào thuộc tính IsValid của ModelState để biết có tồn tại trường hợp "lỗi" nào không?
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                if (model.CategoryID == 0)
                {
                    CommonDataService.CategoryDB.Add(model);
                }
                else
                {
                    CommonDataService.CategoryDB.Update(model);
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
        /// Hiển thị và xóa loại hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            //Xóa loại hàng
            if (Request.Method == "POST")
            {
                CommonDataService.CategoryDB.Delete(id);
                return RedirectToAction("Index");
            }

            //Hiển thị thông tin loại hàng cần xóa
            Category? model = CommonDataService.CategoryDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

    }
}
