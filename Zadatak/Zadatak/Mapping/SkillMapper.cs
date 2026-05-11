using Zadatak.Dtos;
using Zadatak.Models;

namespace Zadatak.Mapping;

public static class SkillMapper
{
    public static SkillResponse ToResponse(Skill skill)
    {
        return new SkillResponse
        {
            Id = skill.Id,
            Name = skill.Name
        };
    }
}
