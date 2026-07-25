using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaInvoiceServiceTests
{
    [Fact]
    public async Task CH023_Stub_Invoice_Creation_Returns_False()
    {
        var logger = new Mock<ILogger<ChifaInvoiceServiceStub>>();
        var service = new ChifaInvoiceServiceStub(logger.Object);

        var result = await service.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 1 }
            }
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task CH024_Stub_Invoice_Exists_Returns_False()
    {
        var logger = new Mock<ILogger<ChifaInvoiceServiceStub>>();
        var service = new ChifaInvoiceServiceStub(logger.Object);

        var exists = await service.InvoiceExistsInChifaAsync("00000001");

        exists.Should().BeFalse();
    }
}
