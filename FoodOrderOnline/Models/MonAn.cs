namespace FoodOrderOnline.Models;

public partial class MonAn
{
    public int MaMon { get; set; }

    public string TenMon { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal Gia { get; set; }

    public string? HinhAnh { get; set; }

    public int? MaDm { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual DanhMuc? MaDmNavigation { get; set; }
}
