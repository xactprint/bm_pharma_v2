using System.Reflection;
using FluentAssertions;
using Xunit;

namespace BMPharma.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(BMPharma.Domain.Common.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(BMPharma.Application.Interfaces.ICurrentUserService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(BMPharma.Infrastructure.InfrastructureMarker).Assembly;
    private static readonly Assembly SharedAssembly = typeof(BMPharma.Shared.Constants.AppConstants).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var domainReferences = DomainAssembly.GetReferencedAssemblies();
        domainReferences.Should().NotContain(a => a.Name == "BMPharma.Infrastructure",
            "Domain layer must not depend on Infrastructure");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var domainReferences = DomainAssembly.GetReferencedAssemblies();
        domainReferences.Should().NotContain(a => a.Name == "BMPharma.Application",
            "Domain layer must not depend on Application");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var appReferences = ApplicationAssembly.GetReferencedAssemblies();
        appReferences.Should().NotContain(a => a.Name == "BMPharma.Infrastructure",
            "Application layer must not depend on Infrastructure");
    }

    [Fact]
    public void Shared_Should_Not_Depend_On_Domain()
    {
        var sharedReferences = SharedAssembly.GetReferencedAssemblies();
        sharedReferences.Should().NotContain(a => a.Name == "BMPharma.Domain",
            "Shared layer should not depend on Domain");
    }

    [Fact]
    public void All_Entity_Classes_Should_Have_Parameterless_Constructor()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains("Entities") == true && !t.IsAbstract && !t.IsInterface);

        foreach (var entityType in entityTypes)
        {
            var constructor = entityType.GetConstructor(Type.EmptyTypes);
            constructor.Should().NotBeNull($"{entityType.Name} should have a parameterless constructor");
        }
    }

    [Fact]
    public void All_Entity_Classes_Should_Inherit_From_BaseEntity()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains("Entities") == true && !t.IsAbstract && !t.IsInterface && !t.Name.Contains("ValueObject"));

        foreach (var entityType in entityTypes)
        {
            typeof(BMPharma.Domain.Common.BaseEntity).IsAssignableFrom(entityType).Should().BeTrue(
                $"{entityType.Name} should inherit from BaseEntity");
        }
    }

    [Fact]
    public void Domain_Enums_Should_Be_In_Domain_Layer()
    {
        var enumTypes = DomainAssembly.GetTypes()
            .Where(t => t.IsEnum && t.Namespace?.Contains("Enums") == true);

        enumTypes.Should().NotBeEmpty("Domain should contain enum types");
    }
}
