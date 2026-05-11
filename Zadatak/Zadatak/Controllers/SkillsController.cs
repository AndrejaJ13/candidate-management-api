using Microsoft.AspNetCore.Mvc;
using Zadatak.Dtos;
using Zadatak.Services;

namespace Zadatak.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController(ISkillService skillService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SkillResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var skills = await skillService.GetAllAsync(cancellationToken);

        return Ok(skills);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var skill = await skillService.GetByIdAsync(id, cancellationToken);

        return skill is null ? NotFound() : Ok(skill);
    }

    [HttpPost]
    public async Task<ActionResult<SkillResponse>> Create(
        CreateSkillRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var skill = await skillService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = skill.Id }, skill);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }
}
