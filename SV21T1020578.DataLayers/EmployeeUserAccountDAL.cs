using Dapper;
using SV21T1020578.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DataLayers
{
    public class EmployeeUserAccountDAL : BaseDAL
    {
        public EmployeeUserAccountDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Kiểm tra thông tin đăng nhập của nhân viên đúng hay không.
        /// Trả về null nếu thông tin đăng nhập không đúng
        /// </summary>
        /// <param name="username">Tên đăng nhập (email của nhân viên)</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserAccount? Authentiate(string username, string password)
        {
            UserAccount? result = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT  EmployeeID AS UserID,
                                    Email AS UserName,
                                    FullName,
                                    Email,
                                    Photo,
                                    RoleNames
                            FROM    Employees
                            WHERE   Email = @UserName AND Password = @Password";
                var parameters = new
                {
                    UserName = username ?? "",
                    Password = password ?? ""
                };
                result = connection.QueryFirstOrDefault<UserAccount>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
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
                var sql = @"UPDATE  Employees
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

        public bool IsEmailExist(string email)
        {
            throw new NotImplementedException();
        }

        public bool Register(string username, string password, string displayName, Customer customer)
        {
            throw new NotImplementedException();
        }
    }
}

