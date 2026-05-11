using Zadatak.Dtos;

namespace Zadatak.Services;

public interface ICandidateService
{
    Task<CandidateResponse> CreateAsync(CreateCandidateRequest request, CancellationToken cancellationToken = default);

    Task<CandidateResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CandidateResponse?> UpdateAsync(int id, UpdateCandidateRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<CandidateResponse?> AddSkillAsync(int candidateId, int skillId, CancellationToken cancellationToken = default);

    Task<CandidateResponse?> RemoveSkillAsync(int candidateId, int skillId, CancellationToken cancellationToken = default);

    Task<List<CandidateResponse>> SearchAsync(
        string? name,
        IReadOnlyCollection<string>? skills,
        CancellationToken cancellationToken = default);
}
