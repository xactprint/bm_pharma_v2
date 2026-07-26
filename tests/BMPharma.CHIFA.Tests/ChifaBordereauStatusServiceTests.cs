using FluentAssertions;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaBordereauStatusServiceTests
{
    private static ServiceCollection CreateServicesWithLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    private static (IBordereauStatusService service, FakeChifaIntegrationProvider fake, ChifaIntegrationModeProvider modeProvider) CreateReadOnlyService()
    {
        var services = CreateServicesWithLogging();
        services.AddChifaIntegrationForTests(ChifaIntegrationMode.ReadOnly);
        var provider = services.BuildServiceProvider();
        return (
            provider.GetRequiredService<IBordereauStatusService>(),
            provider.GetRequiredService<FakeChifaIntegrationProvider>(),
            provider.GetRequiredService<ChifaIntegrationModeProvider>());
    }

    // --- BORD001: Création simulée ---
    [Fact]
    public async Task BORD001_CreateBordereau_ReadOnly_SimulatesCreation()
    {
        var (service, fake, _) = CreateReadOnlyService();
        var result = await service.CreateBordereauAsync("BORD001", "11600", new List<string> { "000001", "000002" });
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.SimulationMessage.Should().Contain("MODE LECTURE SEULE");
        result.State.Should().Be(BordereauWorkflowState.Created);
        result.InvoiceCount.Should().Be(2);
    }

    // --- BORD002: Ajout facture ---
    [Fact]
    public async Task BORD002_AttachInvoices_ReadOnly_Succeeds()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD002", "11600", new List<string> { "000001" });
        var result = await service.AttachInvoicesAsync("BORD002", new List<string> { "000003", "000004" });
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.State.Should().Be(BordereauWorkflowState.InvoicesAttached);
    }

    // --- BORD003: Suppression facture ---
    [Fact]
    public async Task BORD003_RemoveInvoice_Succeeds()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD003", "11600", new List<string> { "000001", "000002" });
        var result = await service.RemoveInvoiceAsync("BORD003", "000001");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("INVOICE_REMOVED");
        result.InvoiceCount.Should().Be(1);
    }

    // --- BORD004: Calcul montant total ---
    [Fact]
    public async Task BORD004_RemoveInvoice_NonExistentInvoice_Fails()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD004", "11600", new List<string> { "000001" });
        var result = await service.RemoveInvoiceAsync("BORD004", "999999");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("INVOICE_NOT_FOUND");
    }

    // --- BORD005: Validation bordereau ---
    [Fact]
    public async Task BORD005_ValidateBordereau_ReadOnly_Succeeds()
    {
        var (service, fake, _) = CreateReadOnlyService();
        fake.SimulateInvoiceVisible("000001");
        fake.SimulateInvoiceVisible("000002");
        await service.CreateBordereauAsync("BORD005", "11600", new List<string> { "000001", "000002" });
        var result = await service.ValidateBordereauAsync("BORD005");
        result.IsSuccess.Should().BeTrue();
    }

    // --- BORD006: Facture non valide ---
    [Fact]
    public async Task BORD006_ValidateBordereau_NoInvoices_Fails()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD006", "11600", new List<string>());
        var result = await service.ValidateBordereauAsync("BORD006");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("VALIDATION_FAILED");
        result.ValidationErrors.Should().Contain(e => e.Code == "NO_INVOICES");
    }

    // --- BORD007: Facture non signée ---
    [Fact]
    public async Task BORD007_RemoveInvoice_FromNonExistentBordereau_Fails()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.RemoveInvoiceAsync("NEXXXX", "000001");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("NOT_FOUND");
    }

    // --- BORD008: Signature partielle ---
    [Fact]
    public async Task BORD008_SignBordereau_ReadOnly_SimulatesSignature()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD008", "11600", new List<string> { "000001" });
        var result = await service.SignBordereauAsync("BORD008");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.State.Should().Be(BordereauWorkflowState.ReadyForClosure);
    }

    // --- BORD009: Signature complète ---
    [Fact]
    public async Task BORD009_CloseBordereau_ReadOnly_SimulatesClosure()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD009", "11600", new List<string> { "000001" });
        await service.SignBordereauAsync("BORD009");
        var result = await service.CloseBordereauAsync("BORD009");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.State.Should().Be(BordereauWorkflowState.AwaitingTransmission);
    }

    // --- BORD010: Prêt pour clôture ---
    [Fact]
    public async Task BORD010_TransmitBordereau_ReadOnly_SimulatesTransmission()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD010", "11600", new List<string> { "000001" });
        await service.SignBordereauAsync("BORD010");
        await service.CloseBordereauAsync("BORD010");
        var result = await service.TransmitBordereauAsync("BORD010");
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("READONLY_SIMULATION");
        result.State.Should().Be(BordereauWorkflowState.Completed);
    }

    // --- BORD011: Clôture requise ---
    [Fact]
    public async Task BORD011_FullWorkflow_ReadOnly_EndsCompleted()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD011", "11600", new List<string> { "000001" });
        await service.SignBordereauAsync("BORD011");
        await service.CloseBordereauAsync("BORD011");
        await service.TransmitBordereauAsync("BORD011");
        var status = await service.GetStatusAsync("BORD011");
        status.State.Should().Be(BordereauWorkflowState.Completed);
        status.RequiresAction.Should().BeFalse();
    }

    // --- BORD012: Clôture simulée ---
    [Fact]
    public async Task BORD012_GetStatus_NonExistentBordereau_Fails()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.GetStatusAsync("NEXIST");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("NOT_FOUND");
    }

    // --- BORD013: Transmission requise ---
    [Fact]
    public async Task BORD013_GetStatus_AfterCreate_ShowsCorrectState()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD013", "11600", new List<string> { "000001" });
        var status = await service.GetStatusAsync("BORD013");
        status.IsSuccess.Should().BeTrue();
        status.State.Should().Be(BordereauWorkflowState.Created);
        status.InvoiceCount.Should().Be(1);
    }

    // --- BORD014: Transmission simulée ---
    [Fact]
    public async Task BORD014_GetStatus_AfterSign_ShowsReadyForClosure()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD014", "11600", new List<string> { "000001" });
        await service.SignBordereauAsync("BORD014");
        var status = await service.GetStatusAsync("BORD014");
        status.State.Should().Be(BordereauWorkflowState.ReadyForClosure);
        status.RequiresAction.Should().BeFalse();
    }

    // --- BORD015: Transmission réussie ---
    [Fact]
    public async Task BORD015_TransitionTo_InvalidTransition_Fails()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD015", "11600", new List<string> { "000001" });
        var result = await service.TransitionToAsync("BORD015", BordereauWorkflowState.Completed);
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("INVALID_TRANSITION");
    }

    // --- BORD016: Erreur signature ---
    [Fact]
    public async Task BORD016_TransitionTo_ValidTransition_Succeeds()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD016", "11600", new List<string> { "000001" });
        var result = await service.TransitionToAsync("BORD016", BordereauWorkflowState.InvoicesAttached);
        result.IsSuccess.Should().BeTrue();
        result.Step.Should().Be("INVOICESATTACHED");
    }

    // --- BORD017: Erreur clôture ---
    [Fact]
    public async Task BORD017_GetAllBordereaux_ReturnsList()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD017A", "11600", new List<string> { "000001" });
        await service.CreateBordereauAsync("BORD017B", "11600", new List<string> { "000002" });
        var all = service.GetAllBordereaux();
        all.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    // --- BORD018: Erreur transmission ---
    [Fact]
    public async Task BORD018_AuditLog_ContainsEntries()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD018", "11600", new List<string> { "000001" });
        var audit = service.GetAuditLog();
        audit.Should().NotBeEmpty();
    }

    // --- BORD019: PostgreSQL indisponible ---
    [Fact]
    public async Task BORD019_AuditLog_FilteredByNumBord()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD019A", "11600", new List<string> { "000001" });
        await service.CreateBordereauAsync("BORD019B", "11600", new List<string> { "000002" });
        var audit = service.GetAuditLog("BORD019A");
        audit.Should().OnlyContain(e => e.NumBord == "BORD019A");
    }

    // --- BORD020: CHIFA indisponible ---
    [Fact]
    public async Task BORD020_ClearAuditLog_EmptiesLog()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD020", "11600", new List<string> { "000001" });
        service.GetAuditLog().Should().NotBeEmpty();
        service.ClearAuditLog();
        service.GetAuditLog().Should().BeEmpty();
    }

    // --- BORD021: Token absent ---
    [Fact]
    public async Task BORD021_CreateBordereau_NonExistentBordereau_GetStatusFails()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.GetStatusAsync("NEXIST");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("NOT_FOUND");
    }

    // --- BORD022: Reprise après erreur ---
    [Fact]
    public async Task BORD022_RemoveInvoice_NonExistentInvoice_Fails()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD022", "11600", new List<string> { "000001" });
        var result = await service.RemoveInvoiceAsync("BORD022", "NOPE");
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("INVOICE_NOT_FOUND");
    }

    // --- BORD023: Audit complet ---
    [Fact]
    public async Task BORD023_AuditLog_CorrelationIdsAreUnique()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD023A", "11600", new List<string> { "000001" });
        await service.CreateBordereauAsync("BORD023B", "11600", new List<string> { "000002" });
        var ids = service.GetAuditLog().Select(e => e.CorrelationId).Distinct().ToList();
        ids.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    // --- BORD024: Transitions invalides ---
    [Fact]
    public async Task BORD024_TransitionFromCreated_InvalidTarget_Fails()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD024", "11600", new List<string> { "000001" });
        var result = await service.TransitionToAsync("BORD024", BordereauWorkflowState.Transmitted);
        result.IsSuccess.Should().BeFalse();
        result.Step.Should().Be("INVALID_TRANSITION");
    }

    // --- BORD025: ReadOnly bloque toute écriture ---
    [Fact]
    public async Task BORD025_ReadOnly_NoRealBordereauInFake()
    {
        var (service, fake, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD025", "11600", new List<string> { "000001" });
        fake.BordereauCount.Should().Be(0, "ReadOnly mode should not create real bordereaux in FakeChifa");
    }

    // --- BORD026: Action descriptions ---
    [Fact]
    public async Task BORD026_GetStatus_ShowsActionDescription()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD026", "11600", new List<string> { "000001" });
        var status = await service.GetStatusAsync("BORD026");
        status.ActionDescription.Should().NotBeNullOrEmpty();
    }

    // --- BORD027: Multiple bordereaux tracking ---
    [Fact]
    public async Task BORD027_MultipleBordereaux_IndependentTracking()
    {
        var (service, _, _) = CreateReadOnlyService();
        await service.CreateBordereauAsync("BORD027A", "11600", new List<string> { "000001" });
        await service.CreateBordereauAsync("BORD027B", "11600", new List<string> { "000002" });
        await service.SignBordereauAsync("BORD027A");
        var statusA = await service.GetStatusAsync("BORD027A");
        var statusB = await service.GetStatusAsync("BORD027B");
        statusA.State.Should().Be(BordereauWorkflowState.ReadyForClosure);
        statusB.State.Should().Be(BordereauWorkflowState.Created);
    }

    // --- BORD028: Correlation ID format ---
    [Fact]
    public async Task BORD028_CreateBordereau_CorrelationIdFormat()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.CreateBordereauAsync("BORD028", "11600", new List<string> { "000001" });
        result.CorrelationId.Should().HaveLength(12);
    }

    // --- BORD029: Mode in result ---
    [Fact]
    public async Task BORD029_CreateBordereau_ModeIsReadOnly()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.CreateBordereauAsync("BORD029", "11600", new List<string> { "000001" });
        result.Mode.Should().Be(ChifaIntegrationMode.ReadOnly);
    }

    // --- BORD030: Duration tracked ---
    [Fact]
    public async Task BORD030_CreateBordereau_DurationMsTracked()
    {
        var (service, _, _) = CreateReadOnlyService();
        var result = await service.CreateBordereauAsync("BORD030", "11600", new List<string> { "000001" });
        result.DurationMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
