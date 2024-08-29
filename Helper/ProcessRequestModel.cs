using System.ComponentModel.DataAnnotations;
namespace api_elise.Helper
{
    public class ProcessRequestModel
    {
        [Required]
        public string Request { get; set; }

        public string? XmlDocument { get; set; } // Nullable string
    }

}
