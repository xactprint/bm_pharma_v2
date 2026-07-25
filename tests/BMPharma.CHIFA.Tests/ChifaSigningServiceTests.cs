using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaSigningServiceTests
{
    [Fact]
    public async Task CH021_Signing_Status_Returns_NotSigned()
    {
        var logger = new Mock<ILogger<ChifaSigningServiceStub>>();
        var service = new ChifaSigningServiceStub(logger.Object);

        var status = await service.GetSigningStatusAsync("000216");

        status.Should().Be(ChifaSigningStatus.NotSigned);
    }

    [Fact]
    public async Task CH022_Token_Available_Returns_False()
    {
        var logger = new Mock<ILogger<ChifaSigningServiceStub>>();
        var service = new ChifaSigningServiceStub(logger.Object);

        var available = await service.IsTokenAvailableAsync();

        available.Should().BeFalse();
    }
}
