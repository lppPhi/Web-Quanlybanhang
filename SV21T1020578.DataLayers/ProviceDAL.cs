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
    /// Lớp cung cấp các phép xử lý dữ liệu liên quan đến tỉnh/thành
    /// </summary>
    public class ProvinceDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Lấy danh sách tất cả các tỉnh thành
        /// </summary>
        /// <returns></returns>
        public List<Province> List()
        {
            List<Province> data = new List<Province>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Provinces";
                var parameters = new { };
                data = connection.Query<Province>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
        }

    }
}
