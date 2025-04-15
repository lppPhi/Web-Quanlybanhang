namespace SV21T1020578.Shop.Models
{
    //lưu thông tin đầu vào sử dụng cho chức năng tìm kiếm và hiển thị dữ liệu dưới dạng phân trang
    public class PaginationSearchInput
    {
        //trang hiển thị
        public int Page { get; set; } = 1;
        //số dòng hiển thị trên mỗi trang
        public int PageSize { get; set; }
        //Chuỗi giá trị cần tìm kiếm
        public string SearchValue { get; set; } = "";
    }
}