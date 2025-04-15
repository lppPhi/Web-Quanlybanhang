using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DomainModels
{
    /// <summary>
    /// Thông tin chi tiết của đơn hàng
    /// </summary>
    public class OrderDetail
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên hàng
        /// </summary>
        public string ProductName { get; set; } = "";
        /// <summary>
        /// Ảnh của hàng
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Số lượng bán
        /// </summary>
        public int Quantity { get; set; } = 0;
        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal SalePrice { get; set; } = 0;
        /// <summary>
        /// Thành tiền = Số lượng * Giá bán
        /// </summary>
        public decimal TotalPrice
        {
            get
            {
                return Quantity * SalePrice;
            }
        }
    }
}
