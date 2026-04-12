using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyDoanKham.API.Services;

public class QrService
{
    private readonly IConfiguration _configuration;

    public QrService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateQr(string text)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var code = new PngByteQRCode(data);
        var bytes = code.GetGraphic(4);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    /// <summary>
    /// Tạo Token bảo mật bằng HMAC-SHA256
    /// Định dạng: Base64(payload).Base64(signature)
    /// </summary>
    public string GenerateSignedToken(int groupId, int expiryHours = 12)
    {
        var secret = _configuration["AppSettings:Token"] ?? "Default_Secret_Key_For_QR_HMAC";
        var expiry = DateTimeOffset.UtcNow.AddHours(expiryHours).ToUnixTimeSeconds();
        var payload = $"{groupId}:{expiry}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        var signatureBase64 = Convert.ToBase64String(signature);

        return $"{payloadBase64}.{signatureBase64}";
    }

    /// <summary>
    /// Xác thực Token có chữ ký hợp lệ
    /// </summary>
    public bool ValidateSignedToken(string token, out int groupId, out string error)
    {
        groupId = 0;
        error = "";

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2)
            {
                error = "Định dạng Token không hợp lệ.";
                return false;
            }

            var payloadBase64 = parts[0];
            var providedSignatureBase64 = parts[1];

            var secret = _configuration["AppSettings:Token"] ?? "Default_Secret_Key_For_QR_HMAC";
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expectedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignatureBase64 = Convert.ToBase64String(expectedSignature);

            // So sánh Time-invariant để tránh Side-channel attacks
            if (providedSignatureBase64 != expectedSignatureBase64)
            {
                error = "Chữ ký Token không hợp lệ (Tampered).";
                return false;
            }

            var payloadParts = payload.Split(':');
            if (payloadParts.Length != 2 || !int.TryParse(payloadParts[0], out groupId) || !long.TryParse(payloadParts[1], out var expiry))
            {
                error = "Dữ liệu Token hỏng.";
                return false;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
            {
                error = "Mã QR đã hết hạn.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = "Lỗi xác thực: " + ex.Message;
            return false;
        }
    }
}
