using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}")]
    public class ShipperController : Controller
    {
        public const int PAGE_SIZE = 10; //Số dòng hiển thị mặc định trên một trang

        /// <summary>
        /// Tìm kiếm, hiển thị người giao hàng dưới dạng phân trang
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(int page = 1, string searchValue = "")
        {
            PaginationSearchOutput<Shipper> model = new PaginationSearchOutput<Shipper>()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue ?? "",
                RowCount = CommonDataService.ShipperDB.Count(searchValue ?? ""),
                Data = CommonDataService.ShipperDB.List(page, PAGE_SIZE, searchValue ?? "")
            };
            return View(model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            //Tạo một đối tượng để lưu thông tin của nhà cung cấp mới
            Shipper model = new Shipper()
            {
                ShipperID = 0,
            };
            return View("Edit", model);
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
            Shipper? model = CommonDataService.ShipperDB.Get(id);
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
        public IActionResult SaveData(Shipper model)
        {
            try
            {
                ViewBag.Title = model.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật thông tin người giao hàng";

                //Kiểm tra dữ liệu đầu vào, nếu dữ liệu không hợp lệ thì đưa các thông báo "lỗi"
                //vào trong ModelState
                if (string.IsNullOrWhiteSpace(model.ShipperName))
                    ModelState.AddModelError(nameof(model.ShipperName), "*");

                if (string.IsNullOrWhiteSpace(model.Phone))
                    ModelState.AddModelError(nameof(model.Phone), "*");

                //Dựa vào thuộc tính IsValid của ModelState để biết có tồn tại trường hợp "lỗi" nào không?
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                if (model.ShipperID == 0)
                {
                    CommonDataService.ShipperDB.Add(model);
                }
                else
                {
                    CommonDataService.ShipperDB.Update(model);
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
                CommonDataService.ShipperDB.Delete(id);
                return RedirectToAction("Index");
            }

            //Hiển thị thông tin nhà cung cấp cần xóa
            Shipper? model = CommonDataService.ShipperDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
    }
}
