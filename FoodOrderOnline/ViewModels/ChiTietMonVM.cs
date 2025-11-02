using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class ChiTietMonVM
    {
        // Món ăn đang xem
        public MonAn MonAn { get; set; }

        // Danh sách đánh giá cho món ăn
        public List<DanhGia> DanhGias { get; set; }

        // Thông số đánh giá
        public double SoSaoTrungBinh { get; set; }
        public int SoLuongDanhGia { get; set; }

        // Danh sách món ăn liên quan (cho phần "You May Also Like")
        public List<MonAn> MonAnLienQuan { get; set; }
    }
}