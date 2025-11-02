using FoodOrderOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    [Authorize(Roles = "NhanVien, QuanTriVien")]
    public class CRUDDonHangController : Controller
    {
        private readonly FoodOrderContext _context;

        public CRUDDonHangController(FoodOrderContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var foodOrderContext = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaVoucherNavigation)
                .OrderByDescending(d => d.NgayDat);
            return View(await foodOrderContext.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .Include(d => d.MaVoucherNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaMonNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);

            if (donHang == null) return NotFound();

            return View(donHang);
        }


        public IActionResult Create()
        {

            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "HoTen");

            ViewData["MaVoucher"] = new SelectList(_context.Vouchers, "MaVoucher", "MaCode");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDh,MaKh,NgayDat,TongTien,MaVoucher,SoTienGiam,TongThanhToan,TrangThai,PhuongThucThanhToan,GhiChu,DiaChiGiao")] DonHang donHang)
        {

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


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();


            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "HoTen", donHang.MaKh);
            ViewData["MaVoucher"] = new SelectList(_context.Vouchers, "MaVoucher", "MaCode", donHang.MaVoucher);


            List<string> trangThaiList = new List<string> { "Chờ xác nhận", "Đang giao", "Hoàn thành", "Đã hủy" };
            ViewData["TrangThaiList"] = new SelectList(trangThaiList, donHang.TrangThai);

            return View(donHang);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrangThai")] DonHang input)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            donHang.TrangThai = input.TrangThai;


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


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = await _context.DonHangs.FindAsync(id);
                if (donHang != null)
                {

                    var chiTietDonHangs = await _context.ChiTietDonHangs
                                                .Where(ct => ct.MaDh == id)
                                                .ToListAsync();

                    if (chiTietDonHangs.Any())
                    {
                        _context.ChiTietDonHangs.RemoveRange(chiTietDonHangs);
                    }


                    _context.DonHangs.Remove(donHang);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

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