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
using iText.Layout.Element;
using QRCoder;
using HtmlAgilityPack;

namespace api_elise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeparatorSheetGenerator : ControllerBase   
    {
        private readonly IModelRepository _modelRepository;
        private readonly IMapper _mapper;
        private readonly ApiClient _apiClient;

        public SeparatorSheetGenerator(IModelRepository modelRepository, IMapper mapper, ApiClient apiClient)
        {
            _modelRepository = modelRepository;
            _mapper = mapper;
            _apiClient = apiClient;
        }

        // Preview Method 
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
        
        // Generate separatorSheet to dowload 
        private List<string> GenerateQrCodesWithFilePaths(List<string> content)
        {
            List<string> imageUrls = new List<string>();
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Test_Data");

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                foreach (string item in content)
                {
                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(item, QRCodeGenerator.ECCLevel.Q))
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(20);

                        // Generate file name based on timestamp
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                        string fileName = $"qrcode_{timestamp}.png";
                        string filePath = Path.Combine(basePath, fileName);

                        // Save the image as a PNG file
                        System.IO.File.WriteAllBytes(filePath, qrCodeImage);

                        imageUrls.Add(filePath);
                    }
                }
            }
            return imageUrls;
        }
        private string InsertQrCodesIntoTemplate(string template, List<string> images)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(template);

            for (int i = 0; i < images.Count; i++)
            {
                string imgId = $"qrcode{i + 1}";
                var imgNode = htmlDoc.DocumentNode.SelectSingleNode($"//img[@id='{imgId}']");

                if (imgNode != null)
                {
                    imgNode.SetAttributeValue("src", images[i]);
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
        
        
        [HttpPost("GenerateSeparatorSheet")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GeneratorSeparatorSheetWithQrCodes([FromQuery] int ModelId, [FromBody] List<string> content)
        {

            Model model = _modelRepository.GetModel(ModelId);

            if (model == null)
            {
                return NotFound($"Model with id {ModelId} does not exist!");
            }
            
            string template = model.Template;
            
            if (string.IsNullOrEmpty(template))
            {
                return BadRequest($"Model with id {ModelId} does not contain a valid template");
            }
            
            // If all is okayy
            List<string> qrCodes = GenerateQrCodesWithFilePaths(content);
            string updatedTemplate = InsertQrCodesIntoTemplate(template, qrCodes);
           
            return Ok(updatedTemplate);
        }
        
    }
}
