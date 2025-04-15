using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class SupplierController : Controller
    {
        private const string SUPPLIER_SEARCH_INPUT = "SupplierSearchInput";

        /// <summary>
        /// Tìm kiếm, hiển thị nhà cung cấp dưới dạng phân trang
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //kiểm tra trong session có điều kiện tìm kiếm được lưu lại hay không.
            //Nếu có thì sử dụng, nếu không thì tạo mới điều kiện tìm kiếm

            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH_INPUT);
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
            PaginationSearchOutput<Supplier> model = new PaginationSearchOutput<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = CommonDataService.SupplierDB.Count(input.SearchValue ?? ""),
                Data = CommonDataService.SupplierDB.List(input.Page, input.PageSize, input.SearchValue ?? "")
            };
            //Lưu lại điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_INPUT, input);
            return View(model);
        }
        /// <summary>
        /// Hiển thị giao diện để cập nhật thông tin của nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";
            //Lấy thông tin của nhà cung cấp cần cập nhật
            Supplier? model = CommonDataService.SupplierDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        /// <summary>
        /// Lưu dữ liệu nhà cung cấp vào CSDL
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveData(Supplier model)
        {
            try
            {
                ViewBag.Title = model.SupplierID == 0 ? "Bổ sung nhà cung cấp" : "Cập nhật thông tin nhà cung cấp";

                //Kiểm tra dữ liệu đầu vào, nếu dữ liệu không hợp lệ thì đưa các thông báo "lỗi"
                //vào trong ModelState
                if (string.IsNullOrWhiteSpace(model.SupplierName))
                    ModelState.AddModelError(nameof(model.SupplierName), "*");

                if (string.IsNullOrWhiteSpace(model.ContactName))
                    ModelState.AddModelError(nameof(model.ContactName), "*");

                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "*");
                else if (CommonDataService.SupplierDB.ExistsEmail(model.SupplierID, model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email bị trùng");

                if (string.IsNullOrWhiteSpace(model.Province))
                    ModelState.AddModelError(nameof(model.Province), "*");

                //Dựa vào thuộc tính IsValid của ModelState để biết có tồn tại trường hợp "lỗi" nào không?
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                if (model.SupplierID == 0)
                {
                    CommonDataService.SupplierDB.Add(model);
                }
                else
                {
                    CommonDataService.SupplierDB.Update(model);
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
        /// Hiển thị và xóa nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            //Xóa nhà cung cấp
            if (Request.Method == "POST")
            {
                CommonDataService.SupplierDB.Delete(id);
                return RedirectToAction("Index");
            }

            //Hiển thị thông tin nhà cung cấp cần xóa
            Supplier? model = CommonDataService.SupplierDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
