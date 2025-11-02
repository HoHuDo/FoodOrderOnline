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


        public async Task<IActionResult> Index(
            int page = 1,
            string sortBy = "price_asc",
            int? danhMucId = null,
            List<int>? danhMucIds = null,
            List<string>? priceRanges = null
        )
        {

            if ((danhMucIds == null || danhMucIds.Count == 0) && danhMucId.HasValue)
                danhMucIds = new List<int> { danhMucId.Value };

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


            ViewBag.SelectedDanhMucIds = danhMucIds ?? new List<int>();
            ViewBag.SelectedPriceRanges = priceRanges ?? new List<string>();
            ViewBag.SortBy = sortBy;


            ViewBag.AllDanhMucs = await db.DanhMucs
                                          .OrderBy(d => d.TenDm)
                                          .Select(d => new SelectListItem { Value = d.MaDm.ToString(), Text = d.TenDm })
                                          .ToListAsync();

            return View(viewModel);
        }


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


        private IQueryable<MonAn> BuildMonAnQuery(IEnumerable<int>? danhMucIds, IEnumerable<string>? priceRanges, string sortBy)
        {
            var query = db.MonAns.AsQueryable();


            if (danhMucIds != null && danhMucIds.Any())
            {
                query = query.Where(m => m.MaDm.HasValue && danhMucIds.Contains(m.MaDm.Value));
            }


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


            query = sortBy switch
            {
                "price_desc" => query.OrderByDescending(m => m.Gia),
                _ => query.OrderBy(m => m.Gia)
            };

            return query;
        }
    }


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
