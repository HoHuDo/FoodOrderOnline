namespace FoodOrderOnline.Models;

public partial class DanhGia
{
    public int MaDg { get; set; }

    public int MaKh { get; set; }

    public int MaMon { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual MonAn MaMonNavigation { get; set; } = null!;
}
