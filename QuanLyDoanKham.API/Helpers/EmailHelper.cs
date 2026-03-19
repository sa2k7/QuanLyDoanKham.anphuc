using System;

namespace QuanLyDoanKham.API.Helpers
{
    public static class EmailHelper
    {
        /// <summary>
        /// Giả lập gửi Email thông báo tài khoản cho nhân sự mới.
        /// Trong thực tế, bạn sẽ dùng SmtpClient hoặc dịch vụ như SendGrid/Mailgun.
        /// </summary>
        public static void SendAccountCredentialNotification(string toEmail, string fullName, string username, string initialPassword)
        {
            Console.WriteLine("================================================================");
            Console.WriteLine($"[EMAIL SIMULATOR] Đã gửi thông báo tới: {toEmail}");
            Console.WriteLine($"Người nhận: {fullName}");
            Console.WriteLine($"Nội dung: Chào mừng bạn gia nhập hệ thống HealthCare.");
            Console.WriteLine($"          Tài khoản của bạn đã được khởi tạo:");
            Console.WriteLine($"          - Tên đăng nhập: {username}");
            Console.WriteLine($"          - Mật khẩu mặc định: {initialPassword}");
            Console.WriteLine($"          Vui lòng đăng nhập và đổi mật khẩu để bảo mật.");
            Console.WriteLine("================================================================");
        }

        public static void SendRoleChangeNotification(string toEmail, string fullName, string newRole)
        {
            Console.WriteLine("================================================================");
            Console.WriteLine($"[EMAIL SIMULATOR] Đã gửi thông báo tới: {toEmail}");
            Console.WriteLine($"Người nhận: {fullName}");
            Console.WriteLine($"Nội dung: Vai trò hệ thống của bạn đã được cập nhật.");
            Console.WriteLine($"          - Vai trò mới: {newRole}");
            Console.WriteLine($"          Vui lòng kiểm tra lại quyền hạn trong lần đăng nhập tới.");
            Console.WriteLine("================================================================");
        }
    }
}
