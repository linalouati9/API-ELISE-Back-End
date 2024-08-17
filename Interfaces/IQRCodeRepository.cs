using api_elise.Models;

namespace api_elise.Interfaces
{
    public interface IQRCodeRepository
    {
        ICollection<QRCode> getQRCodes();
        QRCode getQRCode(int id);
        QRCode getQRCode(string title);
        bool QRCodeExists(int id);
        bool CreateQRCode(QRCode qrcode);
        bool UpdateQRCode(int id, QRCode qrcode);
        bool DeleteQRCode(int id);
        bool Save();
    }
}
