using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using doan3.Models;

namespace doan3.Controllers
{
   
   
    public class ThanhToanController : Controller
    {
        private LTW_DatVeXemPhimEntities db = new LTW_DatVeXemPhimEntities();

       
        public ActionResult Index(long lichChieuId, string lockedSeatIds)
        {
            if (Session["USER_SESSION"] == null) return RedirectToAction("Index_DangNhap", "Login");

            var lichChieu = LayThongTinLichChieu(lichChieuId);
            if (lichChieu == null) return HttpNotFound();

            var danhSachIdGhe = TachChuoiIdGhe(lockedSeatIds);
            var danhSachGheHienThi = LayThongTinGhe(danhSachIdGhe);
            var soGiayConLai = TinhThoiGianGiuGheConLai(lichChieuId, danhSachIdGhe);
           
            var khachHangId = LayMaKhachHangTuSession();

            
            var diemHienCo = 0;
            if (khachHangId.HasValue)
            {
                diemHienCo = LayDiemThanhVien(khachHangId.Value);
            }

            var model = new ThanhToanViewModel
            {
                LichChieuId = lichChieuId,
                LockedSeatIds = lockedSeatIds,
                TenPhim = lichChieu.Phim.TenPhim,
                TenRap = lichChieu.Phong_Chieu.Rap_Chieu.TenRap,
                PhongChieu = lichChieu.Phong_Chieu.TenPhong,
                SuatChieu = lichChieu.ThoiGianBatDau.HasValue ? lichChieu.ThoiGianBatDau.Value.ToString("HH:mm dd/MM/yyyy") : "",
                DanhSachGhe = string.Join(", ", danhSachGheHienThi.Select(s => s.MaGhe)),
                TongTien = danhSachGheHienThi.Sum(s => s.GiaTien.GetValueOrDefault()),
                SoGiayConLai = soGiayConLai,
                DiemThanhVien = diemHienCo
            };

            // TÍNH NĂNG REDIS 2: LƯU GIỎ HÀNG THÔNG TIN THANH TOÁN VÀO REDIS (TTL 600s / 10 PHÚT)
            var sessionUserObj = Session["USER_SESSION"] as UserLogin;
            if (sessionUserObj != null)
            {
                RedisFeaturesService.SaveCart(sessionUserObj.UserName, lichChieuId, model.DanhSachGhe, model.TongTien, 600);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult CancelTransaction(long lichChieuId, string lockedSeatIds)
        {
            var sessionUserObj = Session["USER_SESSION"] as UserLogin;
            if (sessionUserObj != null)
            {
                // Xóa giỏ hàng trên Redis khi hủy giao dịch
                RedisFeaturesService.ClearCart(sessionUserObj.UserName);
            }

            if (!string.IsNullOrEmpty(lockedSeatIds))
            {
                XoaKhoaGheTamThoi(lichChieuId, lockedSeatIds);
            }
            var idPhim = LayIdPhimTuLichChieu(lichChieuId);
            return RedirectToAction("ChonSuat", "DatVe", new { idPhim = idPhim });
        }

       
        [HttpPost]
        public ActionResult SendOtp()
        {
            var sessionUser = Session["USER_SESSION"] as UserLogin;
            if (sessionUser == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // 1. Tạo mã OTP 6 số lưu trên Redis (Hạn 120s)
            string otpCode = RedisFeaturesService.GenerateOtp(sessionUser.UserName, 120);

            // 2. Lấy Email khách hàng từ CSDL
            string customerEmail = null;
            string fullName = sessionUser.FullName ?? "Khách hàng";
            var khachHang = db.Khach_Hang.FirstOrDefault(kh => kh.UserID == sessionUser.UserID);
            if (khachHang != null)
            {
                customerEmail = khachHang.Email;
            }

            // 3. Thử gửi mã OTP tới Gmail thực tế của khách hàng
            var sendResult = EmailService.SendOtpViaGmail(customerEmail, otpCode, fullName);

            if (sendResult.IsSuccess)
            {
                // Đã gửi mail thành công -> Ẩn OTP trên UI để bảo mật
                return Json(new
                {
                    success = true,
                    otp = (string)null,
                    email = customerEmail,
                    ttl = 120,
                    message = $"Mã OTP 6 số đã được gửi thành công đến Gmail: {customerEmail}. Vui lòng kiểm tra hộp thư!"
                });
            }
            else
            {
                // Chưa bật/cấu hình Gmail SMTP -> Hiển thị OTP trên UI kèm thông báo chế độ Demo
                string displayMsg = !string.IsNullOrEmpty(customerEmail)
                    ? $"[Chế độ Demo] Đã tạo OTP trên Redis cho Gmail ({customerEmail}): {otpCode} (Hạn 120s)"
                    : $"[Chế độ Demo] Đã tạo OTP trên Redis: {otpCode} (Hạn 120s)";

                return Json(new
                {
                    success = true,
                    otp = otpCode,
                    email = customerEmail,
                    ttl = 120,
                    message = displayMsg
                });
            }
        }

        [HttpPost]
        public ActionResult ProcessPayment(long lichChieuId, string lockedSeatIds, string paymentMethod, string otpCode = null, bool dungDiem = false)
        {
            if (Session["USER_SESSION"] == null) return RedirectToAction("Index_DangNhap", "Login");

            var sessionUser = Session["USER_SESSION"] as UserLogin;
            long? maKhachHang = LayMaKhachHangTuSession();

            if (maKhachHang == null) return View("PaymentError", (object)"Lỗi: Không tìm thấy thông tin khách hàng.");

            // KIEU TRA XÁC THỰC MÃ OTP REDIS (NẾU CÓ NHẬP)
            if (!string.IsNullOrWhiteSpace(otpCode))
            {
                bool isValidOtp = RedisFeaturesService.VerifyOtp(sessionUser.UserName, otpCode);
                if (!isValidOtp)
                {
                    return View("PaymentError", (object)"Mã OTP không chính xác hoặc đã hết hạn (120s) trên Redis. Vui lòng lấy mã mới.");
                }
            }

            var danhSachIdGhe = TachChuoiIdGhe(lockedSeatIds);

           
            if (!KiemTraGheConThuocVeMinhKhong(lichChieuId, maKhachHang.Value, danhSachIdGhe))
            {
                return View("PaymentError", (object)"Hết thời gian giữ ghế. Vui lòng chọn lại.");
            }

            var danhSachGheCanMua = LayThongTinGhe(danhSachIdGhe);
            decimal tongTienGoc = danhSachGheCanMua.Sum(i => i.GiaTien.GetValueOrDefault());

            var khachHang = db.Khach_Hang.Find(maKhachHang.Value);
            int diemHienCo = khachHang.DiemThanhVien ?? 0;
            decimal soTienGiam = 0;
            int diemBiTru = 0;

            
            if (sessionUser.GroupID == "2")
            {
                if (dungDiem && diemHienCo >= 20)
                {
                    decimal giaTriDiem = diemHienCo * 1000;
                    if (giaTriDiem >= tongTienGoc)
                    {
                        soTienGiam = tongTienGoc;
                        diemBiTru = (int)(tongTienGoc / 1000);
                    }
                    else
                    {
                        soTienGiam = giaTriDiem;
                        diemBiTru = diemHienCo;
                    }
                }
            }

            decimal tongTienPhaiTra = tongTienGoc - soTienGiam;

            
            using (var giaoDich = db.Database.BeginTransaction())
            {
                try
                {
                    
                    long maDonHang = TaoDonHangMoi(maKhachHang.Value, tongTienPhaiTra);

                  
                    TaoChiTietVe(maDonHang, lichChieuId, danhSachGheCanMua);

                    
                    if (khachHang != null)
                    {
                        
                        khachHang.DiemThanhVien = (khachHang.DiemThanhVien ?? 0) - diemBiTru;

                       
                        if (sessionUser.GroupID == "2")
                        {
                            int diemCongThem = TinhDiemTichLuy(danhSachGheCanMua);
                            khachHang.DiemThanhVien += diemCongThem;
                        }
                       
                    }

                    
                    XoaKhoaGheSauKhiMuaThanhCong(lichChieuId, danhSachIdGhe);
                    RedisFeaturesService.ClearCart(sessionUser.UserName);
                    db.SaveChanges();
                    giaoDich.Commit();

                    // TÍNH NĂNG NEO4J: TỰ ĐỘNG GHI NHẬN LƯỢT ĐẶT VÉ VÀO NEO4J GRAPH
                    try
                    {
                        var idPhim = LayIdPhimTuLichChieu(lichChieuId);
                        var neo4jService = new Neo4jService();
                        neo4jService.AddBooking(sessionUser.UserName, (int)idPhim, "BK" + maDonHang, danhSachGheCanMua.Count, tongTienPhaiTra);
                    }
                    catch { }

                    return RedirectToAction("PaymentSuccess", new { orderId = maDonHang });
                }
                catch (Exception loi)
                {
                    giaoDich.Rollback();
                    return View("PaymentError", (object)("Lỗi hệ thống: " + loi.Message));
                }
            }
        }

   
        public ActionResult PaymentSuccess(long orderId)    
        {
            var donHang = LayThongTinDonHangDayDu(orderId);
            if (donHang == null) return RedirectToAction("Index", "Home");
            return View(donHang);
        }

        public ActionResult PaymentError(string error)
        {
            ViewBag.ErrorMessage = error;
            return View();
        }


   

        private List<GheTinhDiem> LayThongTinGhe(List<long> danhSachIdGhe)
        {
            return (from g in db.Ghe_Ngoi
                    join t in db.TienVes on g.LoaiGhe equals t.LoaiGhe
                    where danhSachIdGhe.Contains(g.GheID)
                    select new GheTinhDiem 
                    {
                        GheID = g.GheID,
                        MaGhe = g.MaGhe,
                        GiaTien = t.GiaTien,
                        LoaiGhe = g.LoaiGhe
                    }).ToList();
        }

      
        private int TinhDiemTichLuy(List<GheTinhDiem> danhSachGhe)
        {
            int tongDiem = 0;
            foreach (var ghe in danhSachGhe)
            {
                
                if (string.IsNullOrEmpty(ghe.LoaiGhe))
                {
                    tongDiem += 1;
                    continue;
                }

               
                string loai = ghe.LoaiGhe.Trim().ToLower();

               
                if (loai.Contains("vip"))
                {
                    tongDiem += 2;
                }
                else if (loai.Contains("doi") || loai.Contains("đôi") || loai.Contains("couple"))
                {
                    tongDiem += 3;
                }
                else
                {
                    
                    tongDiem += 1;
                }
            }
            return tongDiem;
        }

        private void CongDiemThanhVien(long khachHangId, int diemCong)
        {
            var kh = db.Khach_Hang.Find(khachHangId);
            if (kh != null)
            {
                kh.DiemThanhVien = (kh.DiemThanhVien ?? 0) + diemCong;
            }
        }

        private int LayDiemThanhVien(long? khachHangId)
        {
            if (!khachHangId.HasValue) return 0;
            var kh = db.Khach_Hang.Find(khachHangId.Value);
            return kh != null ? (kh.DiemThanhVien ?? 0) : 0;
        }

        private void TaoChiTietVe(long maDonHang, long lichChieuId, List<GheTinhDiem> danhSachGhe)
        {
            foreach (var ghe in danhSachGhe)
            {
                var chiTiet = new Chi_Tiet_Ve
                {
                    DonDatVeID = maDonHang,
                    LichChieuID = lichChieuId,
                    GheID = ghe.GheID,
                    LoaiGhe = ghe.LoaiGhe,
                    LoaiVe = "Nguoi Lon",
                    TrangThaiSuDung = false
                };
                db.Chi_Tiet_Ve.Add(chiTiet);
            }
        }

        
        private long? LayMaKhachHangTuSession()
        {
            var sessionNguoiDung = Session["USER_SESSION"] as UserLogin;
            if (sessionNguoiDung == null || sessionNguoiDung.UserID == 0) return null;
            var khachHang = db.Khach_Hang.FirstOrDefault(kh => kh.UserID == sessionNguoiDung.UserID);
            return khachHang?.KhachHangID;
        }

        private Lich_Chieu LayThongTinLichChieu(long lichChieuId)
        {
            return db.Lich_Chieu.Include(l => l.Phim).Include(l => l.Phong_Chieu.Rap_Chieu).FirstOrDefault(l => l.LichChieuID == lichChieuId);
        }

        private List<long> TachChuoiIdGhe(string chuoiIdGhe)
        {
            if (string.IsNullOrEmpty(chuoiIdGhe)) return new List<long>();
            return chuoiIdGhe.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id.Trim())).ToList();
        }

        private long TinhThoiGianGiuGheConLai(long lichChieuId, List<long> danhSachIdGhe)
        {
            // TÍNH THỜI GIAN KHÓA GHẾ CÒN LẠI TỪ REDIS (TTL)
            return SeatLockService.GetRemainingLockTime(lichChieuId, danhSachIdGhe);
        }

        private void XoaKhoaGheTamThoi(long lichChieuId, string chuoiIdGhe)
        {
            var danhSachId = TachChuoiIdGhe(chuoiIdGhe);
            // XÓA KHÓA GHẾ TRÊN REDIS
            SeatLockService.ReleaseSeatLocks(lichChieuId, danhSachId);
        }

        private long LayIdPhimTuLichChieu(long lichChieuId)
        {
            var lc = db.Lich_Chieu.FirstOrDefault(l => l.LichChieuID == lichChieuId);
            return lc?.PhimID ?? 1;
        }

        private bool KiemTraGheConThuocVeMinhKhong(long lichChieuId, long maKhachHang, List<long> danhSachIdGhe)
        {
            // KIỂM TRA XÁC THỰC KHÓA GHẾ NGUYÊN TỬ TRÊN REDIS
            return SeatLockService.VerifySeatsLockedByCustomer(lichChieuId, danhSachIdGhe, maKhachHang);
        }

        private long TaoDonHangMoi(long maKhachHang, decimal tongTien)
        {
            var donHang = new Don_Dat_Ve
            {
                KhachHangID = maKhachHang,
                TongTienDonHang = tongTien,
                TrangThaiDonHang = "Đã thanh toán",
                ThoiGianDat = DateTime.Now
            };
            db.Don_Dat_Ve.Add(donHang);
            db.SaveChanges();
            return donHang.DonDatVeID;
        }

        private void XoaKhoaGheSauKhiMuaThanhCong(long lichChieuId, List<long> danhSachIdGhe)
        {
            // XÓA KHÓA GHẾ TRÊN REDIS SAU KHIMUA THÀNH CÔNG
            SeatLockService.ReleaseSeatLocks(lichChieuId, danhSachIdGhe);
        }

        private Don_Dat_Ve LayThongTinDonHangDayDu(long maDonHang)
        {
            return db.Don_Dat_Ve
                     .Include("Khach_Hang")
                     .Include("Chi_Tiet_Ve")
                     .Include("Chi_Tiet_Ve.Lich_Chieu")
                     .Include("Chi_Tiet_Ve.Lich_Chieu.Phim")
                     .Include("Chi_Tiet_Ve.Ghe_Ngoi")
                     .FirstOrDefault(d => d.DonDatVeID == maDonHang);
        }
    }
}