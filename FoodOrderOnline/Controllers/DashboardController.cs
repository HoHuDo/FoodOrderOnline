using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels; // Thêm VM
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này

namespace FoodOrderOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "NhanVien, QuanTriVien")] // Bảo mật
    public class DashboardController : Controller
    {
        private readonly FoodOrderContext _context;

        public DashboardController(FoodOrderContext context)
        {
            _context = context;
        }

        // Action này sẽ tính toán và gửi dữ liệu thống kê
        public async Task<IActionResult> Index()
        {
            // Lấy ngày đầu tiên của tháng hiện tại
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // 1. Tổng doanh thu (chỉ tính đơn đã "Hoàn thành")
            var tongDoanhThu = await _context.DonHangs
                .Where(dh => dh.TrangThai == "Hoàn thành")
                .SumAsync(dh => dh.TongThanhToan); // Giả sử dùng TongThanhToan

            // 2. Đơn hàng mới (đơn "Chờ xác nhận")
            var donHangMoi = await _context.DonHangs
                .CountAsync(dh => dh.TrangThai == "Chờ xác nhận");

            // 3. Tổng số khách hàng (từ bảng KhachHang)
            var tongSoKhachHang = await _context.KhachHangs.CountAsync();

            // 4. Tổng số món ăn (từ bảng MonAn)
            var tongSoMonAn = await _context.MonAns.CountAsync();

            // 5. Lấy 5 đơn hàng mới nhất
            var donHangGanDay = await _context.DonHangs
                .Include(dh => dh.MaKhNavigation) // Lấy thông tin khách hàng
                .OrderByDescending(dh => dh.NgayDat)
                .Take(5)
                .ToListAsync();

            // Gói tất cả vào ViewModel
            var viewModel = new DashboardVM
            {
                TongDoanhThu = tongDoanhThu ?? 0, // Dùng 0 nếu bị null
                DonHangMoi = donHangMoi,
                TongSoKhachHang = tongSoKhachHang,
                TongSoMonAn = tongSoMonAn,
                DonHangGanDay = donHangGanDay
            };

            return View(viewModel); // Gửi VM này đến View
        }
    }
}