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
        // ... (Các hàm GetCartItems, GetCoupon, BuildCartViewModel của bạn giữ nguyên) ...
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

        // 1. HIỂN THỊ TRANG THANH TOÁN (GET)
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

        // 2. XỬ LÝ ĐẶT HÀNG (POST)
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
                // === SỬA LỖI BẮT ĐẦU TỪ ĐÂY ===

                // BƯỚC 1: Tìm hoặc Tạo Khách Hàng
                KhachHang khachHang;

                // 1.1. Thử tìm khách hàng bằng Email (vì Email thường là UNIQUE)
                khachHang = await db.KhachHangs.FirstOrDefaultAsync(k => k.Email == model.Email);

                if (khachHang == null)
                {
                    // 1.2. Nếu KHÔNG TÌM THẤY -> Tạo khách hàng mới
                    khachHang = new KhachHang
                    {
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = model.SoDienThoai,
                        DiaChi = model.DiaChi,

                        // LƯU Ý: Nếu bảng KhachHang có cột UNIQUE (ví dụ: TenDangNhap)
                        // Bạn BẮT BUỘC phải gán giá trị cho nó ở đây.
                        // Ví dụ (nếu cột UNIQUE là TenDangNhap):
                        // TenDangNhap = model.Email 
                    };
                    db.KhachHangs.Add(khachHang);
                }
                else
                {
                    // 1.3. Nếu TÌM THẤY -> Cập nhật thông tin (tùy chọn)
                    khachHang.HoTen = model.HoTen;
                    khachHang.SoDienThoai = model.SoDienThoai;
                    khachHang.DiaChi = model.DiaChi;
                    db.KhachHangs.Update(khachHang);
                }

                await db.SaveChangesAsync(); // Lưu thay đổi (Thêm mới hoặc Cập nhật) để có MaKh

                // === KẾT THÚC SỬA LỖI ===


                // BƯỚC 2: TẠO ĐƠN HÀNG (Giữ nguyên)
                var donHang = new DonHang
                {
                    MaKh = khachHang.MaKh, // <-- Lấy MaKh (hoặc mới hoặc cũ)
                    NgayDat = DateTime.Now,
                    DiaChiGiao = model.DiaChi,
                    GhiChu = model.GhiChu,
                    TrangThai = "Chờ xác nhận",
                    TongTien = gioHangVM.Subtotal,
                    SoTienGiam = gioHangVM.CouponDiscount,
                    TongThanhToan = gioHangVM.Total,
                    PhuongThucThanhToan = "COD"
                };

                // BƯỚC 3: XỬ LÝ VOUCHER (Giữ nguyên)
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
                await db.SaveChangesAsync(); // Lưu để lấy MaDh

                // BƯỚC 4: TẠO CHI TIẾT ĐƠN HÀNG (Giữ nguyên)
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

                // BƯỚC 5: DỌN DẸP GIỎ HÀNG (Giữ nguyên)
                HttpContext.Session.Remove(GioHangController.CART_KEY);
                HttpContext.Session.Remove(GioHangController.COUPON_KEY);

                // BƯỚC 6: CHUYỂN TRANG THÀNH CÔNG (Giữ nguyên)
                return RedirectToAction("DatHangThanhCong", new { id = donHang.MaDh });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // BÂY GIỜ HÃY DÙNG DEBUGGER (F5) VÀ XEM 'ex.InnerException.Message'
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại.");
                return View(model);
            }
        }

        // 3. TRANG THÀNH CÔNG
        public IActionResult DatHangThanhCong(int id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }
    }
}