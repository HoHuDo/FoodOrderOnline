using FoodOrderOnline.Models;
using Microsoft.AspNetCore.Authorization; // Thêm 1: Thư viện Bảo mật
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    // SỬA: Thêm bảo mật
    [Authorize(Roles = "NhanVien, QuanTriVien")]
    public class CRUDMonAnController : Controller // SỬA: Đổi tên
    {
        private readonly FoodOrderContext _context;
        // SỬA: Thêm WebHostEnvironment để xử lý file
        private readonly IWebHostEnvironment _webHostEnvironment;

        // SỬA: Inject IWebHostEnvironment
        public CRUDMonAnController(FoodOrderContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: AdminMonAn
        public async Task<IActionResult> Index()
        {
            var foodOrderContext = _context.MonAns.Include(m => m.MaDmNavigation);
            return View(await foodOrderContext.ToListAsync());
        }

        // GET: AdminMonAn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null) return NotFound();

            return View(monAn);
        }

        // GET: AdminMonAn/Create
        public IActionResult Create()
        {
            // SỬA: Hiển thị "TenDm" (Tên) thay vì "MaDm" (ID)
            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm");
            return View();
        }

        // POST: AdminMonAn/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // SỬA: Thêm 'IFormFile HinhAnhFile' và bỏ 'HinhAnh' khỏi [Bind]
        public async Task<IActionResult> Create(
            [Bind("MaMon,TenMon,MoTa,Gia,MaDm,TrangThai")] MonAn monAn,
            IFormFile HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // --- BẮT ĐẦU LOGIC XỬ LÝ FILE ---
                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhFile.FileName);
                    string filePath = Path.Combine(wwwRootPath, "images/monan", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhFile.CopyToAsync(fileStream);
                    }
                    monAn.HinhAnh = fileName; // Lưu tên file vào DB
                }
                else
                {
                    monAn.HinhAnh = "default_food.png"; // Ảnh mặc định
                }
                // --- KẾT THÚC LOGIC XỬ LÝ FILE ---

                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // SỬA: Hiển thị "TenDm" khi form lỗi
            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }

        // GET: AdminMonAn/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();

            // SỬA: Hiển thị "TenDm"
            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }

        // POST: AdminMonAn/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // SỬA: Thêm 'IFormFile HinhAnhFile'
        public async Task<IActionResult> Edit(int id,
            [Bind("MaMon,TenMon,MoTa,Gia,MaDm,TrangThai")] MonAn monAn,
            IFormFile HinhAnhFile)
        {
            if (id != monAn.MaMon) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // --- BẮT ĐẦU LOGIC XỬ LÝ FILE (EDIT) ---
                    if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                    {
                        // Người dùng tải ảnh mới -> Lưu ảnh mới
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhFile.FileName);
                        string filePath = Path.Combine(wwwRootPath, "images/monan", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await HinhAnhFile.CopyToAsync(fileStream);
                        }
                        monAn.HinhAnh = fileName; // Gán tên file mới

                        // (Bạn có thể thêm code để xóa file ảnh cũ ở đây)
                    }
                    else
                    {
                        // Người dùng KHÔNG tải ảnh mới -> Giữ lại ảnh cũ
                        var monAnGoc = await _context.MonAns.AsNoTracking().FirstOrDefaultAsync(m => m.MaMon == id);
                        if (monAnGoc != null)
                        {
                            monAn.HinhAnh = monAnGoc.HinhAnh;
                        }
                    }
                    // --- KẾT THÚC LOGIC XỬ LÝ FILE (EDIT) ---

                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonAnExists(monAn.MaMon)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            // SỬA: Hiển thị "TenDm" khi form lỗi
            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }

        // GET: AdminMonAn/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null) return NotFound();

            return View(monAn);
        }

        // POST: AdminMonAn/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {
                // (Bạn nên thêm logic xóa file ảnh trong 'wwwroot' ở đây)
                _context.MonAns.Remove(monAn);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MonAnExists(int id)
        {
            return _context.MonAns.Any(e => e.MaMon == id);
        }
    }
}