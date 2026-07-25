using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaNegativeScenarioTests
{
    private readonly ChifaInvoiceValidator _validator;
    private readonly ChifaBordereauValidator _bordereauValidator;
    private readonly ChifaWorkflowStateMachine _stateMachine;

    public ChifaNegativeScenarioTests()
    {
        var logger = new Mock<ILogger<ChifaInvoiceValidator>>();
        _validator = new ChifaInvoiceValidator(logger.Object);
        _bordereauValidator = new ChifaBordereauValidator();
        _stateMachine = new ChifaWorkflowStateMachine();
    }

    [Fact]
    public void NEG001_NumFact_TooLong_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "123456789",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_FACT_TOO_LONG");
    }

    [Fact]
    public void NEG002_NumFact_Empty_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_FACT_EMPTY");
    }

    [Fact]
    public void NEG003_NumAssure_Empty_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ASSURE_EMPTY");
    }

    [Fact]
    public void NEG004_NumAssure_TooLong_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "1234567890123",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ASSURE_TOO_LONG");
    }

    [Fact]
    public void NEG005_NoLines_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NO_LINES");
    }

    [Fact]
    public void NEG006_Quantity_Zero_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 0 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_INVALID");
    }

    [Fact]
    public void NEG007_Quantity_TooHigh_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 100, Quantite = 1000 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "QTE_TOO_HIGH");
    }

    [Fact]
    public void NEG008_PrixUnit_Zero_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 0, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "PPA_INVALID");
    }

    [Fact]
    public void NEG009_NumBord_Empty_Rejected()
    {
        var result = _bordereauValidator.Validate("", "11600", new() { "00000001" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_BORD_EMPTY");
    }

    [Fact]
    public void NEG010_NumBord_TooLong_Rejected()
    {
        var result = _bordereauValidator.Validate("1234567", "11600", new() { "00000001" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_BORD_TOO_LONG");
    }

    [Fact]
    public void NEG011_CodeCentre_Empty_Rejected()
    {
        var result = _bordereauValidator.Validate("000216", "", new() { "00000001" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "CODE_CENTRE_EMPTY");
    }

    [Fact]
    public void NEG012_NoInvoices_Rejected()
    {
        var result = _bordereauValidator.Validate("000216", "11600", new());
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NO_INVOICES");
    }

    [Fact]
    public void NEG013_WriteGuard_ReadOnlyBlocksWrite()
    {
        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.ReadOnly));
        var act = () => guard.EnsureWriteAllowedAsync();
        act.Should().ThrowAsync<ChifaWriteBlockedException>();
    }

    [Fact]
    public void NEG014_StateMachine_InvalidTransition_Throws()
    {
        var act = () => _stateMachine.Transition(ChifaWorkflowState.Draft, ChifaWorkflowState.Transmitted);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task NEG015_FakeInvoice_WhenOffline_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);
        fake.SimulateOffline();

        var result = await fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("offline");
    }

    [Fact]
    public async Task NEG016_FakeBordereau_MissingInvoice_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);

        var result = await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new() { "NONEXISTENT" }
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task NEG017_FakeSignBordereau_WithoutToken_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);
        await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new()
        });

        var result = await fake.SignBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("token");
    }

    [Fact]
    public async Task NEG018_FakeCloseBordereau_BeforeSigning_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);
        fake.SimulateTokenPresent();
        await fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new()
        });

        var result = await fake.CloseBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("signed");
    }

    [Fact]
    public async Task NEG019_FakeDuplicateInvoice_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600
        };

        await fake.CreateInvoiceAsync(request);
        var result = await fake.CreateInvoiceAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task NEG020_FakeDuplicateBordereau_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);
        var request = new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new()
        };

        await fake.CreateBordereauAsync(request);
        var result = await fake.CreateBordereauAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public void NEG021_NumEnr_TooLong_Rejected()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new() { new() { NumEnr = "123456", MedicCode = 1, PrixUnit = 100, Quantite = 1 } }
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_ENR_TOO_LONG");
    }

    [Fact]
    public void NEG022_MultipleErrors_Returned()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "",
            NumAssure = "",
            CodeCentre = 11600,
            Lines = new()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void NEG023_InvoiceStatus_Draft_CannotBeTransmitted()
    {
        _stateMachine.CanTransition(ChifaWorkflowState.Draft, ChifaWorkflowState.Transmitted).Should().BeFalse();
    }

    [Fact]
    public void NEG024_Transmitted_HasNoTransitions()
    {
        var transitions = _stateMachine.GetAllowedTransitions(ChifaWorkflowState.Transmitted);
        transitions.Should().BeEmpty();
    }

    [Fact]
    public async Task NEG025_FakeSignBordereau_NotFound_Fails()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        var fake = new FakeChifaIntegrationProvider(logger.Object);

        var result = await fake.SignBordereauAsync("NONEXISTENT");

        result.Success.Should().BeFalse();
    }
}
