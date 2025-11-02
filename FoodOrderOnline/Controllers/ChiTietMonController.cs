using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ChiTietMonController : Controller
{
    private readonly FoodOrderContext _db;
    public ChiTietMonController(FoodOrderContext db) => _db = db;
    public IActionResult Index(int? id)
    {
        if (!id.HasValue)
        {
            int maMon = HttpContext.Session.GetInt32("LastMaMon") ?? 1;
            return RedirectToAction(nameof(Index), new { id = maMon });
        }

        int monId = id.Value;

        HttpContext.Session.SetInt32("LastMaMon", monId);

        var monAn = _db.MonAns
            .Include(m => m.MaDmNavigation)
            .FirstOrDefault(m => m.MaMon == monId);

        if (monAn == null) return NotFound();

        var danhGias = _db.DanhGia
            .Include(dg => dg.MaKhNavigation)
            .Where(dg => dg.MaMon == monId)
            .OrderByDescending(dg => dg.NgayDanhGia)
            .ToList();
        double soSaoTrungBinh = danhGias.Count > 0 ? danhGias.Average(dg => dg.SoSao ?? 0) : 0;

        var monAnLienQuan = _db.MonAns
            .Where(m => m.MaDm == monAn.MaDm && m.MaMon != monId)
            .Take(4)
            .ToList();

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
