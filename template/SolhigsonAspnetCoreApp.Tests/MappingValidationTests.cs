using Mapster;
using SolhigsonAspnetCoreApp.Domain.Dto;
using SolhigsonAspnetCoreApp.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace SolhigsonAspnetCoreApp.Tests;

public class MappingValidationTests : TestBase
{
    public MappingValidationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void Repository_Entity_Model_Id_Not_Mapped()
    {
        var institution = new Institution();
        var existingId = institution.Id;
        var institutionDto = new InstitutionDto
        {
            Id = "someid",
            Name = "Test Institution"
        };

        institutionDto.Adapt(institution);
        var dynInstitution = institutionDto.Adapt<Institution>();

        Assert.Equal(institutionDto.Name, institution.Name);
        Assert.Equal(dynInstitution.Name, institution.Name);
        Assert.Equal(existingId, institution.Id);
        Assert.NotEqual(dynInstitution.Id, institutionDto.Id);
    }
}