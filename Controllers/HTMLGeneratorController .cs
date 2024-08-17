using Microsoft.AspNetCore.Mvc;
using api_elise.Interfaces;
using AutoMapper;
using api_elise.Helper;
using api_elise.Models;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using EliseAPIService;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using QRCoder;
using iText.Layout;

namespace api_elise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HTMLGeneratorController : ControllerBase
    {
        private readonly IModelRepository _modelRepository;
        private readonly IMapper _mapper;
        private readonly ApiClient _apiClient;

        public HTMLGeneratorController(IModelRepository modelRepository, IMapper mapper, ApiClient apiClient)
        {
            _modelRepository = modelRepository;
            _mapper = mapper;
            _apiClient = apiClient;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ExtractInfosFromMails([FromQuery] int ModelId, [FromQuery] string[] mailIdList)
        {
            Model model = _modelRepository.GetModel(ModelId);

            if (model == null)
            {
                return NotFound($"Model with id {ModelId} does not exist!");
            }

            var response = await _apiClient.GetCompleteEliseMail(mailIdList);
            if (response == null)
            {
                return NotFound("Response : No Mails found.");
            }

            // Serialize the response to an XML string
            string xmlString = SerializeToXml(response);

            // Validate XML String
            if (!IsValidXml(xmlString))
            {
                return BadRequest("The XML string of the API response is not well-formed.");
            }

            // Regular expression to find content between <success></success> tags
            string pattern = @"<success>(.*?)</success>";

            // Search for content
            Match match = Regex.Match(xmlString, pattern);

            if (match.Success)
            {
                string content = match.Groups[1].Value;

                if (content == "false")
                {
                    return NotFound("No Mails found : success = " + content);
                }
            }
            else
            {
                return NotFound("The 'success' tag is not found to analyze the response");
            }

            List<string> xsltContentList;
            try
            {
                xsltContentList = LoadXSLFromQRCodes(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while loading XSLT content: {ex.Message}");
            }

            List<string> textList = new List<string>();

            foreach (var xsltContent in xsltContentList)
            {
                try
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();

                    using (XmlReader xslReader = XmlReader.Create(new StringReader(xsltContent)))
                    {
                        xslt.Load(xslReader);
                    }

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xmlString);

                    // Use StringWriter to get the output as a single line of text
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    using (XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Indent = false,
                        NewLineHandling = NewLineHandling.None,
                        ConformanceLevel = ConformanceLevel.Fragment // Set ConformanceLevel to Fragment
                    }))
                    {
                        xslt.Transform(xmlDocument, xw);
                    }

                    // Add the resulting text to the list
                    textList.Add(sb.ToString().Trim());
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error during XSLT transformation: {ex.Message}");
                }
            }

            return Ok(textList);
        }


        // Method to validate if the XML string is well-formed
        private bool IsValidXml(string xmlString)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlString); // Load XML to validate
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        private string SerializeToXml(ResponseCompleteEliseMailResponse response)
        {
            var serializer = new DataContractSerializer(typeof(ResponseCompleteEliseMailResponse));

            using (var memoryStream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memoryStream))
            {
                serializer.WriteObject(xmlWriter, response);
                xmlWriter.Flush();
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // Method to load XSL from QR codes
        private List<string> LoadXSLFromQRCodes(Model model)
        {
            List<string> xsltContent = new List<string>();

            foreach (var qrcode in model.QRCodes)
            {
                xsltContent.Add(qrcode.Xslt);
            }

            return xsltContent;
        }
        
        private void SavePdfWithHtmlAndQRCodes(List<string> textList, string filePath = "C:/Users/user/source/repos/api-elise/Test_Data/")
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    using (PdfWriter writer = new PdfWriter(memoryStream))
                    {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdf = new PdfDocument(writer))
                        using (Document document = new Document(pdf))
                        {
                            for (int i = 0; i < textList.Count; i++)
                            {
                                string text = textList[i];

                                // Add a QR code to the page
                                var qrCodeData = GenerateQrCode(text);
                                byte[] qrImageData = Convert.FromBase64String(qrCodeData);
                                ImageData imageData = ImageDataFactory.Create(qrImageData);
                                Image qrImage = new Image(imageData).SetAutoScale(true);
                                document.Add(qrImage);

                                // Add content to the page
                                document.Add(new Paragraph(text).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(20));

                                // Check if this is not the last item
                                if (i < textList.Count - 1)
                                {
                                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                                }
                            }
                        }
                    }

                    // Save to file
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.WriteTo(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error generating and saving PDF", ex);
                }
            }
        }

        private string GenerateQrCode(string content)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeImage);
            }
        }

        [HttpPost("GeneratePdfAndSave")]
        public IActionResult GeneratePdfAndSave([FromBody] List<string> textList)
        {
            // Generate a timestamp for the PDF filename
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string pdfName = $"PDF_{timeStamp}.pdf";

            string filePath = Path.Combine("C:/Users/user/source/repos/api-elise/Test_Data/", pdfName);

            try
            {
                SavePdfWithHtmlAndQRCodes(textList, filePath);
                ScheduleFileRemoval(filePath);
                return Ok($"PDF saved to {filePath}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void ScheduleFileRemoval(string filePath)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(3)); // Wait for 3 minutes
                RemoveFile(filePath);
            });
        }

        private void RemoveFile(string filePath)
        {
            System.IO.File.Delete(filePath);
        }
    }
}
