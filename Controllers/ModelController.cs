using api_elise.Dto;
using api_elise.Interfaces;
using api_elise.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace api_elise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : Controller
    {
        private readonly IModelRepository _modelRepository;
        private readonly IMapper _mapper;

        public ModelController(IModelRepository modelRepository, IMapper mapper)
        {
            _modelRepository = modelRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ICollection<Model>))]
        public IActionResult GetModels()
        {
            var models = _modelRepository.GetModels() ?? new List<Model>();

            return Ok(models); // Returns a status 200 with the list of models, even if it is empty
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(200, Type = typeof(Model))]
        [ProducesResponseType(404)]
        public IActionResult GetModel(int id)
        {
            // var model = _mapper.Map<ModelDto>(_modelRepository.GetModel(id));
            var model = _modelRepository.GetModel(id);

            if (model == null)
                return NotFound($"Model with ID = {id} does not exist.");

            return Ok(model);
        }

        [HttpGet("title/{title}")]
        [ProducesResponseType(200, Type = typeof(Model))]
        [ProducesResponseType(404)]
        public IActionResult GetModel(string title)
        {
            var model = _modelRepository.GetModel(title);

            // Condition to verif if the model does not exist.
            if (model == null)
                return NotFound($"Model with title = {title} does not exist.");

            return Ok(model);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(ModelDto))]
        [ProducesResponseType(400)]
        public IActionResult CreateModel([FromBody] ModelDto modelDto)
        {
            if (modelDto == null)
                return BadRequest("Model object cannot be null.");


            if (modelDto.QRCodes == null || !modelDto.QRCodes.Any())
                return BadRequest("A model must have at least one QR code.");

            // Transformation of the DTO into a Model entity with AutoMapper
            var model = _mapper.Map<Model>(modelDto);

            // Check that the QR codes contain values ​​for Xslt before saving
            if (model.QRCodes.Any(q => string.IsNullOrEmpty(q.Xslt)))
            {
                return BadRequest("All QR codes must have a non-null Xstl.");
            }

            // Add the model to the database via the repository
            _modelRepository.CreateModel(model);

            // Return a success with the created model
            // var modelDtoToReturn = _mapper.Map<ModelDto>(model);
            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }


        [HttpPut("{id:int}")]
        [ProducesResponseType(200, Type = typeof(ModelDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult UpdateModel(int id, [FromBody] ModelDto updatedModelDto)
        {
            if (updatedModelDto == null)
                return BadRequest("Invalid model object (null).");

            if (updatedModelDto.QRCodes == null || !updatedModelDto.QRCodes.Any())
                return BadRequest("A model must have at least one QR code.");

            // Retrieve the existing model from the repository
            var existingModel = _modelRepository.GetModel(id);
            if (existingModel == null)
            {
                return NotFound($"Model with id '{id}' not found.");
            }

            // Update the existing model with data from updatedModelDto using AutoMapper
            _mapper.Map(updatedModelDto, existingModel);

            // Save the updated model to the repository
            _modelRepository.UpdateModel(id, existingModel);

            // Return the updated model DTO
            return Ok(existingModel);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(200, Type = typeof(Model))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult DeleteModel(int id)
        {
            if (!_modelRepository.Modelexists(id))
                return NotFound($"Model with ID = {id} does not exist.");

            if (!_modelRepository.DeleteModel(id))
            {
                ModelState.AddModelError("", "Something went wrong while deleting the model");
                return StatusCode(500, ModelState);
            }

            return Ok("Model removed successfully.");
        }
    }
}
