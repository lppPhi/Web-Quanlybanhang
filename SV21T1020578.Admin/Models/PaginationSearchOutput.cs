namespace SV21T1020578.Admin.Models
{
    /// <summary>
    /// Lưu trữ dữ liệu đầu ra của chức năng tìm kiếm, phân trang
    /// mà Action chuyển cho View
    /// </summary>
    /// <typeparam name="T">Kiểu của dữ liệu mà cần xử lý</typeparam>
    public class PaginationSearchOutput<T> where T : class
    {
        /// <summary>
        /// Trang được hiển thị
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Số dòng trên mỗi trang
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Giá trị tìm kiếm
        /// </summary>
        public string SearchValue { get; set; } = "";
        /// <summary>
        /// Số dòng tìm được
        /// </summary>
        public int RowCount { get; set; }
        /// <summary>
        /// Số trang
        /// </summary>
        public int PageCount
        {
            get
            {
                if (PageSize <= 0)
                    return 1;

                int c = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    c += 1;
                return c;
            }
        }
        /// <summary>
        /// Dữ liệu truy vấn được
        /// </summary>
        public required List<T> Data { get; set; }
    }
}