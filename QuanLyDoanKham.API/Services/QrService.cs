using QRCoder;
using System.Text;

namespace QuanLyDoanKham.API.Services;

public class QrService
{
    public string GenerateQr(string text)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var code = new PngByteQRCode(data);
        var bytes = code.GetGraphic(4);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
