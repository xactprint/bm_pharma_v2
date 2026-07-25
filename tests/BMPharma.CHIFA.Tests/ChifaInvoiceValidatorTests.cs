using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaInvoiceValidatorTests
{
    private readonly ChifaInvoiceValidator _validator;

    public ChifaInvoiceValidatorTests()
    {
        var logger = new Mock<ILogger<ChifaInvoiceValidator>>();
        _validator = new ChifaInvoiceValidator(logger.Object);
    }

    [Fact]
    public void CH001_Valid_Invoice_Passes_Validation()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            DateSoin = DateTime.Today,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 2 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CH002_Invoice_NumFact_Too_Long_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "123456789",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_FACT_TOO_LONG");
    }

    [Fact]
    public void CH003_NumEnr_Too_Long_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "123456", MedicCode = 1, PrixUnit = 150, Quantite = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ENR_TOO_LONG");
    }

    [Fact]
    public void CH004_Empty_NumFact_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_FACT_EMPTY");
    }

    [Fact]
    public void CH005_Empty_NumAssure_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ASSURE_EMPTY");
    }

    [Fact]
    public void CH006_No_Lines_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NO_LINES");
    }

    [Fact]
    public void CH007_Quantity_Zero_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 0 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_INVALID");
    }

    [Fact]
    public void CH008_Quantity_Too_High_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 1000 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_TOO_HIGH");
    }

    [Fact]
    public void CH009_PPA_Zero_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 0, Quantite = 1 }
            }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "PPA_INVALID");
    }

    [Fact]
    public void CH010_ApplyDefaults_Sets_DateSoin()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890",
            CodeCentre = 11600,
            DateSoin = default
        };

        _validator.ApplyDefaults(request);

        request.DateSoin.Should().Be(DateTime.Today);
    }

    [Fact]
    public void CH011_ApplyLineDefaults_Sets_Standard_Values()
    {
        var line = new ChifaInvoiceLineRequest
        {
            NumEnr = "00001",
            MedicCode = 1,
            PrixUnit = 150,
            Quantite = 1,
            InfTr = 0,
            ApplicTr = 0,
            Medic = 0,
            Ts = 0,
            DureeTrait = 0
        };

        _validator.ApplyLineDefaults(line);

        line.InfTr.Should().Be(1);
        line.ApplicTr.Should().Be(1);
        line.Medic.Should().Be(1);
        line.Ts.Should().Be(4);
        line.DureeTrait.Should().Be(5);
    }
}
