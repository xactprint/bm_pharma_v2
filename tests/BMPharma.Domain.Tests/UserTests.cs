using FluentAssertions;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.Domain.Tests;

public class UserTests
{
    [Fact]
    public void User_Should_Be_Created_With_Default_Values()
    {
        var user = new User();

        user.Id.Should().NotBe(Guid.Empty);
        user.IsActive.Should().BeTrue();
        user.IsLocked.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void User_Should_Have_Role()
    {
        var user = new User
        {
            Email = "admin@bm-stock.dz",
            DisplayName = "Administrator",
            Role = RoleType.Admin
        };

        user.Role.Should().Be(RoleType.Admin);
    }
}
