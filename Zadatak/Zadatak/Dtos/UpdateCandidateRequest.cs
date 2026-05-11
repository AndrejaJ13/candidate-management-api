namespace Zadatak.Dtos;

public class UpdateCandidateRequest
{
    public required string FullName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public required string ContactNumber { get; set; }

    public required string Email { get; set; }

    public List<int> SkillIds { get; set; } = [];
}
