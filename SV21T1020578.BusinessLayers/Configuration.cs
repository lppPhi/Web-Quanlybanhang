using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1020578.BusinessLayers
{
    public class Configuration
    {
        /// <summary>
        /// Khởi tạo cấu hình liên quan đến chuỗi tham số kết nối CSDL
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Init (string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static string ConnectionString { get; private set; }
    }
}
