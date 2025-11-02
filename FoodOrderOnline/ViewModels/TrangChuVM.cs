using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class TrangChuVM
    {
        public IEnumerable<DanhMucTrangChu> DanhMucs { get; set; }

        public IEnumerable<MonAn> MonAnNoiBat { get; set; }
        public IEnumerable<MonAn> MonAnMoiNhat { get; set; }
    }
}