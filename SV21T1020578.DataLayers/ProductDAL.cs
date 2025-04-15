using Azure;
using Dapper;
using SV21T1020578.DomainModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV21T1020578.DataLayers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductDAL : BaseDAL
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <param name="categoryID"></param>
        /// <param name="supplierID"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        public List<Product> List(int page = 1, int pageSize = 0,
                                    string searchValue = "", int categoryID = 0, int supplierID = 0,
                                    decimal minPrice = 0, decimal maxPrice = 0)
        {
            List<Product> list = new List<Product>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"with cte as
                            (
                                select  *,
                                        row_number() over(order by ProductName) as RowNumber
                                from    Products 
                                where   (ProductName like @SearchValue)
                                    and (@CategoryID = 0 or CategoryID = @CategoryID)
                                    and (@SupplierID = 0 or SupplierId = @SupplierID)
                                    and (Price >= @MinPrice)
                                    and (@MaxPrice <= 0 or Price <= @MaxPrice)
                            )
                            select * from cte 
                            where   (@PageSize = 0) 
                                or (RowNumber between (@Page - 1)*@PageSize + 1 and @Page * @PageSize)";
                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = searchValue ?? "",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                list = connection.Query<Product>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchValue"></param>
        /// <param name="categoryID"></param>
        /// <param name="supplierID"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"select  count(*)
                            from    Products 
                            where   (@SearchValue = N'' or ProductName like @SearchValue)
                                and (@CategoryID = 0 or CategoryID = @CategoryID)
                                and (@SupplierID = 0 or SupplierId = @SupplierID)
                                and (Price >= @MinPrice)
                                and (@MaxPrice <= 0 or Price <= @MaxPrice)";
                var parameters = new
                {
                    SearchValue = searchValue ?? "",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return count;
        }
        public int CountByCategory(int categoryId)
        {
            using (var connection = OpenConnection())
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Products WHERE CategoryID = @categoryId";
                cmd.Parameters.AddWithValue("@categoryId", categoryId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public Product? Get(int id)
        {
            Product? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Products WHERE ProductId=@ProductId";
                var parameters = new { ProductId = id };
                data = connection.QueryFirstOrDefault<Product>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// Bổ sung thêm một khách hàng, trả về id của khách hàng vừa bổ sung
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Add(Product data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                         VALUES(@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                         SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    data.ProductName,
                    data.ProductDescription,
                    data.SupplierID,
                    data.CategoryID,
                    data.Unit,
                    data.Price,
                    data.Photo,
                    data.IsSelling,
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
        public bool Update(Product data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE	Products
                            SET		ProductName = @ProductName
	                            ,ProductDescription = @ProductDescription
	                            ,CategoryId = @CategoryId
	                            ,SupplierId = @SupplierId
	                            ,Unit = @Unit
	                            ,Price = @Price
                                ,Photo = @Photo
	                            ,IsSelling = @IsSelling
                             WHERE ProductId = @ProductId";
                var parameters = new
                {   
                    data.ProductID,
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    data.CategoryID,
                    data.SupplierID,
                    data.Unit,
                    data.Price,
                    data.Photo,
                    data.IsSelling
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
                var sql = @"DELETE FROM Products WHERE ProductID = @ProductId";
                var parameters = new { ProductId = id };
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
                var sql = @"IF EXISTS(SELECT * FROM Suppliers WHERE ProductID = @ProductId)
	                          BEGIN
		                        SELECT 1;
	                          END
                            ELSE
	                          BEGIN
		                        SELECT 0;
	                          END";
                var parameters = new { ProductId = id };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ProductAttribute> ListAttributes(int productID)
        {
            List<ProductAttribute> data = [];
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder ASC";
                var parameters = new
                {
                    ProductID = productID
                };
                data = connection.Query<ProductAttribute>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ProductAttribute? GetAttribute(long attributeID)
        {
            ProductAttribute? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";
                var parameters = new
                {
                    AttributeID = attributeID
                };
                data = connection.QueryFirstOrDefault<ProductAttribute>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public long AddAttribute(ProductAttribute data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                    VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                var parameters = new
                {
                    data.ProductID,
                    data.AttributeName,
                    data.AttributeValue,
                    data.DisplayOrder,
                };
                id = connection.ExecuteScalar<long>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE ProductAttributes
                         SET ProductID = @ProductID,
                             AttributeName = @AttributeName,
                             AttributeValue = @AttributeValue,
                             DisplayOrder = @DisplayOrder
                         WHERE AttributeID = @AttributeID";
                var parameters = new
                {
                    data.ProductID,
                    data.AttributeName,
                    data.AttributeValue,
                    data.DisplayOrder,
                    data.AttributeID,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool DeleteAttribute(long attributeID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
                var parameters = new { AttributeID = attributeID };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public long GetConflictingAttributeID(int productID, int displayOrder, long attributeID)
        {
            long conflictingAttributeID = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS ( SELECT AttributeID FROM ProductAttributes
                                     WHERE ProductID = @ProductID
                                         AND DisplayOrder = @DisplayOrder
                                         AND AttributeID <> @AttributeID)
                             SELECT AttributeID FROM ProductAttributes
                             WHERE ProductID = @ProductID
                             AND DisplayOrder = @DisplayOrder
                         ELSE
                             SELECT 0";
                var parameters = new
                {
                    ProductID = productID,
                    DisplayOrder = displayOrder,
                    AttributeID = attributeID,
                };
                conflictingAttributeID = connection.ExecuteScalar<long>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return conflictingAttributeID;
        }
        public bool ExistsAttribute(int ProductID, string AttributeName, string AttributeValue, long AttributeID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS ( SELECT AttributeID FROM ProductAttributes
                                     WHERE ProductID = @ProductID
                                         AND AttributeName = @AttributeName
                                         AND AttributeValue = @AttributeValue
                                         AND AttributeID <> @AttributeID)
                             SELECT 1
                         ELSE
                             SELECT 0";
                var parameters = new
                {
                    ProductID,
                    AttributeName,
                    AttributeValue,
                    AttributeID,
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ProductPhoto> ListPhotos(int productID)
        {
            List<ProductPhoto> data = [];
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder ASC";
                var parameters = new
                {
                    ProductID = productID
                };
                data = connection.Query<ProductPhoto>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ProductPhoto? GetPhoto(long photoID)
        {
            ProductPhoto? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";
                var parameters = new
                {
                    PhotoID = photoID
                };
                data = connection.QueryFirstOrDefault<ProductPhoto>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public long AddPhoto(ProductPhoto data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                         VALUES(@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                         SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    data.ProductID,
                    data.Photo,
                    data.Description,
                    data.DisplayOrder,
                    data.IsHidden,
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }

            return id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE ProductPhotos
                         SET ProductID = @ProductID,
                             Photo = @Photo,
                             Description = @Description,
                             DisplayOrder = @DisplayOrder,
                             IsHidden = @IsHidden
                         WHERE PhotoID = @PhotoID";
                var parameters = new
                {
                    data.ProductID,
                    data.Photo,
                    data.Description,
                    data.DisplayOrder,
                    data.IsHidden,
                    data.PhotoID,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="photoID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool DeletePhoto(long photoID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
                var parameters = new { PhotoID = photoID };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        public bool ExistsDisplayOrderPhoto(int ProductID, int DisplayOrder, long PhotoID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS ( SELECT PhotoID FROM ProductPhotos
                                     WHERE ProductID = @ProductID
                                         AND DisplayOrder = @DisplayOrder
                                         AND PhotoID <> @PhotoID)
                             SELECT 1
                         ELSE
                             SELECT 0";
                var parameters = new
                {
                    ProductID,
                    DisplayOrder,
                    PhotoID,
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public long GetConflictingPhotoID(int productID, int displayOrder, long photoID)
        {
            long conflictingPhotoID = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS ( SELECT PhotoID FROM ProductPhotos
                                     WHERE ProductID = @ProductID
                                         AND DisplayOrder = @DisplayOrder
                                         AND PhotoID <> @PhotoID)
                             SELECT PhotoID FROM ProductPhotos
                             WHERE ProductID = @ProductID
                             AND DisplayOrder = @DisplayOrder
                         ELSE
                             SELECT 0";
                var parameters = new
                {
                    ProductID = productID,
                    DisplayOrder = displayOrder,
                    PhotoID = photoID,
                };
                conflictingPhotoID = connection.ExecuteScalar<long>(sql: sql, param: parameters, commandType: CommandType.Text);
                connection.Close();
            }
            return conflictingPhotoID;
        }

    }
}
