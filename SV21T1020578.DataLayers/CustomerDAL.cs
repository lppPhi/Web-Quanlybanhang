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
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến khách hàng
    /// </summary>
    public class CustomerDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public CustomerDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trên mỗi trang (0 nếu không phân trang)</param>
        /// <param name="searchValue">Giá trị tìm kiếm (chuỗi rỗng nếu lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>
        public List<Customer> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Customer> data = new List<Customer>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"WITH t AS
                            (
	                            SELECT	*,
			                            ROW_NUMBER() OVER(ORDER BY CustomerName) AS RowNumber
	                            FROM	Customers
	                            WHERE	CustomerName LIKE @SearchValue 
		                            OR	ContactName LIKE @SearchValue
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
                data = connection.Query<Customer>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Đếm số lượng khách hàng tìm được
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
                            FROM	Customers
                            WHERE	CustomerName LIKE @SearchValue 
	                            OR	ContactName LIKE @SearchValue";
                var parameters = new
                {
                    SearchValue = searchValue ?? ""
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return count;
        }
        /// <summary>
        /// Lấy thông tin khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Customer? Get(int id)
        {
            Customer? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Customers WHERE CustomerId=@CustomerId";
                var parameters = new { CustomerId = id };
                data = connection.QueryFirstOrDefault<Customer>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Bổ sung thêm một khách hàng, trả về id của khách hàng vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Add(Customer data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Customers
                            (
	                             CustomerName
	                            ,ContactName
	                            ,Province
	                            ,Address
	                            ,Phone
	                            ,Email
	                            ,IsLocked
                            )
                            VALUES
                            (
	                            @CustomerName
	                            ,@ContactName
	                            ,@Province
	                            ,@Address
	                            ,@Phone
	                            ,@Email	
	                            ,@IsLocked
                            );
                            SELECT SCOPE_IDENTITY()";
                var parameters = new
                {
                    CustomerName = data.CustomerName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    IsLocked = data.IsLocked
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
        public bool Update(Customer data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE	Customers
                            SET		CustomerName = @CustomerName
                                  ,ContactName = @ContactName
                                  ,Province = @Province
                                  ,Address = @Address
                                  ,Phone = @Phone
                                  ,Email = @Email
                                  ,IsLocked = @IsLocked
                             WHERE CustomerId = @CustomerId";
                var parameters = new
                {
                    CustomerId = data.CustomerId,
                    CustomerName = data.CustomerName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    IsLocked = data.IsLocked
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Xóa khách hàng có mã là id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Customers WHERE CustomerId = @CustomerId";
                var parameters = new { CustomerId = id };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Kiểm tra xem một khách hàng có đơn hàng hay không?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Orders WHERE CustomerID = @CustomerId)
	                          BEGIN
		                        SELECT 1;
	                          END
                            ELSE
	                          BEGIN
		                        SELECT 0;
	                          END";
                var parameters = new { CustomerId = id };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }
        public bool ExistsEmail(int id, string email)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Customers WHERE CustomerId <> @id AND Email = @email)
                        BEGIN
                            SELECT 1
                        END
                    ELSE
                        BEGIN
                            SELECT 0;
                        END";
                var parameters = new { id, email };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
        }


    }
}
