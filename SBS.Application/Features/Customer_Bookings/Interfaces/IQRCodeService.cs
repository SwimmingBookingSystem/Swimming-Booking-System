namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface IQRCodeService
{
    byte[] GenerateQrCode(string data);
}
