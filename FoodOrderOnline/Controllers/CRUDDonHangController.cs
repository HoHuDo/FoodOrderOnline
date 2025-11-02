using FoodOrderOnline.Models;
using Microsoft.AspNetCore.Authorization; // Thêm bảo mật
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    [Authorize(Roles = "NhanVien, QuanTriVien")] // Thêm bảo mật
    public class CRUDDonHangController : Controller
    {
        private readonly FoodOrderContext _context;

        public CRUDDonHangController(FoodOrderContext context)
        {
            _context = context;
        }

        // GET: CRUDDonHang
        public async Task<IActionResult> Index()
        {
            var foodOrderContext = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaVoucherNavigation)
                .OrderByDescending(d => d.NgayDat); // Sắp xếp mới nhất
            return View(await foodOrderContext.ToListAsync());
        }

        // GET: CRUDDonHang/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaVoucherNavigation)
                .Include(d => d.ChiTietDonHangs) // Lấy CÁC MÓN HÀNG
                    .ThenInclude(ct => ct.MaMonNavigation) // Lấy tên Món ăn
                .FirstOrDefaultAsync(m => m.MaDh == id);

            if (donHang == null) return NotFound();

            return View(donHang);
        }

        // GET: CRUDDonHang/Create
        public IActionResult Create()
        {
            // SỬA: Hiển thị "HoTen" (Tên) thay vì "MaKh" (ID)
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "HoTen");
            // SỬA: Hiển thị "MaCode" (Code) thay vì "MaVoucher" (ID)
            ViewData["MaVoucher"] = new SelectList(_context.Vouchers, "MaVoucher", "MaCode");
            return View();
        }

        // POST: CRUDDonHang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDh,MaKh,NgayDat,TongTien,MaVoucher,SoTienGiam,TongThanhToan,TrangThai,PhuongThucThanhToan,GhiChu,DiaChiGiao")] DonHang donHang)
        {
            // (Lưu ý: Logic tính toán TongTien, SoTienGiam... nên được tự động hóa)
            // Tạm thời gán ngày đặt
            donHang.NgayDat = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "HoTen", donHang.MaKh);
            ViewData["MaVoucher"] = new SelectList(_context.Vouchers, "MaVoucher", "MaCode", donHang.MaVoucher);
            return View(donHang);
        }

        // GET: CRUDDonHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            // SỬA: Hiển thị "HoTen" và "MaCode"
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "HoTen", donHang.MaKh);
            ViewData["MaVoucher"] = new SelectList(_context.Vouchers, "MaVoucher", "MaCode", donHang.MaVoucher);

            // SỬA: Tạo List Trạng Thái
            List<string> trangThaiList = new List<string> { "Chờ xác nhận", "Đang giao", "Hoàn thành", "Đã hủy" };
            ViewData["TrangThaiList"] = new SelectList(trangThaiList, donHang.TrangThai);

            return View(donHang);
        }

        // POST: CRUDDonHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrangThai")] DonHang input)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            donHang.TrangThai = input.TrangThai;

            // Chỉ update thuộc tính cần thiết
            _context.Entry(donHang).Property(d => d.TrangThai).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DonHangExists(id)) return NotFound();
                throw;
            }
        }

        // GET: CRUDDonHang/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaVoucherNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (donHang == null) return NotFound();

            return View(donHang);
        }

        // POST: CRUDDonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // SỬA LỖI REFERENCE CONSTRAINT
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = await _context.DonHangs.FindAsync(id);
                if (donHang != null)
                {
                    // 1. Tìm và Xóa 'ChiTietDonHang' (con) trước
                    var chiTietDonHangs = await _context.ChiTietDonHangs
                                                .Where(ct => ct.MaDh == id)
                                                .ToListAsync();

                    if (chiTietDonHangs.Any())
                    {
                        _context.ChiTietDonHangs.RemoveRange(chiTietDonHangs);
                    }

                    // 2. Xóa 'DonHang' (cha)
                    _context.DonHangs.Remove(donHang);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                // (Có thể thêm TempData["Error"] = "Lỗi khi xóa đơn hàng")
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.MaDh == id);
        }
    }
}