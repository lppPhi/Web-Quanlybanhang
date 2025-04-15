using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DomainModels
{
    /// <summary>
    /// Thông tin đơn hàng
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// Thời điểm đặt hàng
        /// </summary>
        public DateTime OrderTime { get; set; }
        /// <summary>
        /// Thời điểm duyệt (chấp nhận) đơn hàng
        /// </summary>
        public DateTime? AcceptTime { get; set; }

        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int CustomerID { get; set; }
        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; } = "";
        /// <summary>
        /// Tên giao dịch của khách hàng
        /// </summary>
        public string CustomerContactName { get; set; } = "";
        /// <summary>
        /// Địa chỉ của khách hàng
        /// </summary>
        public string CustomerAddress { get; set; } = "";
        /// <summary>
        /// Điện thoại của khách hàng
        /// </summary>
        public string CustomerPhone { get; set; } = "";
        /// <summary>
        /// Email của khách hàng
        /// </summary>
        public string CustomerEmail { get; set; } = "";

        /// <summary>
        /// Tỉnh/thành nhận hàng
        /// </summary>
        public string DeliveryProvince { get; set; } = "";
        /// <summary>
        /// Địa chỉ nhận hàng
        /// </summary>
        public string DeliveryAddress { get; set; } = "";

        /// <summary>
        /// Mã nhân viên phụ trách đơn hàng
        /// </summary>
        public int? EmployeeID { get; set; }
        /// <summary>
        /// Tên nhân viên phụ trách đơn hàng
        /// </summary>
        public string EmployeeName { get; set; } = "";

        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int? ShipperID { get; set; }
        /// <summary>
        /// Tên người giao hàng
        /// </summary>
        public string ShipperName { get; set; } = "";
        /// <summary>
        /// Điện thoại người giao hàng
        /// </summary>
        public string ShipperPhone { get; set; } = "";
        /// <summary>
        /// Thời điểm nhận giao hàng
        /// </summary>
        public DateTime? ShippedTime { get; set; }

        /// <summary>
        /// Thời điểm kết thúc đơn hàng
        /// </summary>
        public DateTime? FinishedTime { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Mô tả trạng thái đơn hàng dựa trên mã trạng thái
        /// </summary>
        public string StatusDescription
        {
            get
            {
                switch (Status)
                {
                    case Constants.ORDER_INIT:
                        return "Đơn hàng mới. Đang chờ duyệt";
                    case Constants.ORDER_ACCEPTED:
                        return "Đơn đã chấp nhận. Đang chờ chuyển hàng";
                    case Constants.ORDER_SHIPPING:
                        return "Đơn hàng đang được giao";
                    case Constants.ORDER_FINISHED:
                        return "Đơn hàng đã hoàn tất";
                    case Constants.ORDER_CANCEL:
                        return "Đơn hàng đã bị hủy";
                    case Constants.ORDER_REJECTED:
                        return "Đơn hàng bị từ chối";
                    default:
                        return "";
                }
            }
        }
    }
}
