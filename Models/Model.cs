using System.Text.Json.Serialization;

namespace api_elise.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Template { get; set; } // HTML
        public ICollection<QRCode> QRCodes { get; set; }

        public Model()
        {
            QRCodes = new List<QRCode>();
        }
    }

}
