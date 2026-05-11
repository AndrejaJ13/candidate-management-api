namespace Zadatak.Models;

public class Candidate
{
    public int Id { get; set; }

    public required string FullName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public required string ContactNumber { get; set; }

    public required string Email { get; set; }

    public required string NormalizedEmail { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];
}
