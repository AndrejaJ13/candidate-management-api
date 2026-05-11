namespace Zadatak.Models;

public class Skill
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string NormalizedName { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];
}
