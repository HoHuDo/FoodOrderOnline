using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly FoodOrderContext _context;

        public TrangChuController(FoodOrderContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var danhMucs = await _context.DanhMucs
                .Select(dm => new DanhMucTrangChu
                {
                    MaDm = dm.MaDm,
                    TenDm = dm.TenDm,
                    HinhAnh = dm.HinhAnh,
                    SoLuongMonAn = dm.MonAns.Count()
                })
                .Take(12)
                .ToListAsync();
            var monAnNoiBat = await _context.MonAns
                .OrderByDescending(m => m.Gia)
                .Take(8)
                .ToListAsync();
            var monAnMoiNhat = await _context.MonAns
                .OrderBy(m => m.MaMon)
                .Take(8)
                .ToListAsync();
            var viewModel = new TrangChuVM
            {
                DanhMucs = danhMucs,
                MonAnNoiBat = monAnNoiBat,
                MonAnMoiNhat = monAnMoiNhat
            };

            return View(viewModel);
        }
    }
}