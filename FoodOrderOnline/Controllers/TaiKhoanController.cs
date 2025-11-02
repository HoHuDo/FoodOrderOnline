using FoodOrderOnline.Models;
using FoodOrderOnline.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace FoodOrderOnline.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly FoodOrderContext db;

        public TaiKhoanController(FoodOrderContext context)
        {
            db = context;
        }

        #region Đăng ký (Register)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(DangKyVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingAccount = await db.TaiKhoans.FirstOrDefaultAsync(k => k.TenDangNhap == model.TenDangNhap);
                if (existingAccount != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã được sử dụng.");
                    return View(model);
                }

                var existingEmail = await db.KhachHangs.FirstOrDefaultAsync(k => k.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    VaiTro = "KhachHang",
                    TrangThai = true,


                    MatKhau = model.MatKhau
                };

                db.TaiKhoans.Add(taiKhoan);
                await db.SaveChangesAsync();

                var khachHang = new KhachHang
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    MaTk = taiKhoan.MaTk
                };

                db.KhachHangs.Add(khachHang);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại.");
                return View(model);
            }
        }
        #endregion

        #region Đăng nhập (Login)
        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await db.TaiKhoans
                .Include(tk => tk.KhachHang)
                .Include(tk => tk.NhanVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap);

            if (taiKhoan == null || taiKhoan.MatKhau != model.MatKhau || taiKhoan.TrangThai != true)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, taiKhoan.KhachHang?.HoTen ?? taiKhoan.NhanVien?.HoTen ?? taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Email, taiKhoan.KhachHang?.Email ?? taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro ?? "KhachHang")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (taiKhoan.VaiTro == "NhanVien" || taiKhoan.VaiTro == "QuanTriVien")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "TrangChu");
        }
        #endregion

        #region Đăng xuất (Logout)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "TrangChu");
        }
        #endregion
    }
}