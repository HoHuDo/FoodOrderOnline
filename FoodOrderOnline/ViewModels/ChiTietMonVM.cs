using FoodOrderOnline.Models;

namespace FoodOrderOnline.ViewModels
{
    public class ChiTietMonVM
    {

        public MonAn MonAn { get; set; }


        public List<DanhGia> DanhGias { get; set; }


        public double SoSaoTrungBinh { get; set; }
        public int SoLuongDanhGia { get; set; }


        public List<MonAn> MonAnLienQuan { get; set; }
    }
}