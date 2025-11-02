using System.ComponentModel.DataAnnotations;

namespace FoodOrderOnline.ViewModels
{
    public class LoginVM
    {
        // SỬA: Đổi 'Email' thành 'TenDangNhap'
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } // Đã xóa [EmailAddress]

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
    }
}