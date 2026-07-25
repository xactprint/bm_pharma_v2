using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaBordereauServiceTests
{
    [Fact]
    public async Task CH025_Stub_Bordereau_Creation_Returns_False()
    {
        var logger = new Mock<ILogger<ChifaBordereauServiceStub>>();
        var service = new ChifaBordereauServiceStub(logger.Object);

        var result = await service.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            TypeBord = "BORD_CNAS",
            InvoiceNumbers = new List<string> { "00000001" }
        });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CH026_Stub_GetNextBordereauNumber_Returns_000001()
    {
        var logger = new Mock<ILogger<ChifaBordereauServiceStub>>();
        var service = new ChifaBordereauServiceStub(logger.Object);

        var num = await service.GetNextBordereauNumberAsync();

        num.Should().Be("000001");
    }

    [Fact]
    public async Task CH027_Sign_Bordereau_Delegates_To_CHIFA()
    {
        var logger = new Mock<ILogger<ChifaBordereauServiceStub>>();
        var service = new ChifaBordereauServiceStub(logger.Object);

        var result = await service.SignBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task CH028_Close_Bordereau_Delegates_To_CHIFA()
    {
        var logger = new Mock<ILogger<ChifaBordereauServiceStub>>();
        var service = new ChifaBordereauServiceStub(logger.Object);

        var result = await service.CloseBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not yet implemented");
    }
}
