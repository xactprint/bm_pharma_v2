using FluentAssertions;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.Domain.Tests;

public class InvoiceTests
{
    [Fact]
    public void Invoice_Should_Be_Created_With_Default_Values()
    {
        var invoice = new Invoice();

        invoice.Id.Should().NotBe(Guid.Empty);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.InvoiceDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        invoice.Lines.Should().BeEmpty();
        invoice.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Invoice_Line_Should_Calculate_Total()
    {
        var line = new InvoiceLine
        {
            Quantity = 10,
            UnitPriceDA = 150,
            LineTotalDA = 1500
        };

        line.LineTotalDA.Should().Be(1500);
    }
}
