using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class DashboardVM
    {
        // Con số cho 4 "Thẻ" (Cards) trên cùng
        public decimal TongDoanhThu { get; set; }
        public int DonHangMoi { get; set; }
        public int TongSoKhachHang { get; set; }
        public int TongSoMonAn { get; set; }

        // Dữ liệu cho bảng "Đơn hàng gần đây"
        public List<DonHang> DonHangGanDay { get; set; } = new List<DonHang>();
    }
}