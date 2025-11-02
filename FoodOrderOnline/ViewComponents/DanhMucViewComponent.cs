using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrders.ViewComponents
{
    public class DanhMucViewComponent : ViewComponent

    {
        private readonly FoodOrderContext db;
        public DanhMucViewComponent(FoodOrderContext context) => db = context;
        public IViewComponentResult Invoke()
        {
            var data = db.DanhMucs.Select(DM => new DanhMucVM
            {
                MaDm = DM.MaDm,
                TenDm = DM.TenDm
            });
            return View(data);
        }
    }
}
