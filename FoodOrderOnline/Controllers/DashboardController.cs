using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien, QuanTriVien")]
    public class DashboardController : Controller
    {
        private readonly FoodOrderContext _context;

        public DashboardController(FoodOrderContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);


            var tongDoanhThu = await _context.DonHangs
                .Where(dh => dh.TrangThai == "Hoàn thành")
                .SumAsync(dh => dh.TongThanhToan);


            var donHangMoi = await _context.DonHangs
                .CountAsync(dh => dh.TrangThai == "Chờ xác nhận");


            var tongSoKhachHang = await _context.KhachHangs.CountAsync();


            var tongSoMonAn = await _context.MonAns.CountAsync();


            var donHangGanDay = await _context.DonHangs
                .Include(dh => dh.MaKhNavigation)
                .OrderByDescending(dh => dh.NgayDat)
                .Take(5)
                .ToListAsync();


            var viewModel = new DashboardVM
            {
                TongDoanhThu = tongDoanhThu ?? 0,
                DonHangMoi = donHangMoi,
                TongSoKhachHang = tongSoKhachHang,
                TongSoMonAn = tongSoMonAn,
                DonHangGanDay = donHangGanDay
            };

            return View(viewModel);
        }
    }
}