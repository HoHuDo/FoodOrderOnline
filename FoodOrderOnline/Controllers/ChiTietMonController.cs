using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ChiTietMonController : Controller
{
    private readonly FoodOrderContext _db;
    public ChiTietMonController(FoodOrderContext db) => _db = db;

    // URL: /ChiTietMon      -> tự redirect sang Index/{maMon}
    // URL: /ChiTietMon/Index/5 hoặc /ChiTietMon?id=5 -> hiển thị chi tiết
    public IActionResult Index(int? id)
    {
        if (!id.HasValue)
        {
            int maMon = HttpContext.Session.GetInt32("LastMaMon") ?? 1;
            return RedirectToAction(nameof(Index), new { id = maMon });
        }

        int monId = id.Value;

        // Lưu "lần xem gần nhất"
        HttpContext.Session.SetInt32("LastMaMon", monId);

        // 1. Lấy món ăn + danh mục
        var monAn = _db.MonAns
            .Include(m => m.MaDmNavigation)
            .FirstOrDefault(m => m.MaMon == monId);

        if (monAn == null) return NotFound();

        // 2. Lấy đánh giá + khách hàng
        var danhGias = _db.DanhGia
            .Include(dg => dg.MaKhNavigation)
            .Where(dg => dg.MaMon == monId)
            .OrderByDescending(dg => dg.NgayDanhGia)
            .ToList();

        // 3. Tính sao trung bình
        double soSaoTrungBinh = danhGias.Count > 0 ? danhGias.Average(dg => dg.SoSao ?? 0) : 0;

        // 4. Món liên quan
        var monAnLienQuan = _db.MonAns
            .Where(m => m.MaDm == monAn.MaDm && m.MaMon != monId)
            .Take(4)
            .ToList();

        // 5. ViewModel
        var vm = new ChiTietMonVM
        {
            MonAn = monAn,
            DanhGias = danhGias,
            SoSaoTrungBinh = soSaoTrungBinh,
            SoLuongDanhGia = danhGias.Count,
            MonAnLienQuan = monAnLienQuan
        };

        return View(vm);
    }
}
