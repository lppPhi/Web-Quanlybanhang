using Dapper;
using SV21T1020578.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DataLayers
{
    public class ShipperDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trên mỗi trang (0 nếu không phân trang)</param>
        /// <param name="searchValue">Giá trị tìm kiếm (chuỗi rỗng nếu lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>
        public List<Shipper> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Shipper> data = new List<Shipper>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"WITH t AS
                            (
	                            SELECT	*,
			                            ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
	                            FROM	Shippers
	                            WHERE	ShipperName LIKE @SearchValue 
		                            OR	Phone LIKE @SearchValue
                            )
                            SELECT * FROM t 
                            WHERE	(@PageSize = 0)
	                            OR	(t.RowNumber BETWEEN (@Page -1 )*@PageSize + 1 AND @Page * @PageSize)
                            ORDER BY t.RowNumber";
                var parameters = new
                {
                    Page = page <= 0 ? 1 : page,
                    PageSize = pageSize < 0 ? 0 : pageSize,
                    SearchValue = searchValue ?? "%"
                };
                data = connection.Query<Shipper>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Đếm số lượng người giao hàng tìm được
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT	COUNT(*)
                            FROM	Shippers
                            WHERE	ShipperName LIKE @SearchValue 
	                            OR	Phone LIKE @SearchValue";
                var parameters = new
                {
                    SearchValue = searchValue ?? ""
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return count;
        }
        public Shipper? Get(int id)
        {
            Shipper? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Shippers WHERE ShipperId=@ShipperId";
                var parameters = new { ShipperId = id };
                data = connection.QueryFirstOrDefault<Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Bổ sung thêm một người giao hàng, trả về id của người giao hàng vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <summary>
        /// Bổ sung thêm một khách hàng, trả về id của khách hàng vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Add(Shipper data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Shippers
                            (
	                             ShipperName
	                            ,Phone
	                            
                            )
                            VALUES
                            (
	                            @ShipperName
	                            ,@Phone
	                            
                            );
                            SELECT SCOPE_IDENTITY()";
                var parameters = new
                {
                    ShipperName = data.ShipperName ?? "",
                    Phone = data.Phone ?? ""
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }
        /// <summary>
        /// Cập nhật thông tin của khách hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Update(Shipper data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE	Shippers
                            SET		ShipperName = @ShipperName
                                   ,Phone = @Phone
                                  
                             WHERE  ShipperId = @ShipperId";
                var parameters = new
                {
                    ShipperId = data.ShipperID,
                    ShipperName = data.ShipperName ?? "",
                    Phone = data.Phone ?? ""
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Xóa người giao hàng có mã là id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Shippers WHERE ShipperId = @ShipperId";
                var parameters = new { ShipperId = id };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Kiểm tra xem một người giao hàng có đơn hàng hay không?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Orders WHERE ShipperID = @ShipperId)
	                          BEGIN
		                        SELECT 1;
	                          END
                            ELSE
	                          BEGIN
		                        SELECT 0;
	                          END";
                var parameters = new { ShipperId = id };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }
    }
}
