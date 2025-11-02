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

        // selectedIds: các danh mục cần check sẵn (truyền từ Index.cshtml)
        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<int>? selectedIds = null)
        {
            var data = await db.DanhMucs
                .AsNoTracking()
                .OrderBy(dm => dm.TenDm) // đổi theo tên cột thực tế
                .Select(dm => new DanhMucMonAnVM
                {
                    MaDm = dm.MaDm,
                    TenDm = dm.TenDm,
                    SoLuong = dm.MonAns.Count() // dùng Count() để EF dịch về COUNT(*)
                    // Hoặc an toàn hơn: SoLuong = db.MonAns.Count(m => m.MaDm == dm.MaDm)
                })
                .ToListAsync();

            // Truyền danh sách đã chọn cho View (Default.cshtml dùng ViewBag.SelectedIds để "checked")
            ViewBag.SelectedIds = selectedIds ?? Enumerable.Empty<int>();

            return View(data);
        }
    }
}
