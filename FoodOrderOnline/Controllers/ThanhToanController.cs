using FoodOrderOnline.Helpers;
using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderOnline.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly FoodOrderContext db;

        public ThanhToanController(FoodOrderContext context)
        {
            db = context;
        }

        #region Helper Functions

        List<GioHangItem> GetCartItems()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>(GioHangController.CART_KEY);
            return cart ?? new List<GioHangItem>();
        }
        Voucher GetCoupon()
        {
            return HttpContext.Session.GetObjectFromJson<Voucher>(GioHangController.COUPON_KEY);
        }
        GioHangVM BuildCartViewModel()
        {
            var cart = GetCartItems();
            var voucher = GetCoupon();
            decimal subtotal = cart.Sum(item => item.ThanhTien);
            var viewModel = new GioHangVM
            {
                CartItems = cart,
                Shipping = 25000
            };
            if (voucher != null)
            {
                viewModel.CouponCode = voucher.MaCode;
                if (subtotal >= voucher.DieuKienDonHangTu)
                {
                    if (voucher.LoaiGiamGia == "PhanTram")
                    {
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
                else
                {
                    viewModel.CouponMessage = $"Mã chỉ áp dụng cho đơn từ {voucher.DieuKienDonHangTu:N0}đ";
                }
            }
            return viewModel;
        }
        #endregion


        [HttpGet]
        public IActionResult Index()
        {
            var gioHangVM = BuildCartViewModel();
            if (!gioHangVM.CartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống, không thể thanh toán.";
                return RedirectToAction("Index", "GioHang");
            }
            var checkoutVM = new ThanhToanVM
            {
                Summary = gioHangVM
            };
            return View(checkoutVM);
        }


        [HttpPost]
        public async Task<IActionResult> Index(ThanhToanVM model)
        {
            var gioHangVM = BuildCartViewModel();
            model.Summary = gioHangVM;

            if (!gioHangVM.CartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("Index", "GioHang");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {


                KhachHang khachHang;


                khachHang = await db.KhachHangs.FirstOrDefaultAsync(k => k.Email == model.Email);

                if (khachHang == null)
                {

                    khachHang = new KhachHang
                    {
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = model.SoDienThoai,
                        DiaChi = model.DiaChi,


                    };
                    db.KhachHangs.Add(khachHang);
                }
                else
                {

                    khachHang.HoTen = model.HoTen;
                    khachHang.SoDienThoai = model.SoDienThoai;
                    khachHang.DiaChi = model.DiaChi;
                    db.KhachHangs.Update(khachHang);
                }

                await db.SaveChangesAsync();




                var donHang = new DonHang
                {
                    MaKh = khachHang.MaKh,
                    NgayDat = DateTime.Now,
                    DiaChiGiao = model.DiaChi,
                    GhiChu = model.GhiChu,
                    TrangThai = "Chờ xác nhận",
                    TongTien = gioHangVM.Subtotal,
                    SoTienGiam = gioHangVM.CouponDiscount,
                    TongThanhToan = gioHangVM.Total,
                    PhuongThucThanhToan = "COD"
                };


                if (!string.IsNullOrEmpty(gioHangVM.CouponCode))
                {
                    var voucher = await db.Vouchers.FirstOrDefaultAsync(v => v.MaCode == gioHangVM.CouponCode);
                    if (voucher != null)
                    {
                        donHang.MaVoucher = voucher.MaVoucher;
                        voucher.SoLuong -= 1;
                    }
                }

                db.DonHangs.Add(donHang);
                await db.SaveChangesAsync();


                var chiTietDonHangs = new List<ChiTietDonHang>();
                foreach (var item in gioHangVM.CartItems)
                {
                    var chiTiet = new ChiTietDonHang
                    {
                        MaDh = donHang.MaDh,
                        MaMon = item.MaMon,
                        SoLuong = item.SoLuong,
                        DonGia = item.Gia,
                        ThanhTien = item.ThanhTien
                    };
                    chiTietDonHangs.Add(chiTiet);
                }

                await db.ChiTietDonHangs.AddRangeAsync(chiTietDonHangs);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();


                HttpContext.Session.Remove(GioHangController.CART_KEY);
                HttpContext.Session.Remove(GioHangController.COUPON_KEY);


                return RedirectToAction("DatHangThanhCong", new { id = donHang.MaDh });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();


                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại.");
                return View(model);
            }
        }

        public IActionResult DatHangThanhCong(int id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }
    }
}