using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class MonAnPaginationVM
    {
        // Danh sách món ăn của trang hiện tại
        public IEnumerable<MonAn> MonAns { get; set; }

        // Thông tin phân trang
        public PaginationInfo Pagination { get; set; }

        public List<int> SelectedDanhMucIds { get; set; } = new List<int>();
    }
}