using QRCoder;
using SBS.Application.Features.Customer_Bookings.Interfaces;

namespace SBS.Infrastructure.CustomerBookings.Services;

public class QRCodeService : IQRCodeService
{
    public byte[] GenerateQrCode(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        return qrCodeImage;
    }
}
