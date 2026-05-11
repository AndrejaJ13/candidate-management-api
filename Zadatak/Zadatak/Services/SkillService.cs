using Microsoft.EntityFrameworkCore;
using Zadatak.Data;
using Zadatak.Dtos;
using Zadatak.Mapping;
using Zadatak.Models;

namespace Zadatak.Services;

public class SkillService(AppDbContext dbContext) : ISkillService
{
    public async Task<SkillResponse> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default)
    {
        var name = TextNormalizer.NormalizeWhiteSpace(request.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Skill name is required.", nameof(request));
        }

        var normalizedName = TextNormalizer.NormalizeSkillName(name);
        var alreadyExists = await dbContext.Skills
            .AnyAsync(skill => skill.NormalizedName == normalizedName, cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException("Skill with the same name already exists.");
        }

        var skill = new Skill
        {
            Name = name,
            NormalizedName = normalizedName
        };

        dbContext.Skills.Add(skill);
        await dbContext.SaveChangesAsync(cancellationToken);

        return SkillMapper.ToResponse(skill);
    }

    public async Task<List<SkillResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Skills
            .OrderBy(skill => skill.Name)
            .Select(skill => SkillMapper.ToResponse(skill))
            .ToListAsync(cancellationToken);
    }

    public async Task<SkillResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Skills
            .Where(skill => skill.Id == id)
            .Select(skill => SkillMapper.ToResponse(skill))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
