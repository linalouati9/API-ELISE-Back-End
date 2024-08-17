using System.Text.Json.Serialization;
using System.Xml;

namespace api_elise.Models
{
    // one-to-many relationship
    public class QRCode
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Xslt { get; set; }
        public int ModelId { get; set; }

    }
}
