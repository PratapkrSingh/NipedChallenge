using MedicalData.Models;
using MedicalData.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalData.Controllers
{
    [Authorize(Policy = "GuidelineManagerPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class GuideLineController : ControllerBase
    {
        private readonly IGuidelineRepository _guidelineRepository;
        private readonly ILogger<GuideLineController> _logger;

        public GuideLineController(IGuidelineRepository guidelineRepository, ILogger<GuideLineController> logger)
        {
            _guidelineRepository = guidelineRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(MedicalGuidelines), 200)]
        [ProducesResponseType(404)] // If no guidelines are configured yet
        public async Task<ActionResult<MedicalGuidelines>> GetCurrentGuidelines()
        {
            var guidelines = await _guidelineRepository.GetMedicalGuidelinesAsync();
            if (guidelines == null)
            {
                _logger.LogInformation("No medical guidelines found in the system.");
                return NotFound("No medical guidelines configured.");
            }
            return Ok(guidelines);
        }

        [HttpPut]
        [ProducesResponseType(typeof(MedicalGuidelines), 200)] // OK if updated
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MedicalGuidelines>> SetMedicalGuidelines([FromBody] MedicalGuidelines guidelines)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _guidelineRepository.SaveMedicalGuidelinesAsync(guidelines);
            _logger.LogInformation("Medical guidelines updated successfully.");
            return Ok(guidelines);
        }
    }
}
