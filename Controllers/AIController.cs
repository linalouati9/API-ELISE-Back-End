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
    public async Task<IActionResult> ProcessRequest([FromBody] string request, [FromBody] string xmlDocument /* If GetDocumentById() return a valid xml, switch it to this method */)
    {

        var jsonRequest = new { prompt = request, xml = xmlDocument };

        var jsonContent = JsonConvert.SerializeObject(jsonRequest);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send the POST request
        HttpResponseMessage response = await _httpClient.PostAsync("http://127.0.0.1:8000/", content);

        if (response.IsSuccessStatusCode)
        {
            // Read the response content as a string
            string result = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            JObject json = JObject.Parse(result);

            // Extract the XSLT code
            string xslt_code = json["response"]?.ToString();

            if (xslt_code != null)
            {
                // Clean the XSLT code
                string cleaned_xslt_code = xslt_code
                    .Replace("\\", "")
                    .Replace("\n", "")
                    .Replace("```xml", "")
                    .Trim();

                // Escape quotes
                cleaned_xslt_code = cleaned_xslt_code.Replace("\"", "\\\"");

                return Ok(cleaned_xslt_code);
            }
            else
            {
                return BadRequest("Invalid JSON format");
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error communicating with the AI model");
        }

    }
}
