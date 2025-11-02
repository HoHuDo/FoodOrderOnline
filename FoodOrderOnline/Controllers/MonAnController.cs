using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    public class MonAnController : Controller
    {
        private readonly FoodOrderContext db;
        private int pageSize = 12;

        public MonAnController(FoodOrderContext context) => db = context;

        // /MonAn?danhMucId=3&sortBy=price_asc&page=1
        public async Task<IActionResult> Index(
            int page = 1,
            string sortBy = "price_asc",
            int? danhMucId = null,                    // <-- nhận từ Trang chủ
            List<int>? danhMucIds = null,             // <-- nhận khi người dùng lọc nhiều danh mục
            List<string>? priceRanges = null          // <-- nhận khi có filter khoảng giá
        )
        {
            // Gộp tham số: nếu gọi từ Trang chủ (chỉ có danhMucId) thì chuyển thành danhMucIds
            if ((danhMucIds == null || danhMucIds.Count == 0) && danhMucId.HasValue)
                danhMucIds = new List<int> { danhMucId.Value };

            page = Math.Max(1, page);

            // Build query dùng chung
            var query = BuildMonAnQuery(danhMucIds, priceRanges, sortBy);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var monAns = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new MonAnPaginationVM
            {
                MonAns = monAns,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            // Truyền trạng thái filter cho View để tự "chọn sẵn"
            ViewBag.SelectedDanhMucIds = danhMucIds ?? new List<int>();
            ViewBag.SelectedPriceRanges = priceRanges ?? new List<string>();
            ViewBag.SortBy = sortBy;

            // (tuỳ chọn) truyền toàn bộ danh mục để render filter
            ViewBag.AllDanhMucs = await db.DanhMucs
                                          .OrderBy(d => d.TenDm)
                                          .Select(d => new SelectListItem { Value = d.MaDm.ToString(), Text = d.TenDm })
                                          .ToListAsync();

            return View(viewModel);
        }

        // AJAX lọc dữ liệu (giữ nguyên API cũ)
        public async Task<IActionResult> LocMonAn(
            List<int> danhMucIds,
            List<string> priceRanges,
            int page = 1,
            string sortBy = "price_asc")
        {
            page = Math.Max(1, page);

            var query = BuildMonAnQuery(danhMucIds, priceRanges, sortBy);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var monAns = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new MonAnPaginationVM
            {
                MonAns = monAns,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            return Json(new
            {
                productsHtml = await this.RenderViewToStringAsync("_DanhSachMonAnPartial", viewModel.MonAns),
                paginationHtml = await this.RenderViewToStringAsync("_PaginationPartial", viewModel.Pagination)
            });
        }

        // ======= Private: build query dùng chung cho Index & LocMonAn =======
        private IQueryable<MonAn> BuildMonAnQuery(IEnumerable<int>? danhMucIds, IEnumerable<string>? priceRanges, string sortBy)
        {
            var query = db.MonAns.AsQueryable();

            // lọc theo danh mục (1 hoặc nhiều)
            if (danhMucIds != null && danhMucIds.Any())
            {
                query = query.Where(m => m.MaDm.HasValue && danhMucIds.Contains(m.MaDm.Value));
            }

            // lọc theo khoảng giá
            if (priceRanges != null)
            {
                var ranges = priceRanges.ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (ranges.Count > 0 && !ranges.Contains("all"))
                {
                    query = query.Where(m =>
                        (ranges.Contains("duoi50") && m.Gia < 50000) ||
                        (ranges.Contains("tu50den100") && m.Gia >= 50000 && m.Gia < 100000) ||
                        (ranges.Contains("tu100den150") && m.Gia >= 100000 && m.Gia < 150000) ||
                        (ranges.Contains("tu150den200") && m.Gia >= 150000 && m.Gia < 200000) ||
                        (ranges.Contains("tren200") && m.Gia >= 200000)
                    );
                }
            }

            // sắp xếp
            query = sortBy switch
            {
                "price_desc" => query.OrderByDescending(m => m.Gia),
                _ => query.OrderBy(m => m.Gia)
            };

            return query;
        }
    }

    // ========== Render partial ra string cho JSON ==========
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller, string viewName, TModel model)
        {
            var viewData = new ViewDataDictionary<TModel>(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
            { Model = model };

            using var sw = new System.IO.StringWriter();
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, isMainPage: false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"Không tìm thấy View '{viewName}'.");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                viewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
