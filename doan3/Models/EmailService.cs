using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace doan3.Models
{
    public class EmailSendResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class EmailService
    {
        /// <summary>
        /// Gửi mã OTP xác thực qua Gmail SMTP với giao diện HTML chuẩn thương hiệu Rạp Phim
        /// </summary>
        public static EmailSendResult SendOtpViaGmail(string toEmail, string otpCode, string customerName = "Khách hàng")
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return new EmailSendResult { IsSuccess = false, Message = "Địa chỉ Email của tài khoản không hợp lệ." };
            }

            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            int smtpPort = int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out int port) ? port : 587;
            string smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"] ?? "";
            string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            bool enableRealGmailOtp = bool.TryParse(ConfigurationManager.AppSettings["EnableRealGmailOtp"], out bool isReal) && isReal;

            if (!enableRealGmailOtp || string.IsNullOrWhiteSpace(smtpEmail) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                return new EmailSendResult { IsSuccess = false, Message = "CHƯA_CAU_HINH_SMTP" };
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(smtpEmail, "MOVANA CINEMA - Rạp Phim Trực Tuyến");
                    message.To.Add(new MailAddress(toEmail.Trim()));
                    message.Subject = $"[{otpCode}] Mã OTP Xác Thực Giao Dịch Đặt Vé - Movana Cinema";
                    
                    message.IsBodyHtml = true;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.BodyEncoding = Encoding.UTF8;
                    message.HeadersEncoding = Encoding.UTF8;

                    string safeName = string.IsNullOrWhiteSpace(customerName) ? "Quý khách" : HttpUtility.HtmlEncode(customerName);

                    // Template HTML Email Tiếng Việt chuẩn 100%, thiết kế đẹp mắt giống rạp CGV / Lotte
                    message.Body = $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Mã OTP Xác Thực Thanh Toán - Movana Cinema</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #0b0e26; font-family: 'Segoe UI', Helvetica, Arial, sans-serif;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #0b0e26; padding: 30px 10px;"">
        <tr>
            <td align=""center"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #15183c; border-radius: 16px; border: 1px solid rgba(249, 224, 0, 0.25); box-shadow: 0 12px 40px rgba(0, 0, 0, 0.6); overflow: hidden;"">
                    
                    <!-- HEADER LOGO BANNER -->
                    <tr>
                        <td align=""center"" style=""background: linear-gradient(135deg, #1e1b4b 0%, #0a0d29 100%); padding: 35px 20px; border-bottom: 3px solid #f9e000;"">
                            <h1 style=""color: #f9e000; margin: 0; font-size: 28px; font-weight: 900; letter-spacing: 3px; text-transform: uppercase;"">
                                🎬 MOVANA CINEMA
                            </h1>
                            <p style=""color: #94a3b8; margin: 6px 0 0 0; font-size: 13px; text-transform: uppercase; letter-spacing: 2px;"">
                                Hệ Thống Đặt Vé Xem Phim Trực Tuyến
                            </p>
                        </td>
                    </tr>

                    <!-- BODY CONTENT -->
                    <tr>
                        <td style=""padding: 35px 30px; color: #e2e8f0; font-size: 15px; line-height: 1.6;"">
                            <p style=""font-size: 17px; margin-top: 0; color: #ffffff; font-weight: 600;"">
                                Kính gửi <span style=""color: #f9e000;"">{safeName}</span>,
                            </p>
                            <p style=""color: #cbd5e1; margin-bottom: 25px;"">
                                Cảm ơn Quý khách đã lựa chọn dịch vụ đặt vé tại <strong>Movana Cinema</strong>. Bạn vừa thực hiện yêu cầu xác thực giao dịch thanh toán vé xem phim.
                            </p>

                            <!-- OTP CONTAINER CARD -->
                            <div style=""background-color: #0a0d29; border: 2px dashed #f9e000; border-radius: 14px; padding: 30px 20px; text-align: center; margin: 30px 0;"">
                                <span style=""font-size: 13px; color: #94a3b8; text-transform: uppercase; letter-spacing: 1.5px; font-weight: 600; display: block; margin-bottom: 12px;"">
                                    MÃ XÁC THỰC OTP CỦA BẠN
                                </span>
                                <div style=""font-size: 42px; font-weight: 900; color: #f9e000; letter-spacing: 12px; font-family: 'Courier New', Courier, monospace; padding: 10px 0; text-shadow: 0 0 15px rgba(249, 224, 0, 0.4);"">
                                    {otpCode}
                                </div>
                                <div style=""margin-top: 12px; font-size: 14px; color: #ff6b6b; font-weight: 600;"">
                                    ⏱️ Mã có hiệu lực trong vòng <strong>120 giây (2 phút)</strong>
                                </div>
                            </div>

                            <!-- WARNING SECURITY BOX -->
                            <div style=""background: rgba(239, 68, 68, 0.12); border-left: 4px solid #ef4444; padding: 15px; border-radius: 6px; font-size: 13.5px; color: #fca5a5; margin-bottom: 25px; line-height: 1.5;"">
                                <strong>⚠️ Lưu ý bảo mật quan trọng:</strong><br/>
                                • Tuyệt đối <strong>KHÔNG chia sẻ mã OTP này</strong> cho bất kỳ ai (kể cả nhân viên hỗ trợ rạp phim).<br/>
                                • Movana Cinema không bao giờ yêu cầu cung cấp mã OTP qua điện thoại hoặc tin nhắn.
                            </div>

                            <p style=""font-size: 13.5px; color: #94a3b8; margin-bottom: 0;"">
                                Nếu Quý khách không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ khách hàng để bảo vệ tài khoản.
                            </p>
                        </td>
                    </tr>

                    <!-- FOOTER -->
                    <tr>
                        <td align=""center"" style=""background-color: #0a0d29; padding: 25px 30px; border-top: 1px solid rgba(255, 255, 255, 0.08); font-size: 12.5px; color: #64748b; line-height: 1.7;"">
                            <p style=""margin: 0 0 6px 0; color: #cbd5e1; font-weight: 700; font-size: 13.5px;"">
                                HỆ THỐNG RẠP CHIẾU PHIM MOVANA CINEMA
                            </p>
                            <p style=""margin: 0 0 4px 0;"">
                                📍 Trụ sở chính: Tòa nhà Movana Tower, Quận Cầu Giấy, Hà Nội
                            </p>
                            <p style=""margin: 0 0 8px 0;"">
                                📞 Tổng đài CSKH: <strong>1900 6868</strong> | ✉️ Email: <strong>support@movanacinema.vn</strong>
                            </p>
                            <p style=""margin: 0; font-size: 11.5px; color: #475569;"">
                                © 2026 Movana Cinema Việt Nam. Tất cả các quyền được bảo lưu.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                    using (var smtp = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                    }
                }

                return new EmailSendResult { IsSuccess = true, Message = $"Mã OTP đã được gửi thành công đến Gmail {toEmail}!" };
            }
            catch (Exception ex)
            {
                return new EmailSendResult { IsSuccess = false, Message = $"Lỗi khi kết nối Gmail SMTP: {ex.Message}" };
            }
        }
    }
}
