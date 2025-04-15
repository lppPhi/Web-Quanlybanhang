using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.DomainModels
{
    public class UserAccount
    {
        /// <summary>
        /// ID tài khoản
        /// </summary>
        public string UserID { get; set; } = "";
        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// Tên đầy đủ (tên hiển thị)
        /// </summary>
        public string FullName { get; set; } = "";
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = "";
        /// <summary>
        /// Tên file ảnh (nếu có)
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Chuỗi các quyền của tài khoản, phân cách bởi dấu phẩy
        /// </summary>
        public string RoleNames { get; set; } = "";
    }
}
