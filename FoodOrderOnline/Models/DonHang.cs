namespace FoodOrderOnline.Models;

public partial class DonHang
{
    public int MaDh { get; set; }

    public int MaKh { get; set; }

    public DateTime? NgayDat { get; set; }

    public decimal? TongTien { get; set; }

    public int? MaVoucher { get; set; }

    public decimal? SoTienGiam { get; set; }

    public decimal? TongThanhToan { get; set; }

    public string? TrangThai { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public string? GhiChu { get; set; }

    public string? DiaChiGiao { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual Voucher? MaVoucherNavigation { get; set; }
}
