using System;
using System.Collections.Generic;

namespace FoodOrderOnline.Models;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string HoTen { get; set; } = null!;

    public string? ChucVu { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public int? MaTk { get; set; }

    public virtual TaiKhoan? MaTkNavigation { get; set; }
}
