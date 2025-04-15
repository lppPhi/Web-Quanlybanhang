using SV21T1020578.DataLayers;
using SV21T1020578.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.BusinessLayers
{
    /// <summary>
    /// Nghiệp vụ bán hàng
    /// </summary>
     public class SaleDataService
    {
        static SaleDataService()
        {
            string connectionString = Configuration.ConnectionString;
            OrderDB = new OrderDAL(connectionString);
        }
        private static OrderDAL OrderDB { get;  set; }

        public static List<Order> ListOrders(out int rowCount, int page = 1, int pageSize = 0, int status = 0, DateTime? formTime = null, DateTime? toTime = null, string searchValue = "")
        {
            rowCount = OrderDB.Count(status, formTime, toTime, searchValue);
            return OrderDB.List(page, pageSize, status, formTime, toTime, searchValue);
        }

        public static List<Order> ListOrdersByCustomer(out int rowCount, int customerID, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "", int page = 1, int pageSize = 10)
        {
            // Lấy danh sách tất cả các đơn hàng phù hợp với điều kiện
            var query = OrderDB.List(1, 0, status, fromTime, toTime, searchValue)
                                .Where(o => o.CustomerID == customerID); // Lọc theo CustomerID của khách hàng

            // Tính tổng số lượng đơn hàng (rowCount)
            rowCount = query.Count(); // Đếm tổng số đơn hàng của khách hàng

            // Áp dụng phân trang
            var pagedOrders = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(); // Lấy đúng số đơn hàng cho trang hiện tại

            // Trả về danh sách đơn hàng đã phân trang
            return pagedOrders;
        }
        public static Order GetOrder(int orderID)
        {
            return OrderDB.Get(orderID);
        }
        /// <summary>
        /// Bổ sung một đơn hàng. Hàm trả về ID của đơn hàng được tạo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int AddOrder(Order data)
        {
            return OrderDB.Add(data);
        }
        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public static bool AcceptOrder(int orderID, int employeeID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if (order.Status != Constants.ORDER_INIT) return false;

            order.Status = Constants.ORDER_ACCEPTED;
            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            return OrderDB.Update(order);

        }
        /// <summary>
        /// Xác nhận giao hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public static bool ShipOrder(int orderID, int shipperID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if (order.Status == Constants.ORDER_ACCEPTED || order.Status == Constants.ORDER_SHIPPING)
            {
                order.Status = Constants.ORDER_SHIPPING;
                order.ShipperID = shipperID;
                order.ShippedTime = DateTime.Now;
                return OrderDB.Update(order);
            }

            return false;
        }

        /// <summary>
        /// Hoàn thành giao hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static bool FinishOrder(int orderID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;
            if(order.Status == Constants.ORDER_SHIPPING || order.Status == Constants.ORDER_ACCEPTED)
            {
                order.Status = Constants.ORDER_FINISHED;
                order.FinishedTime = DateTime.Now;
                return OrderDB.Update(order);
            }
            return false;
        }
        /// <summary>
        /// Huỷ đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public static bool CancelOrder(int orderID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if(order.Status == Constants.ORDER_FINISHED)
            {
                return false;
            }
            order.Status = Constants.ORDER_CANCEL;
            return OrderDB.Update(order);
        }
        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public static bool RejectOrder(int orderID, int employeeID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if (order.Status != Constants.ORDER_INIT)
            {
                return false;
            }
            order.Status = Constants.ORDER_REJECTED;
            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            return OrderDB.Update(order);
        }

        public static bool DeleteOrder(int orderID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if(order.Status == Constants.ORDER_CANCEL || order.Status == Constants.ORDER_REJECTED)
            {
                return OrderDB.Delete(orderID);

            }
            return false;
        }

        public static List<OrderDetail> ListOrderDetails(int orderID)
        {
            return OrderDB.ListDetails(orderID).ToList();
        }

        public static OrderDetail? GetOrderDetail(int orderID, int productID)
        {
            return OrderDB.GetDetail(orderID, productID);
        }

        public static bool SaveOrderDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if(order.Status == Constants.ORDER_INIT || order.Status == Constants.ORDER_ACCEPTED)
            {
                return OrderDB.SaveDetail(orderID, productID, quantity, salePrice);
            }

            return false;
        }

        public static bool DeleteOrderDetail(int orderID, int productID)
        {
            Order? order = OrderDB.Get(orderID);
            if (order == null) return false;

            if (order.Status == Constants.ORDER_INIT || order.Status == Constants.ORDER_ACCEPTED)
            {
                return OrderDB.DeleteDetail(orderID, productID);
            }

            return false;

        }

        public static int InitOrderCustomer(int customerID,

        string deliveryProvince, string deliveryAddress,
        IEnumerable<OrderDetail> details)

        {
            if (details.Count() == 0)
                return 0;
            Order data = new Order()
            {
                CustomerID = customerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress
            };
            int orderID = OrderDB.Add(data);
            if (orderID > 0)
            {
                foreach (var item in details)
                {
                    OrderDB.SaveDetail(orderID, item.ProductID, item.Quantity, item.SalePrice);
                }
                return orderID;
            }
            return 0;
        }

    }
}
