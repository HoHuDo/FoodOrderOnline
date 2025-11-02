using FoodOrderOnline.Helpers; // SỬA: Đã thêm dòng này
using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    public class GioHangController : Controller
    {
        private readonly FoodOrderContext db;
        public const string CART_KEY = "Cart";
        public const string COUPON_KEY = "Coupon";

        public GioHangController(FoodOrderContext context)
        {
            db = context;
        }

        #region Helper Functions
        List<GioHangItem> GetCartItems()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>(CART_KEY);
            return cart ?? new List<GioHangItem>();
        }

        void SaveCartSession(List<GioHangItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
        }

        Voucher GetCoupon()
        {
            return HttpContext.Session.GetObjectFromJson<Voucher>(COUPON_KEY);
        }

        GioHangVM BuildCartViewModel()
        {
            var cart = GetCartItems();
            var voucher = GetCoupon();
            // SỬA: Dùng 'decimal' cho subtotal
            decimal subtotal = cart.Sum(item => item.ThanhTien);

            var viewModel = new GioHangVM
            {
                CartItems = cart,
                Shipping = 25000 // Tự động là decimal
            };

            if (voucher != null)
            {
                viewModel.CouponCode = voucher.MaCode;

                // SỬA: Giờ tất cả đều là 'decimal', không còn lỗi so sánh
                if (subtotal < voucher.DieuKienDonHangTu)
                {
                    viewModel.CouponDiscount = 0;
                    viewModel.CouponMessage = $"Mã giảm giá chỉ áp dụng cho đơn hàng từ {voucher.DieuKienDonHangTu:N0}đ";
                }
                else
                {
                    if (voucher.LoaiGiamGia == "PhanTram")
                    {
                        // SỬA: Phép tính 'decimal'
                        decimal discount = subtotal * (voucher.GiaTri / 100);
                        if (voucher.GiamToiDa.HasValue && discount > voucher.GiamToiDa.Value)
                        {
                            discount = voucher.GiamToiDa.Value;
                        }
                        viewModel.CouponDiscount = discount;
                    }
                    else if (voucher.LoaiGiamGia == "SoTien")
                    {
                        viewModel.CouponDiscount = voucher.GiaTri;
                    }
                }
            }
            return viewModel;
        }
        #endregion

        public IActionResult Index()
        {
            return View(BuildCartViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int maMon, int soLuong = 1)
        {
            var cart = GetCartItems();
            GioHangItem item = cart.SingleOrDefault(p => p.MaMon == maMon);

            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                var monAn = await db.MonAns.FindAsync(maMon);
                if (monAn == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy món ăn" });
                }
                item = new GioHangItem
                {
                    MaMon = monAn.MaMon,
                    TenMon = monAn.TenMon,
                    Gia = monAn.Gia, // SỬA: Tự động gán decimal vào decimal
                    HinhAnh = monAn.HinhAnh,
                    SoLuong = soLuong
                };
                cart.Add(item);
            }
            SaveCartSession(cart);
            return Json(new { success = true, cartCount = cart.Count });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int maMon)
        {
            var cart = GetCartItems();
            var item = cart.SingleOrDefault(p => p.MaMon == maMon);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartSession(cart);
            }
            return Json(new { success = true, summary = BuildCartViewModel(), cartCount = cart.Count });
        }

        [HttpPost]
        public IActionResult UpdateCart(int maMon, int soLuong)
        {
            var cart = GetCartItems();
            var item = cart.SingleOrDefault(p => p.MaMon == maMon);

            if (item != null)
            {
                if (soLuong <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.SoLuong = soLuong;
                }
                SaveCartSession(cart);
            }

            var itemTotal = item?.ThanhTien ?? 0;
            return Json(new
            {
                success = true,
                summary = BuildCartViewModel(),
                itemTotal = itemTotal,
                cartCount = cart.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                HttpContext.Session.Remove(COUPON_KEY);
                return Json(new { success = true, message = "Đã xóa mã giảm giá.", summary = BuildCartViewModel() });
            }

            var voucher = await db.Vouchers.FirstOrDefaultAsync(v => v.MaCode == couponCode);

            // SỬA LỖI 'bool? != int':
            // Dựa trên ảnh của bạn, 'TrangThai' là 'int' (giá trị 1).
            // Hãy đảm bảo model 'Voucher.cs' của bạn có: public int TrangThai { get; set; }
            if (voucher == null || voucher.TrangThai != true)
            {
                HttpContext.Session.Remove(COUPON_KEY);
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ.", summary = BuildCartViewModel() });
            }
            if (voucher.NgayKetThuc < DateTime.Now)
            {
                HttpContext.Session.Remove(COUPON_KEY);
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn.", summary = BuildCartViewModel() });
            }
            if (voucher.SoLuong <= 0)
            {
                HttpContext.Session.Remove(COUPON_KEY);
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng.", summary = BuildCartViewModel() });
            }

            // SỬA: Dùng 'decimal'
            decimal subtotal = GetCartItems().Sum(item => item.ThanhTien);
            if (subtotal < voucher.DieuKienDonHangTu)
            {
                HttpContext.Session.Remove(COUPON_KEY);
                return Json(new
                {
                    success = false,
                    message = $"Mã này chỉ áp dụng cho đơn hàng từ {voucher.DieuKienDonHangTu:N0}đ",
                    summary = BuildCartViewModel()
                });
            }

            HttpContext.Session.SetObjectAsJson(COUPON_KEY, voucher);

            return Json(new { success = true, message = "Áp dụng mã giảm giá thành công!", summary = BuildCartViewModel() });
        }
    }
}