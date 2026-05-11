using Zadatak.Dtos;
using Zadatak.Models;

namespace Zadatak.Mapping;

public static class CandidateMapper
{
    public static CandidateResponse ToResponse(Candidate candidate)
    {
        return new CandidateResponse
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            DateOfBirth = candidate.DateOfBirth,
            ContactNumber = candidate.ContactNumber,
            Email = candidate.Email,
            Skills = candidate.CandidateSkills
                .Select(candidateSkill => SkillMapper.ToResponse(candidateSkill.Skill))
                .OrderBy(skill => skill.Name)
                .ToList()
        };
    }
}
