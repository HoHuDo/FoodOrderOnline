namespace FoodOrderOnline.ViewModels
{
    // Đã đổi 'double' sang 'decimal'
    public class GioHangItem
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = "";
        public decimal Gia { get; set; } // SỬA: double -> decimal
        public string HinhAnh { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * Gia; // Tự động là decimal
    }

    // Đã đổi 'double' sang 'decimal'
    public class GioHangVM
    {
        public List<GioHangItem> CartItems { get; set; } = new List<GioHangItem>();

        // Tổng tiền hàng
        public decimal Subtotal => CartItems.Sum(item => item.ThanhTien);

        // Phí ship
        public decimal Shipping { get; set; } = 0; // SỬA: double -> decimal

        // Thông tin giảm giá
        public decimal CouponDiscount { get; set; } = 0; // SỬA: double -> decimal
        public string CouponCode { get; set; } = "";
        public string CouponMessage { get; set; } = "";

        // Tổng tiền cuối cùng
        public decimal Total => (Subtotal + Shipping - CouponDiscount) > 0 ? (Subtotal + Shipping - CouponDiscount) : 0;
    }
}