using FoodOrderOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{

    [Authorize(Roles = "NhanVien, QuanTriVien")]
    public class CRUDMonAnController : Controller
    {
        private readonly FoodOrderContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;


        public CRUDMonAnController(FoodOrderContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            var foodOrderContext = _context.MonAns.Include(m => m.MaDmNavigation);
            return View(await foodOrderContext.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null) return NotFound();

            return View(monAn);
        }


        public IActionResult Create()
        {

            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(
            [Bind("MaMon,TenMon,MoTa,Gia,MaDm,TrangThai")] MonAn monAn,
            IFormFile HinhAnhFile)
        {
            if (ModelState.IsValid)
            {

                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhFile.FileName);
                    string filePath = Path.Combine(wwwRootPath, "images/monan", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhFile.CopyToAsync(fileStream);
                    }
                    monAn.HinhAnh = fileName;
                }
                else
                {
                    monAn.HinhAnh = "default_food.png";
                }


                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();


            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id,
            [Bind("MaMon,TenMon,MoTa,Gia,MaDm,TrangThai")] MonAn monAn,
            IFormFile HinhAnhFile)
        {
            if (id != monAn.MaMon) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {

                    if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                    {

                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhFile.FileName);
                        string filePath = Path.Combine(wwwRootPath, "images/monan", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await HinhAnhFile.CopyToAsync(fileStream);
                        }
                        monAn.HinhAnh = fileName;


                    }
                    else
                    {

                        var monAnGoc = await _context.MonAns.AsNoTracking().FirstOrDefaultAsync(m => m.MaMon == id);
                        if (monAnGoc != null)
                        {
                            monAn.HinhAnh = monAnGoc.HinhAnh;
                        }
                    }


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

            ViewData["MaDm"] = new SelectList(_context.DanhMucs, "MaDm", "TenDm", monAn.MaDm);
            return View(monAn);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var monAn = await _context.MonAns
                .Include(m => m.MaDmNavigation)
                .FirstOrDefaultAsync(m => m.MaMon == id);
            if (monAn == null) return NotFound();

            return View(monAn);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn != null)
            {

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