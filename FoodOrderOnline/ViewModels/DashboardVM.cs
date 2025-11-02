using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class DashboardVM
    {

        public decimal TongDoanhThu { get; set; }
        public int DonHangMoi { get; set; }
        public int TongSoKhachHang { get; set; }
        public int TongSoMonAn { get; set; }


        public List<DonHang> DonHangGanDay { get; set; } = new List<DonHang>();
    }
}