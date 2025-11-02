namespace FoodOrderOnline.Models;

public partial class Voucher
{
    public int MaVoucher { get; set; }

    public string MaCode { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? LoaiGiamGia { get; set; }

    public decimal GiaTri { get; set; }

    public decimal? DieuKienDonHangTu { get; set; }

    public decimal? GiamToiDa { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public int SoLuong { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
