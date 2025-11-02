using System.ComponentModel.DataAnnotations;

namespace FoodOrderOnline.ViewModels
{
    public class ThanhToanVM
    {
        // 1. Dùng để hiển thị tóm tắt đơn hàng (bên phải)
        public GioHangVM Summary { get; set; } = new GioHangVM();


        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; }

        public string? GhiChu { get; set; }
    }
}