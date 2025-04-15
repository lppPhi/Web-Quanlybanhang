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
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến loại hàng
    /// </summary>
    public class CategoryDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách loại hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trên mỗi trang (0 nếu không phân trang)</param>
        /// <param name="searchValue">Giá trị tìm kiếm (chuỗi rỗng nếu lấy toàn bộ dữ liệu)</param>
        /// <returns></returns>
        public List<Category> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Category> data = new List<Category>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"WITH t AS
                            (
	                            SELECT	*,
			                            ROW_NUMBER() OVER(ORDER BY CategoryName) AS RowNumber
	                            FROM	Categories
	                            WHERE	CategoryName LIKE @SearchValue 
		                            OR	Description LIKE @SearchValue
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
                data = connection.Query<Category>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Đếm số lượng loại hàng tìm được
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
                            FROM	Categories
                            WHERE	CategoryName LIKE @SearchValue 
	                            OR	Description LIKE @SearchValue";
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
        /// Lấy thông tin của một loại hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category? Get(int id)
        {
            Category? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Categories WHERE CategoryID=@CategoryID";
                var parameters = new { CategoryID = id };
                data = connection.QueryFirstOrDefault<Category>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Bổ sung thêm một loại hàng, trả về id của loại hàng vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Add(Category data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Categories 
                               ( 
                                   CategoryName,
                                   Description                                  
                                )
                           VALUES
                               (
                                   @CategoryName, 
                                   @Description
		                       );
                            SELECT SCOPE_IDENTITY()";
                var parameters = new
                {
                    CategoryName = data.CategoryName ?? "",
                    Description = data.Description ?? "",
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }
        /// <summary>
        /// Cập nhật thông tin của loại hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Update(Category data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE	Categories
                            SET		CategoryName = @CategoryName
                                   ,Description = @Description
                            WHERE   CategoryId = @CategoryId";
                var parameters = new
                {
                    CategoryId = data.CategoryID,
                    CategoryName = data.CategoryName ?? "",
                    Description = data.Description ?? "",
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Xóa loại hàng có mã là id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Categories WHERE CategoryId = @CategoryId";
                var parameters = new { CategoryId = id };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Kiểm tra xem một loại hàng có đơn hàng hay không?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM Products     WHERE CategoryID = @CategoryId)
	                          BEGIN
		                        SELECT 1;
	                          END
                            ELSE
	                          BEGIN
		                        SELECT 0;
	                          END";
                var parameters = new { CategoryId = id };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }
    }
}
