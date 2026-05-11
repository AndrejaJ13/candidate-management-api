using Zadatak.Dtos;

namespace Zadatak.Services;

public interface ISkillService
{
    Task<SkillResponse> CreateAsync(CreateSkillRequest request, CancellationToken cancellationToken = default);

    Task<List<SkillResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SkillResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
