using Microsoft.AspNetCore.Mvc.Rendering;
using SV21T1020578.BusinessLayers;

namespace SV21T1020578.Admin
{
    public static class SelectListHeper
    {
        /// <summary>
        /// Danh sách tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> Provinces()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Chọn Tỉnh/Thành --"
            });
            foreach(var p in CommonDataService.ProvinceDB.List()){
                list.Add(new SelectListItem()
                {
                    Value = p.ProvinceName,
                    Text = p.ProvinceName
                });
            }

            return list;
        }

        public static List<SelectListItem> Categories()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Chọn loại hàng --"
            });
            foreach (var p in CommonDataService.CategoryDB.List())
            {
                list.Add(new SelectListItem()
                {
                    Value = p.CategoryName,
                    Text = p.CategoryName
                });
            }

            return list;
        }
        public static List<SelectListItem> Suppliers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Chọn nhà cung cấp --"
            });
            foreach (var p in CommonDataService.SupplierDB.List())
            {
                list.Add(new SelectListItem()
                {
                    Value = p.SupplierName,
                    Text = p.SupplierName
                });
            }

            return list;
        }
        public static List<SelectListItem> Shippers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Chọn người giao hàng --"
            });
            foreach (var p in CommonDataService.ShipperDB.List())
            {
                list.Add(new SelectListItem()
                {
                    Value = p.ShipperID.ToString(),
                    Text = p.ShipperName
                });
            }

            return list;
        }

    }
}
