using Zadatak.Dtos;
using Zadatak.Services;

namespace Zadatak.Tests.Services;

public class CandidateServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateAsync_AddsCandidateWithSkills()
    {
        var service = new CandidateService(DbContext);

        var candidate = await service.CreateAsync(new CreateCandidateRequest
        {
            FullName = "Jelena Petrovic",
            DateOfBirth = new DateOnly(1998, 5, 12),
            ContactNumber = "+38164555666",
            Email = "jelena.petrovic@example.com",
            SkillIds = [1, 4]
        });

        Assert.True(candidate.Id > 0);
        Assert.Equal("Jelena Petrovic", candidate.FullName);
        Assert.Equal("jelena.petrovic@example.com", candidate.Email);
        Assert.Equal(["C# Programming", "English"], candidate.Skills.Select(skill => skill.Name));
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateEmailIgnoringCase()
    {
        var service = new CandidateService(DbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCandidateRequest
            {
                FullName = "Ana Duplicate",
                DateOfBirth = new DateOnly(1995, 1, 1),
                ContactNumber = "+38164000000",
                Email = " ANA.MARKOVIC@example.com ",
                SkillIds = []
            }));

        Assert.Equal("Candidate with the same email already exists.", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_FiltersByNameAndSkill()
    {
        var service = new CandidateService(DbContext);

        var candidates = await service.SearchAsync("ana", ["english"]);

        var candidate = Assert.Single(candidates);
        Assert.Equal("Ana Markovic", candidate.FullName);
        Assert.Contains(candidate.Skills, skill => skill.Name == "English");
    }

    [Fact]
    public async Task AddSkillAsync_DoesNotDuplicateExistingCandidateSkill()
    {
        var service = new CandidateService(DbContext);

        var candidate = await service.AddSkillAsync(candidateId: 1, skillId: 4);

        Assert.NotNull(candidate);
        Assert.Single(candidate.Skills, skill => skill.Name == "English");
    }

    [Fact]
    public async Task UpdateAsync_ReplacesCandidateSkills()
    {
        var service = new CandidateService(DbContext);

        var candidate = await service.UpdateAsync(1, new UpdateCandidateRequest
        {
            FullName = "Ana Markovic Updated",
            DateOfBirth = new DateOnly(1996, 4, 18),
            ContactNumber = "+38164111222",
            Email = "ana.updated@example.com",
            SkillIds = [2, 5]
        });

        Assert.NotNull(candidate);
        Assert.Equal("Ana Markovic Updated", candidate.FullName);
        Assert.Equal(["German Language", "Java Programming"], candidate.Skills.Select(skill => skill.Name));
    }
}
