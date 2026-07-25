using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaInvoiceMapperTests
{
    private readonly ChifaInvoiceMapper _mapper = new();

    [Fact]
    public void MAP001_MapNumFact_PadsZeros()
    {
        var result = _mapper.MapNumFact("123");
        result.Should().Be("00000123");
    }

    [Fact]
    public void MAP002_MapNumFact_TruncatesTo8()
    {
        var result = _mapper.MapNumFact("123456789012");
        result.Should().Be("56789012");
    }

    [Fact]
    public void MAP003_MapNumFact_RemovesDashes()
    {
        var result = _mapper.MapNumFact("12-34-56");
        result.Should().Be("00123456");
    }

    [Fact]
    public void MAP004_MapNumFact_EmptyReturnsEmpty()
    {
        var result = _mapper.MapNumFact("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void MAP005_MapNumFact_WhitespaceReturnsEmpty()
    {
        var result = _mapper.MapNumFact("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public void MAP006_MapNumAssure_TruncatesIfTooLong()
    {
        var customer = new Customer { InsuranceNumber = "123456789012345678" };
        var result = _mapper.MapNumAssure(customer);
        result.Should().HaveLength(12);
    }

    [Fact]
    public void MAP007_MapNumAssure_EmptyIfNull()
    {
        var customer = new Customer { InsuranceNumber = null };
        var result = _mapper.MapNumAssure(customer);
        result.Should().BeEmpty();
    }

    [Fact]
    public void MAP008_MapMedicCode_FromCIPCode()
    {
        var result = _mapper.MapMedicCode("340096012");
        result.Should().Be(340096012);
    }

    [Fact]
    public void MAP009_MapMedicCode_DefaultsTo1_IfNull()
    {
        var result = _mapper.MapMedicCode(null);
        result.Should().Be(1);
    }

    [Fact]
    public void MAP010_MapMedicCode_DefaultsTo1_IfNonNumeric()
    {
        var result = _mapper.MapMedicCode("ABC123");
        result.Should().Be(1);
    }

    [Fact]
    public void MAP011_MapNumEnr_PadsZeros()
    {
        var result = _mapper.MapNumEnr("42");
        result.Should().Be("00042");
    }

    [Fact]
    public void MAP012_MapNumEnr_TruncatesTo5()
    {
        var result = _mapper.MapNumEnr("12345678");
        result.Should().Be("45678");
    }

    [Fact]
    public void MAP013_MapNumEnr_DefaultsTo00001_IfNull()
    {
        var result = _mapper.MapNumEnr(null);
        result.Should().Be("00001");
    }

    [Fact]
    public void MAP014_MapLine_ProducesValidRequest()
    {
        var product = new Product { Code = "42", CIPCode = "123456789", PriceDA = 250 };
        var line = new InvoiceLine
        {
            Quantity = 3,
            UnitPriceDA = 250,
            LineTotalDA = 750,
            Product = product
        };

        var result = _mapper.MapLine(line);

        result.NumEnr.Should().Be("00042");
        result.MedicCode.Should().Be(123456789);
        result.PrixUnit.Should().Be(250);
        result.Quantite.Should().Be(3);
        result.InfTr.Should().Be(1);
        result.ApplicTr.Should().Be(1);
        result.Medic.Should().Be(1);
        result.Ts.Should().Be(4);
        result.DureeTrait.Should().Be(5);
    }

    [Fact]
    public void MAP015_MapToChifaRequest_ProducesValidRequest()
    {
        var customer = new Customer { InsuranceNumber = "123456789012" };
        var product = new Product { Code = "42", CIPCode = "123456789", PriceDA = 250 };
        var invoice = new Invoice
        {
            InvoiceNumber = "00000123",
            InvoiceDate = new DateTime(2025, 6, 15),
            Customer = customer,
            Lines = new List<InvoiceLine>
            {
                new() { Quantity = 2, UnitPriceDA = 250, Product = product }
            }
        };

        var result = _mapper.MapToChifaRequest(invoice, customer);

        result.NumFact.Should().Be("00000123");
        result.NumAssure.Should().Be("123456789012");
        result.CodeCentre.Should().Be(11600);
        result.DateSoin.Should().Be(new DateTime(2025, 6, 15));
        result.Lines.Should().HaveCount(1);
    }

    [Fact]
    public void MAP016_MapToChifaRequest_ThrowsIfInvoiceNull()
    {
        var act = () => _mapper.MapToChifaRequest(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MAP017_CalculateMontFact_SumsLines()
    {
        var invoice = new Invoice
        {
            Lines = new List<InvoiceLine>
            {
                new() { Quantity = 2, UnitPriceDA = 150 },
                new() { Quantity = 1, UnitPriceDA = 300 }
            }
        };

        _mapper.CalculateMontFact(invoice).Should().Be(600);
    }

    [Fact]
    public void MAP018_CalculateMontAs_AppliesReimbursementRate()
    {
        var invoice = new Invoice
        {
            Lines = new List<InvoiceLine>
            {
                new() { Quantity = 1, UnitPriceDA = 1000 }
            }
        };

        _mapper.CalculateMontAs(invoice, 0.70m).Should().Be(700);
    }

    [Fact]
    public void MAP019_CalculateMontMut_IsRemainder()
    {
        var invoice = new Invoice
        {
            Lines = new List<InvoiceLine>
            {
                new() { Quantity = 1, UnitPriceDA = 1000 }
            }
        };

        _mapper.CalculateMontMut(invoice, 0.70m).Should().Be(300);
    }

    [Fact]
    public void MAP020_ApplyChifaFieldsToInvoice_SetsAllFields()
    {
        var invoice = new Invoice
        {
            InvoiceNumber = "00000123",
            InvoiceDate = DateTime.Today,
            Lines = new List<InvoiceLine>
            {
                new() { Quantity = 2, UnitPriceDA = 150 }
            }
        };
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000123",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            DateSoin = DateTime.Today
        };
        var chifaResult = new ChifaInvoiceResult { Success = true, ChifaNumFact = "00000123" };

        _mapper.ApplyChifaFieldsToInvoice(invoice, request, chifaResult);

        invoice.ChifaNumFact.Should().Be("00000123");
        invoice.ChifaNumAssure.Should().Be("123456789012");
        invoice.ChifaCodeCentre.Should().Be(11600);
        invoice.ChifaMontFact.Should().Be(300);
        invoice.ChifaMontAs.Should().Be(210);
        invoice.ChifaMontMut.Should().Be(90);
        invoice.ChifaDateFinMut.Should().Be(DateTime.Today.AddYears(1));
        invoice.ChifaLastUpdated.Should().NotBeNull();
    }
}
