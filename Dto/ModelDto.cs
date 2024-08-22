namespace api_elise.Dto
{
    public class ModelDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public ICollection<QRCodeDto> QRCodes { get; set; }

        public ModelDto()
        {
            QRCodes = new List<QRCodeDto>();
        }
    }
}
