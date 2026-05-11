using Microsoft.AspNetCore.Mvc;
using Zadatak.Dtos;
using Zadatak.Services;

namespace Zadatak.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController(ICandidateService candidateService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CandidateResponse>> Create(
        CreateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var candidate = await candidateService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = candidate.Id }, candidate);
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidateResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var candidate = await candidateService.GetByIdAsync(id, cancellationToken);

        return candidate is null ? NotFound() : Ok(candidate);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CandidateResponse>> Update(
        int id,
        UpdateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var candidate = await candidateService.UpdateAsync(id, request, cancellationToken);
            return candidate is null ? NotFound() : Ok(candidate);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await candidateService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{candidateId:int}/skills/{skillId:int}")]
    public async Task<ActionResult<CandidateResponse>> AddSkill(
        int candidateId,
        int skillId,
        CancellationToken cancellationToken)
    {
        try
        {
            var candidate = await candidateService.AddSkillAsync(candidateId, skillId, cancellationToken);
            return candidate is null ? NotFound() : Ok(candidate);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{candidateId:int}/skills/{skillId:int}")]
    public async Task<ActionResult<CandidateResponse>> RemoveSkill(
        int candidateId,
        int skillId,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateService.RemoveSkillAsync(candidateId, skillId, cancellationToken);

        return candidate is null ? NotFound() : Ok(candidate);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<CandidateResponse>>> Search(
        [FromQuery] string? name,
        [FromQuery] List<string>? skills,
        CancellationToken cancellationToken)
    {
        var candidates = await candidateService.SearchAsync(name, skills, cancellationToken);

        return Ok(candidates);
    }
}
