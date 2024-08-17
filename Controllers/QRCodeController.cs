using api_elise.Dto;
using api_elise.Interfaces;
using api_elise.Models;
using api_elise.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace api_elise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeController : Controller
    {
        private readonly IModelRepository _modelRepository;
        private readonly IQRCodeRepository _qrcodeRepository;
        private readonly IMapper _mapper;

        public QRCodeController(IModelRepository modelRepository, IQRCodeRepository qrcodeRepository, IMapper mapper)
        {
            _modelRepository = modelRepository;
            _qrcodeRepository = qrcodeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ICollection<QRCode>))]
        public IActionResult GetQRCodes()
        {
            var qrcodes = _qrcodeRepository.getQRCodes() ?? new List<QRCode>();
            return Ok(qrcodes);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(200, Type = typeof(QRCode))]
        [ProducesResponseType(404)]
        public IActionResult GetQRCode(int id)
        {
            var qrcode = _qrcodeRepository.getQRCode(id);

            if(qrcode == null)
                return NotFound($"QR code with ID = {id} does not exist.");

            return Ok(qrcode);
        }

        [HttpGet("title/{title}")]
        [ProducesResponseType(200, Type = typeof(QRCode))]
        [ProducesResponseType(404)]
        public IActionResult GetModel(string title)
        {
            var qrcode = _qrcodeRepository.getQRCode(title);
            
            if (qrcode == null)
                return NotFound($"QR code with title = {title} does not exist.");

            return Ok(qrcode);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(QRCode))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CreateQRCode([FromQuery] int ModelId, [FromBody] QRCodeDto qrcodedto)
        {

            Model model = _modelRepository.GetModel(ModelId);

            // Check if Model with ModelId exists 
            if (model == null)
                return NotFound($"Model with the provided ID = {ModelId} does not exist..");

            if (qrcodedto == null)
                return BadRequest("QR code object cannot be null.");

            // Map QRCodedto to QRCode entity
            var qrcode = _mapper.Map<QRCode>(qrcodedto);
            qrcode.ModelId = ModelId;

            // Call repository method to create QRCode
            _qrcodeRepository.CreateQRCode(qrcode);

            // Return 201 Created response with the created QRCode object
            return CreatedAtAction(nameof(GetQRCode), new { id = qrcode.Id }, qrcode);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(200, Type = typeof(QRCode))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateQRCode(int id, [FromBody] QRCodeDto UpdatedQrcode)
        {
            QRCode existingQRcode = _qrcodeRepository.getQRCode(id);

            if (existingQRcode == null)
                return NotFound($"QR code with ID = {id} does not exist.");

            // if (UpdatedQrcode == null)
            if(! ModelState.IsValid)
                return BadRequest("Unable to update QR code with a null value.");

            _mapper.Map(UpdatedQrcode, existingQRcode);

            _qrcodeRepository.UpdateQRCode(id, existingQRcode);

            return Ok("QR code updated successfully..");
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(200, Type = typeof(QRCode))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteQRCode(int id)
        {
            QRCode qrcode = _qrcodeRepository.getQRCode(id);

            if (qrcode == null)
                return NotFound($"QR code with ID = {id} does not exists!");

            Model model = _modelRepository.GetModel(qrcode.ModelId);

            // When we delete a qrcode from a model, we must be sure that the model still has at least 1 qrcode
            if (model.QRCodes.Count == 1)
                return BadRequest("When you delete a qrcode from a model, You must be sure that the model still has at least 1 qrcode");

            // Delete QRCode 
            _qrcodeRepository.DeleteQRCode(id);

            return Ok("QR code deleted successfully..");
        }
    }


}
