using Dapper;
using SV21T1020578.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DataLayers
{
    public class CustomerAccountDAL : BaseDAL
    {
        public CustomerAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public UserAccount? Authentiate(string username, string password)
        {
            UserAccount? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select CustomerID as UserId,
                                Email as UserName,
                                CustomerName as DisplayName,
                                N'' as Photo,
                                N'' as RoleNames
                            from Customers                      
                            where Email=@Email and Password = @Password";
                var parameters = new
                {
                    Email = username,
                    Password = password
                };
                data = connection.QueryFirstOrDefault<UserAccount>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return data;
        }

        public bool ChangePassword(string username, string oldpassword, string newpassword)
        {
            bool result = false;
            // so sánh mật khẩu cũ trong database
            UserAccount userAccount = Authentiate(username, oldpassword);
            if (userAccount == null) return result;
            using (var connection = OpenConnection())
            {
                // cập nhật mật khẩu mới
                var sql = @"UPDATE  Customers
                    SET     Password = @Password
                    WHERE   Email = @Email";

                var parameters = new
                {
                    Email = username,
                    Password = newpassword
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public bool Register(string username, string password, string displayName, Customer customer)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Customers (Email, Password, CustomerName, ContactName, Province, Address, Phone)
                    VALUES (@Email, @Password, @CustomerName, @ContactName, @Province, @Address, @Phone)";

                var parameters = new
                {
                    Email = username,
                    Password = password,
                    CustomerName = customer.CustomerName,
                    ContactName = customer.ContactName,
                    Province = customer.Province,
                    Address = customer.Address,
                    Phone = customer.Phone
                };

                result = connection.Execute(sql: sql, param: parameters, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
        public bool IsEmailExist(string email)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT COUNT(1) 
                            FROM Customers 
                            WHERE Email = @Email";
                var parameters = new { Email = email };
                int count = connection.QuerySingle<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
                return count > 0;  // Nếu đếm được ít nhất một bản ghi, email đã tồn tại
            }
        }

    }
}

