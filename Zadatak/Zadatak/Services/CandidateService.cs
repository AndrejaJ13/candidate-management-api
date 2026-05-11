using Microsoft.EntityFrameworkCore;
using Zadatak.Data;
using Zadatak.Dtos;
using Zadatak.Mapping;
using Zadatak.Models;
using Zadatak.Validation;

namespace Zadatak.Services;

public class CandidateService(AppDbContext dbContext) : ICandidateService
{
    public async Task<CandidateResponse> CreateAsync(
        CreateCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        CandidateValidator.Validate(request.FullName, request.DateOfBirth, request.ContactNumber, request.Email);

        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Email);
        var emailExists = await dbContext.Candidates
            .AnyAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("Candidate with the same email already exists.");
        }

        var skillIds = await GetExistingSkillIdsAsync(request.SkillIds, cancellationToken);
        var candidate = new Candidate
        {
            FullName = TextNormalizer.NormalizeWhiteSpace(request.FullName),
            DateOfBirth = request.DateOfBirth,
            ContactNumber = request.ContactNumber.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            CandidateSkills = skillIds.Select(skillId => new CandidateSkill { SkillId = skillId }).ToList()
        };

        dbContext.Candidates.Add(candidate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(candidate.Id, cancellationToken))!;
    }

    public async Task<CandidateResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var candidate = await GetCandidateQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return candidate is null ? null : CandidateMapper.ToResponse(candidate);
    }

    public async Task<CandidateResponse?> UpdateAsync(
        int id,
        UpdateCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        CandidateValidator.Validate(request.FullName, request.DateOfBirth, request.ContactNumber, request.Email);

        var candidate = await dbContext.Candidates
            .Include(candidate => candidate.CandidateSkills)
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (candidate is null)
        {
            return null;
        }

        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Email);
        var emailExists = await dbContext.Candidates
            .AnyAsync(
                other => other.Id != id && other.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("Candidate with the same email already exists.");
        }

        var skillIds = await GetExistingSkillIdsAsync(request.SkillIds, cancellationToken);

        candidate.FullName = TextNormalizer.NormalizeWhiteSpace(request.FullName);
        candidate.DateOfBirth = request.DateOfBirth;
        candidate.ContactNumber = request.ContactNumber.Trim();
        candidate.Email = request.Email.Trim();
        candidate.NormalizedEmail = normalizedEmail;

        candidate.CandidateSkills.Clear();
        foreach (var skillId in skillIds)
        {
            candidate.CandidateSkills.Add(new CandidateSkill
            {
                CandidateId = candidate.Id,
                SkillId = skillId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(candidate.Id, cancellationToken))!;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var candidate = await dbContext.Candidates
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (candidate is null)
        {
            return false;
        }

        dbContext.Candidates.Remove(candidate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CandidateResponse?> AddSkillAsync(
        int candidateId,
        int skillId,
        CancellationToken cancellationToken = default)
    {
        var candidateExists = await dbContext.Candidates
            .AnyAsync(candidate => candidate.Id == candidateId, cancellationToken);

        if (!candidateExists)
        {
            return null;
        }

        var skillExists = await dbContext.Skills
            .AnyAsync(skill => skill.Id == skillId, cancellationToken);

        if (!skillExists)
        {
            throw new InvalidOperationException("Skill does not exist.");
        }

        var alreadyAssigned = await dbContext.CandidateSkills
            .AnyAsync(
                candidateSkill => candidateSkill.CandidateId == candidateId && candidateSkill.SkillId == skillId,
                cancellationToken);

        if (alreadyAssigned)
        {
            return await GetByIdAsync(candidateId, cancellationToken);
        }

        dbContext.CandidateSkills.Add(new CandidateSkill
        {
            CandidateId = candidateId,
            SkillId = skillId
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(candidateId, cancellationToken);
    }

    public async Task<CandidateResponse?> RemoveSkillAsync(
        int candidateId,
        int skillId,
        CancellationToken cancellationToken = default)
    {
        var candidateExists = await dbContext.Candidates
            .AnyAsync(candidate => candidate.Id == candidateId, cancellationToken);

        if (!candidateExists)
        {
            return null;
        }

        var candidateSkill = await dbContext.CandidateSkills
            .FirstOrDefaultAsync(
                candidateSkill => candidateSkill.CandidateId == candidateId && candidateSkill.SkillId == skillId,
                cancellationToken);

        if (candidateSkill is null)
        {
            return await GetByIdAsync(candidateId, cancellationToken);
        }

        dbContext.CandidateSkills.Remove(candidateSkill);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(candidateId, cancellationToken);
    }

    public async Task<List<CandidateResponse>> SearchAsync(
        string? name,
        IReadOnlyCollection<string>? skills,
        CancellationToken cancellationToken = default)
    {
        var query = GetCandidateQuery();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var trimmedName = name.Trim();
            query = query.Where(candidate => EF.Functions.Like(candidate.FullName, $"%{trimmedName}%"));
        }

        var normalizedSkills = skills?
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(TextNormalizer.NormalizeSkillName)
            .Distinct()
            .ToList() ?? [];

        foreach (var normalizedSkill in normalizedSkills)
        {
            query = query.Where(candidate =>
                candidate.CandidateSkills.Any(candidateSkill =>
                    candidateSkill.Skill.NormalizedName == normalizedSkill));
        }

        var candidates = await query
            .OrderBy(candidate => candidate.FullName)
            .ToListAsync(cancellationToken);

        return candidates.Select(CandidateMapper.ToResponse).ToList();
    }

    private async Task<List<int>> GetExistingSkillIdsAsync(
        IEnumerable<int> requestedSkillIds,
        CancellationToken cancellationToken)
    {
        var skillIds = requestedSkillIds.Distinct().ToList();

        if (skillIds.Count == 0)
        {
            return skillIds;
        }

        var existingSkillIds = await dbContext.Skills
            .Where(skill => skillIds.Contains(skill.Id))
            .Select(skill => skill.Id)
            .ToListAsync(cancellationToken);

        if (existingSkillIds.Count != skillIds.Count)
        {
            throw new InvalidOperationException("One or more skills do not exist.");
        }

        return existingSkillIds;
    }

    private IQueryable<Candidate> GetCandidateQuery()
    {
        return dbContext.Candidates
            .Include(candidate => candidate.CandidateSkills)
            .ThenInclude(candidateSkill => candidateSkill.Skill);
    }

}
