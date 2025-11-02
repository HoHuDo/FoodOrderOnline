using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderOnline.ViewComponents
{
    public class LocTheoGiaViewComponent : ViewComponent
    {
        private readonly FoodOrderContext db;
        public LocTheoGiaViewComponent(FoodOrderContext context) => db = context;
        public IViewComponentResult Invoke()
        {
            var data = new LocTheoGiaVM
            {
                TongSoMon = db.MonAns.Count(),
                duoi50 = db.MonAns.Count(m => m.Gia < 50000),
                tu50den100 = db.MonAns.Count(m => m.Gia >= 50000 && m.Gia < 100000),
                tu100den150 = db.MonAns.Count(m => m.Gia >= 100000 && m.Gia < 150000),
                tu150den200 = db.MonAns.Count(m => m.Gia >= 150000 && m.Gia < 200000),
                tren200 = db.MonAns.Count(m => m.Gia >= 200000)
            };
            return View(data);

        }
    }
}
