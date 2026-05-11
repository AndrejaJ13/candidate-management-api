namespace Zadatak.Dtos;

public class CandidateResponse
{
    public int Id { get; set; }

    public required string FullName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public required string ContactNumber { get; set; }

    public required string Email { get; set; }

    public List<SkillResponse> Skills { get; set; } = [];
}
