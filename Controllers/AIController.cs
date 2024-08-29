using api_elise.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;
using EliseAPIService;
using System.Runtime.Serialization;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ApiClient _apiClient;
    public AIController(HttpClient httpClient, ApiClient apiClient)
    {
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        _httpClient = httpClient;
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocumentById([FromQuery] string[] documentId)
    {
        var response = await _apiClient.GetCompleteEliseMail(documentId);
        if (response == null)
        {
             return NotFound("No document found for the provided document IDs.");
        }

        string xmlString = SerializeToXml(response);

        if (!IsValidXml(xmlString))
        {
            BadRequest("The XML string of the API response is not well-formed.");
        }

        return Ok(xmlString);
    }

    private bool IsValidXml(string xmlString)
    {
        try
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString); 
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

    [HttpPost("process")]
    public async Task<IActionResult> ProcessRequest([FromBody] ProcessRequestModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Request))
        {
            // model.XmlDocument can be null : We can describe the query to the LLM without giving it the XML of the document
            return BadRequest("Invalid input");
        }

        var jsonRequest = new
        {
            prompt = model.Request,
            xml = model.XmlDocument
        };

        var jsonContent = JsonConvert.SerializeObject(jsonRequest);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://127.0.0.1:8000/", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);

                string xsltCode = json["response"]?.ToString();
                if (!string.IsNullOrEmpty(xsltCode))
                {
                    string cleanedXsltCode = xsltCode
                        .Replace("\\", "")
                        .Replace("\n", "")
                        .Replace("```xml", "")
                        .Trim()
                        .Replace("\"", "\\\"");

                    return Ok(cleanedXsltCode);
                }
                else
                {
                    return BadRequest("Invalid JSON format: 'response' field is missing");
                }
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error communicating with the AI model");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}
