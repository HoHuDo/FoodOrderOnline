namespace FoodOrderOnline.ViewModels
{

    public class GioHangItem
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = "";
        public decimal Gia { get; set; }
        public string HinhAnh { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * Gia;
    }


    public class GioHangVM
    {
        public List<GioHangItem> CartItems { get; set; } = new List<GioHangItem>();


        public decimal Subtotal => CartItems.Sum(item => item.ThanhTien);


        public decimal Shipping { get; set; } = 25000;


        public decimal CouponDiscount { get; set; } = 0;
        public string CouponCode { get; set; } = "";
        public string CouponMessage { get; set; } = "";


        public decimal Total => (Subtotal + Shipping - CouponDiscount) > 0 ? (Subtotal + Shipping - CouponDiscount) : 0;
    }
}