using Zadatak.Dtos;
using Zadatak.Services;

namespace Zadatak.Tests.Services;

public class SkillServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateAsync_AddsNewSkill()
    {
        var service = new SkillService(DbContext);

        var skill = await service.CreateAsync(new CreateSkillRequest
        {
            Name = "React"
        });

        Assert.True(skill.Id > 0);
        Assert.Equal("React", skill.Name);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateNameIgnoringCaseAndWhitespace()
    {
        var service = new SkillService(DbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateSkillRequest
            {
                Name = " english "
            }));

        Assert.Equal("Skill with the same name already exists.", exception.Message);
    }
}
