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
    /// Các dịch vụ liên quan đến tài khoản
    /// </summary>
    public static class UserAccountService
    {
        /// <summary>
        /// 
        /// </summary>
        static UserAccountService()
        {
            //string connectionString = @"server=.;user id=sa;password=123456;database=LiteCommerceDB;TrustServerCertificate=true";
            string connectionString = Configuration.ConnectionString;
            EmployeeAccountDB = new EmployeeUserAccountDAL(connectionString);
            CustomerAccountDB = new CustomerAccountDAL(connectionString);
        }
        /// <summary>
        /// Tài khoản của nhân viên
        /// </summary>
        public static EmployeeUserAccountDAL EmployeeAccountDB { get; private set; }
        public static CustomerAccountDAL CustomerAccountDB { get; private set; }

        public static UserAccount? Authentiate(UserTypes userType, string username, string password)
        {
            if (userType == UserTypes.Employee)
                return EmployeeAccountDB.Authentiate(username, password);
            else
                return CustomerAccountDB.Authentiate(username, password);
        }
        public static bool ChangePassword(UserTypes userType, string username, string oldpassword, string newpassword)
        {
            if (userType == UserTypes.Employee)
                return EmployeeAccountDB.ChangePassword(username, oldpassword, newpassword);
            else
                return CustomerAccountDB.ChangePassword(username, oldpassword, newpassword);
        }
        public static bool Register(UserTypes userType, string username, string password, string displayName, Customer customer)
        {
            if (userType == UserTypes.Employee)
                return EmployeeAccountDB.Register(username, password, displayName, customer);
            else
                return CustomerAccountDB.Register(username, password, displayName, customer);
        }
        public static bool IsEmailExist(string email)
        {
            return CustomerAccountDB.IsEmailExist(email);
        }
        public enum UserTypes
        {
            Employee,
            Customer
        }
    }
}
