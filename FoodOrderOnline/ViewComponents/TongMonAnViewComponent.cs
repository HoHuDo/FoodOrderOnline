using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderOnline.ViewComponents
{
    public class TongMonAnViewComponent : ViewComponent
    {
        private readonly FoodOrderContext db;
        public TongMonAnViewComponent(FoodOrderContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            int total = db.MonAns.Count();
            var viewModel = new TongMonAnVM
            {
                TongSoLuong = total
            };
            return View(viewModel);
        }
    }
}