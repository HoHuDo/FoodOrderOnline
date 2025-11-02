using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.ViewComponents
{
    public class DanhMucMonAnViewComponent : ViewComponent
    {
        private readonly FoodOrderContext db;
        public DanhMucMonAnViewComponent(FoodOrderContext context) => db = context;


        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<int>? selectedIds = null)
        {
            var data = await db.DanhMucs
                .AsNoTracking()
                .OrderBy(dm => dm.TenDm)
                .Select(dm => new DanhMucMonAnVM
                {
                    MaDm = dm.MaDm,
                    TenDm = dm.TenDm,
                    SoLuong = dm.MonAns.Count()

                })
                .ToListAsync();


            ViewBag.SelectedIds = selectedIds ?? Enumerable.Empty<int>();

            return View(data);
        }
    }
}
