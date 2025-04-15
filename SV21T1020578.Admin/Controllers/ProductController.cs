using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020578.Admin.Models;
using SV21T1020578.BusinessLayers;
using SV21T1020578.DomainModels;

namespace SV21T1020578.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH_INPUT = "ProductSearchInput";
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_INPUT);
            if (input == null)
            {

                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 5,
                    SearchValue = ""
                };

            }
            return View(input);
        }
        public IActionResult Search(ProductSearchOutput input)
        {
            ProductSearchOutput model = new ProductSearchOutput()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue,
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice,
                RowCount = CommonDataService.ProductDB.Count(input.SearchValue ?? "", input.CategoryID, input.SupplierID, input.MinPrice, input.MaxPrice),
                Data = CommonDataService.ProductDB.List(input.Page, input.PageSize, input.SearchValue ?? "", input.CategoryID, input.SupplierID, input.MinPrice, input.MaxPrice)
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_INPUT, input);
            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng mới";
            var model = new ProductEditModel
            {
                Product = new Product()
                {
                    ProductID = 0
                },
                ProductAttributes = new List<ProductAttribute>(),
                ProductPhotos = new List<ProductPhoto>()
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";

            // Lấy thông tin sản phẩm từ DB
            Product? product = CommonDataService.ProductDB.Get(id);
            if (product == null)
                return RedirectToAction("Index");

            // Lấy danh sách thuộc tính sản phẩm
            List<ProductAttribute> attributes = CommonDataService.ProductDB.ListAttributes(id);

            // Lấy danh sách ảnh sản phẩm
            List<ProductPhoto> photos = CommonDataService.ProductDB.ListPhotos(id);

            var model = new ProductEditModel
            {
                Product = product,
                ProductAttributes = attributes,
                ProductPhotos = photos
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult SaveData(ProductEditModel model, IFormFile uploadPhoto)
        {
            try
            {
                ViewBag.Title = model.Product.ProductID == 0 ? "Bổ sung mặt hàng mới" : "Cập nhật thông tin mặt hàng";

                // Kiểm tra dữ liệu hợp lệ
                if (string.IsNullOrWhiteSpace(model.Product.ProductName))
                    ModelState.AddModelError(nameof(model.Product.ProductName), "Tên không được để trống");
                if (model.Product.CategoryID == 0)
                    ModelState.AddModelError(nameof(model.Product.CategoryID), "Danh mục không được để trống");
                if (model.Product.SupplierID == 0)
                    ModelState.AddModelError(nameof(model.Product.SupplierID), "Nhà cung cấp không được để trống");
                if (string.IsNullOrWhiteSpace(model.Product.Unit))
                    ModelState.AddModelError(nameof(model.Product.Unit), "Đơn vị tính không được để trống");
                if (model.Product.Price < 0)
                    ModelState.AddModelError(nameof(model.Product.Price), "Vui lòng nhập giá hợp lệ");

                // Nếu có lỗi, quay lại form Edit
                if (!ModelState.IsValid)
                {
                    model.ProductAttributes = CommonDataService.ProductDB.ListAttributes(model.Product.ProductID);
                    model.ProductPhotos = CommonDataService.ProductDB.ListPhotos(model.Product.ProductID);
                    return View("Edit", model);
                }

                if (uploadPhoto != null)
                {
                string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WebRootPath, @"images\products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }
                model.Product.Photo = fileName;
                }

                if (model.Product.ProductID == 0)
                {
                    CommonDataService.ProductDB.Add(model.Product);
                }
                else
                {
                    CommonDataService.ProductDB.Update(model.Product);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                ModelState.AddModelError("Error", "Hệ thống tạm thời quá tải hoặc gián đoạn. Vui lòng thử lại sau.");

                model.ProductAttributes = CommonDataService.ProductDB.ListAttributes(model.Product.ProductID);
                model.ProductPhotos = CommonDataService.ProductDB.ListPhotos(model.Product.ProductID);

                return View("Edit", model);
            }
        }




        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.ProductDB.Delete(id);
                return RedirectToAction("Index");
            }

            Product? model = CommonDataService.ProductDB.Get(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        public IActionResult Photo(int id = 0, string method = "", int photoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";

                    var model = new ProductPhoto
                    {
                        ProductID = id
                    };

                    return View(model);

                case "edit":
                    ViewBag.Title = "Cập nhật ảnh";
                    var photo = CommonDataService.ProductDB.GetPhoto(photoId);
                    if (photo == null)
                        return RedirectToAction("Edit", new { id = id });
                    return View(photo);

                case "delete":
                    CommonDataService.ProductDB.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id = id });

                default:
                    return RedirectToAction("Edit", new { id = id });
            }
        }

        [HttpPost]
        public IActionResult SavePhoto(ProductPhoto model, IFormFile uploadPhoto)
        {
            try
            {
                ViewBag.Title = model.PhotoID == 0 ? "Bổ sung ảnh" : "Cập nhật ảnh";

                if (model.ProductID == 0)
                    return RedirectToAction("Index");

                if (string.IsNullOrWhiteSpace(model.Description))
                    ModelState.AddModelError(nameof(model.Description), "Mô tả không được để trống");
                if (model.DisplayOrder <= 0)
                    ModelState.AddModelError(nameof(model.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");

                if (!ModelState.IsValid)
                {
                    return View("Photo", model);
                }

                if (uploadPhoto != null)
                {
                    string uploadsFolder = Path.Combine(ApplicationContext.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    model.Photo = $"{fileName}";
                }

                if (model.PhotoID == 0)
                {
                    CommonDataService.ProductDB.AddPhoto(model);
                }
                else
                {
                    CommonDataService.ProductDB.UpdatePhoto(model);
                }

                return RedirectToAction("Edit", new { id = model.ProductID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                ModelState.AddModelError("Error", "Hệ thống tạm thời quá tải hoặc gián đoạn. Vui lòng thử lại sau.");
                return View("Photo", model);
            }
        }

        [HttpPost]
        public IActionResult SaveAttribute(ProductAttribute data)
        {
            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị thuộc tính không được để trống");
            if (data.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự không hợp lệ");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.AttributeID == 0 ? "Bổ sung thuộc tính của mặt hàng" : "Cập nhật thuộc tính của mặt hàng";
                return View("Attribute", data);
            }

            if (data.AttributeID == 0)
            {
                long id = CommonDataService.ProductDB.AddAttribute(data);
                if (id > 0)
                    return RedirectToAction("Edit", new { id = data.ProductID });
            }
            else
            {
                bool result = CommonDataService.ProductDB.UpdateAttribute(data);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không thể cập nhật thuộc tính. Vui lòng thử lại.");
                    return View("Attribute", data);
                }
            }

            return RedirectToAction("Edit", new { id = data.ProductID });
        }


        public IActionResult Attribute(int id = 0, string method = "", int attributeId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính";
                    var data = new ProductAttribute
                    {
                        AttributeID = 0,
                        DisplayOrder = 1,
                        ProductID = id
                    };
                    return View(data);

                case "edit":
                    ViewBag.Title = "Cập nhật thuộc tính";
                    var attribute = CommonDataService.ProductDB.GetAttribute(attributeId);
                    if (attribute == null)
                        return RedirectToAction("Edit", new { id = id });
                    return View(attribute);

                case "delete":
                    if (attributeId > 0)
                    {
                        CommonDataService.ProductDB.DeleteAttribute(attributeId);
                    }
                    return RedirectToAction("Edit", new { id = id });

                default:
                    return RedirectToAction("Edit", new { id = id });
            }
        }


    }
}
