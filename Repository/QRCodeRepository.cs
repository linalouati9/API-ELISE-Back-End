using api_elise.Data;
using api_elise.Interfaces;
using api_elise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_elise.Repository
{
    public class QRCodeRepository : IQRCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public QRCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICollection<QRCode> getQRCodes()
        {
            return _context.QRCodes.ToList();
        }

        public QRCode getQRCode(int id)
        {
            return _context.QRCodes.Where(q => q.Id == id).FirstOrDefault();
        }

        public QRCode getQRCode(string title)
        {
            return _context.QRCodes.Where(q => q.Title == title).FirstOrDefault();
        }

        public bool QRCodeExists(int id)
        {
            return _context.QRCodes.Any(q => q.Id == id);
        }

        public bool CreateQRCode(QRCode qrcode)
        {
            _context.QRCodes.Add(qrcode);
            return Save();
        }

        public bool UpdateQRCode(int id, QRCode qrcode)
        {
            _context.QRCodes.Update(qrcode);
            return Save();
        }

        public bool DeleteQRCode(int id)
        {
            var qrcode = getQRCode(id);
            _context.QRCodes.Remove(qrcode);

            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        
    }
}
