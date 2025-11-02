using System;
using System.Collections.Generic;

namespace FoodOrderOnline.Models;

public partial class ChiTietDonHang
{
    public int MaCtdh { get; set; }

    public int MaDh { get; set; }

    public int MaMon { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual DonHang MaDhNavigation { get; set; } = null!;

    public virtual MonAn MaMonNavigation { get; set; } = null!;
}
