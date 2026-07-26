using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaInvoiceWorkflowServiceTests
{
    private static ServiceCollection CreateServicesWithLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    private static (IChifaInvoiceWorkflowService service, FakeChifaIntegrationProvider fake, ChifaIntegrationModeProvider modeProvider) CreateReadOnlyWorkflowService()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.ReadOnly);
        var provider = services.BuildServiceProvider();
        return (
            provider.GetRequiredService<IChifaInvoiceWorkflowService>(),
            provider.GetRequiredService<FakeChifaIntegrationProvider>(),
            provider.GetRequiredService<ChifaIntegrationModeProvider>());
    }

    private static ChifaInvoiceRequest CreateValidRequest()
    {
        return new ChifaInvoiceRequest
        {
            NumFact = "000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            DateSoin = DateTime.Today,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, Quantite = 2, PrixUnit = 15.50m },
                new() { NumEnr = "00002", MedicCode = 2, Quantite = 1, PrixUnit = 25.00m }
            }
        };
    }

    // --- WF001-WF003: Service registration and basic instantiation ---

    [Fact]
    public void WF001_WorkflowService_IsRegistered()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.ReadOnly);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IChifaInvoiceWorkflowService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<ChifaInvoiceWorkflowService>();
    }

    [Fact]
    public async Task WF002_ExecuteFullWorkflow_ReadOnlyMode_SimulatesSuccessfully()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ExecuteFullWorkflowAsync(request, "test_user");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.SimulationMessage.Should().Contain("MODE LECTURE SEULE");
        result.Mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    [Fact]
    public async Task WF003_ExecuteFullWorkflow_ReadOnlyMode_NoRealWrite()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        await service.ExecuteFullWorkflowAsync(request);
        fake.InvoicesCreated.Should().Be(0, "ReadOnly mode should not write to CHIFA");
    }

    // --- WF004-WF008: ValidateOnlyAsync ---

    [Fact]
    public async Task WF004_ValidateOnly_ValidRequest_Passes()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("VALIDATION_PASSED");
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task WF005_ValidateOnly_EmptyNumFact_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.NumFact = string.Empty;
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
        result.ValidationErrors.Should().Contain(e => e.Field == "num_fact");
    }

    [Fact]
    public async Task WF006_ValidateOnly_NullNumFact_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.NumFact = null!;
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WF007_ValidateOnly_InvalidNumFactLength_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.NumFact = "123456789"; // 9 chars > max 8
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Field == "num_fact");
    }

    [Fact]
    public async Task WF008_ValidateOnly_ComputesAmounts()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeTrue();
        var expectedMontFact = (2 * 15.50m) + (1 * 25.00m); // 56.00
        result.ComputedMontFact.Should().Be(expectedMontFact);
        result.ComputedMontAs.Should().Be(Math.Round(expectedMontFact * 0.70m, 2));
        result.ComputedMontMut.Should().Be(Math.Round(expectedMontFact - result.ComputedMontAs, 2));
    }

    // --- WF009-WF012: Validation edge cases ---

    [Fact]
    public async Task WF009_ValidateOnly_ZeroQuantite_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.Lines[0].Quantite = 0;
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Code == "QTE_INVALID");
    }

    [Fact]
    public async Task WF010_ValidateOnly_NegativePrixUnit_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.Lines[0].PrixUnit = -5.00m;
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Code == "PPA_INVALID");
    }

    [Fact]
    public async Task WF011_ValidateOnly_EmptyLines_NoLineErrors()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.Lines.Clear();
        var result = await service.ValidateOnlyAsync(request);
        // Empty lines should still have validation errors for missing required fields
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WF012_ValidateOnly_ZeroTotalAmount_Fails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.Lines.Clear();
        request.Lines.Add(new ChifaInvoiceLineRequest { NumEnr = "00001", MedicCode = 1, Quantite = 0, PrixUnit = 0 });
        var result = await service.ValidateOnlyAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Code == "QTE_INVALID" || e.Code == "PPA_INVALID");
    }

    // --- WF013-WF016: PrepareInvoiceAsync ---

    [Fact]
    public async Task WF013_PrepareInvoice_ValidRequest_PreparesSuccessfully()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.PrepareInvoiceAsync(request, "test_user");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("PREPARED");
        result.State.Should().Be(ChifaWorkflowState.PreparedForChifa);
    }

    [Fact]
    public async Task WF014_PrepareInvoice_ComputesAmountsCorrectly()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.PrepareInvoiceAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.ComputedMontFact.Should().Be(56.00m);
        result.ComputedMontAs.Should().Be(39.20m); // 70%
        result.ComputedMontMut.Should().Be(16.80m); // 30%
    }

    [Fact]
    public async Task WF015_PrepareInvoice_SetsCorrelationId()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.PrepareInvoiceAsync(request);
        result.CorrelationId.Should().NotBeNullOrEmpty();
        result.CorrelationId.Length.Should().Be(12);
    }

    [Fact]
    public async Task WF016_PrepareInvoice_SetsTimestamp()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var before = DateTime.UtcNow;
        var result = await service.PrepareInvoiceAsync(request);
        var after = DateTime.UtcNow;
        result.Timestamp.Should().BeOnOrAfter(before);
        result.Timestamp.Should().BeOnOrBefore(after);
    }

    // --- WF017-WF020: CreateInDatabaseAsync ---

    [Fact]
    public async Task WF017_CreateInDatabase_NewInvoice_CreatesSuccessfully()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var result = await service.CreateInDatabaseAsync("999999", "test_user");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("CREATED_IN_DATABASE");
        result.State.Should().Be(ChifaWorkflowState.WrittenToChifa);
    }

    [Fact]
    public async Task WF018_CreateInDatabase_ExistingInvoice_Fails()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateInvoiceExists("123456");
        var result = await service.CreateInDatabaseAsync("123456");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("ALREADY_EXISTS");
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task WF019_CreateInDatabase_SetsDurationMs()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var result = await service.CreateInDatabaseAsync("555555");
        result.DurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task WF020_CreateInDatabase_WithUserId_PassesUserId()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var result = await service.CreateInDatabaseAsync("777777", "pharmacist1");
        result.IsSuccess.Should().BeTrue();
    }

    // --- WF021-WF023: CheckVisibilityAsync ---

    [Fact]
    public async Task WF021_CheckVisibility_Invisible_ReturnsNotVisible()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var result = await service.CheckVisibilityAsync("000001");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("NOT_VISIBLE");
        result.State.Should().Be(ChifaWorkflowState.WrittenToChifa);
    }

    [Fact]
    public async Task WF022_CheckVisibility_Visible_ReturnsVisible()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateInvoiceVisible("000001");
        var result = await service.CheckVisibilityAsync("000001");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("VISIBLE");
        result.State.Should().Be(ChifaWorkflowState.VisibleInChifa);
    }

    [Fact]
    public async Task WF023_CheckVisibility_NotVisible_HasErrorMessage()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var result = await service.CheckVisibilityAsync("000001");
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("replication delay");
    }

    // --- WF024-WF026: SignBordereauAsync ---

    [Fact]
    public async Task WF024_SignBordereau_TokenNotPresent_FailsWithAction()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateTokenAbsent();
        var result = await service.SignBordereauAsync("BORD001", "test_user");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("TOKEN_NOT_PRESENT");
        result.RequiresAction.Should().BeTrue();
        result.ActionDescription.Should().Contain("token");
        result.ActionApplication.Should().Be("CHIFA-OFFICINE");
    }

    [Fact]
    public async Task WF025_SignBordereau_TokenPresent_SignsSuccessfully()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateTokenPresent();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "000001", Lines = new List<ChifaInvoiceLineRequest>() });
        await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "BORD01", InvoiceNumbers = new List<string> { "000001" } });
        var result = await service.SignBordereauAsync("BORD01", "test_user");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("SIGNED");
        result.State.Should().Be(ChifaWorkflowState.Signed);
        result.SignatureId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WF026_SignBordereau_TokenPresent_SigningFails()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateTokenPresent();
        fake.SimulateSigningRequired();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "000001", Lines = new List<ChifaInvoiceLineRequest>() });
        await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "BORDF1", InvoiceNumbers = new List<string> { "000001" } });
        var result = await service.SignBordereauAsync("BORDF1", "test_user");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("SIGN_FAILED");
    }

    // --- WF027-WF028: CloseBordereauAsync ---

    [Fact]
    public async Task WF027_CloseBordereau_Success()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateTokenPresent();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "000001", Lines = new List<ChifaInvoiceLineRequest>() });
        await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "BORD02", InvoiceNumbers = new List<string> { "000001" } });
        await fake.SignBordereauAsync("BORD02");
        var result = await service.CloseBordereauAsync("BORD02");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("CLOSED");
        result.State.Should().Be(ChifaWorkflowState.BordereauClosed);
    }

    [Fact]
    public async Task WF028_CloseBordereau_Failure_SetsAction()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        fake.SimulateCloseFails();
        await fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "000001", Lines = new List<ChifaInvoiceLineRequest>() });
        await fake.CreateBordereauAsync(new ChifaBordereauRequest { NumBord = "BORDF2", InvoiceNumbers = new List<string> { "000001" } });
        fake.SimulateTokenPresent();
        await fake.SignBordereauAsync("BORDF2");
        var result = await service.CloseBordereauAsync("BORDF2");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("CLOSE_FAILED");
        result.RequiresAction.Should().BeTrue();
    }

    // --- WF029-WF030: AssignBordereauAsync ---

    [Fact]
    public async Task WF029_AssignBordereau_Succeeds()
    {
        var (service, fake, _) = CreateReadOnlyWorkflowService();
        var result = await service.AssignBordereauAsync("000001", "test_user");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("BORDEREAU_ASSIGNED");
        result.State.Should().Be(ChifaWorkflowState.BordereauAssigned);
        result.NumBord.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WF030_AssignBordereau_SetsRequiresAction()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var result = await service.AssignBordereauAsync("000001");
        result.RequiresAction.Should().BeTrue();
        result.ActionDescription.Should().Contain("Bordereau");
    }

    // --- WF031-WF033: Audit log ---

    [Fact]
    public async Task WF031_ExecuteFullWorkflow_CreatesAuditEntries()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        await service.ExecuteFullWorkflowAsync(request);
        var auditLog = service.GetAuditLog();
        auditLog.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WF032_AuditLog_CorrelationIdMatches()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ExecuteFullWorkflowAsync(request);
        var auditLog = service.GetAuditLog();
        auditLog.Should().Contain(e => e.CorrelationId == result.CorrelationId);
    }

    [Fact]
    public async Task WF033_ClearAuditLog_EmptiesLog()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        await service.ExecuteFullWorkflowAsync(request);
        service.GetAuditLog().Should().NotBeEmpty();
        service.ClearAuditLog();
        service.GetAuditLog().Should().BeEmpty();
    }

    // --- WF034-WF035: Full workflow edge cases ---

    [Fact]
    public async Task WF034_ExecuteFullWorkflow_InvalidRequest_ValidationFails()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        request.NumFact = string.Empty;
        var result = await service.ExecuteFullWorkflowAsync(request);
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("VALIDATION_FAILED");
    }

    [Fact]
    public async Task WF035_ExecuteFullWorkflow_ReadOnlyMode_ReturnsReadOnlyState()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ExecuteFullWorkflowAsync(request);
        result.State.Should().Be(ChifaWorkflowState.PreparedForChifa);
        result.Mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    // --- WF036-WF037: Reimbursement rate accuracy ---

    [Fact]
    public async Task WF036_ReimbursementRate_70Percent()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ValidateOnlyAsync(request);
        var expectedAs = Math.Round(result.ComputedMontFact * 0.70m, 2);
        result.ComputedMontAs.Should().Be(expectedAs);
    }

    [Fact]
    public async Task WF037_PatientShare_30Percent()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = CreateValidRequest();
        var result = await service.ValidateOnlyAsync(request);
        var expectedMut = result.ComputedMontFact - result.ComputedMontAs;
        result.ComputedMontMut.Should().Be(expectedMut);
    }

    // --- WF038-WF040: Multiple operations ---

    [Fact]
    public async Task WF038_MultipleWorkflows_AuditEntriesAccumulate()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request1 = CreateValidRequest();
        var request2 = CreateValidRequest();
        request2.NumFact = "000002";
        await service.ExecuteFullWorkflowAsync(request1);
        await service.ExecuteFullWorkflowAsync(request2);
        var auditLog = service.GetAuditLog();
        auditLog.Count.Should().BeGreaterThanOrEqualTo(4); // At least 2 per workflow
    }

    [Fact]
    public async Task WF039_MultipleWorkflows_CorrelationIdsAreUnique()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request1 = CreateValidRequest();
        var request2 = CreateValidRequest();
        request2.NumFact = "000002";
        var result1 = await service.ExecuteFullWorkflowAsync(request1);
        var result2 = await service.ExecuteFullWorkflowAsync(request2);
        result1.CorrelationId.Should().NotBe(result2.CorrelationId);
    }

    [Fact]
    public async Task WF040_PrepareInvoice_SingleLine_ComputesCorrectly()
    {
        var (service, _, _) = CreateReadOnlyWorkflowService();
        var request = new ChifaInvoiceRequest
        {
            NumFact = "000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            DateSoin = DateTime.Today,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, Quantite = 1, PrixUnit = 100.00m }
            }
        };
        var result = await service.PrepareInvoiceAsync(request);
        result.ComputedMontFact.Should().Be(100.00m);
        result.ComputedMontAs.Should().Be(70.00m);
        result.ComputedMontMut.Should().Be(30.00m);
    }
}
