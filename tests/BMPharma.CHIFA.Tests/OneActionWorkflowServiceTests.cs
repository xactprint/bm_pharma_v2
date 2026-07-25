using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class OneActionWorkflowServiceTests
{
    private readonly FakeChifaIntegrationProvider _fake;
    private readonly OneActionWorkflowService _workflow;

    public OneActionWorkflowServiceTests()
    {
        var fakeLogger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        _fake = new FakeChifaIntegrationProvider(fakeLogger.Object);
        _fake.SimulateTokenPresent();

        var validatorLogger = new Mock<ILogger<ChifaInvoiceValidator>>();
        var workflowLogger = new Mock<ILogger<OneActionWorkflowService>>();
        var auditLogger = new Mock<ILogger<StructuredChifaAuditService>>();
        var auditService = new StructuredChifaAuditService(auditLogger.Object);

        _workflow = new OneActionWorkflowService(
            _fake, _fake, _fake, _fake,
            new ChifaInvoiceValidator(validatorLogger.Object),
            new ChifaBordereauValidator(),
            new ChifaInvoiceMapper(),
            new ChifaBordereauMapper(),
            new ChifaWorkflowStateMachine(),
            auditService,
            workflowLogger.Object);
    }

    private Invoice CreateValidInvoice(string numFact = "00000001")
    {
        var product = new Product
        {
            Code = "42",
            CIPCode = "3400960001234",
            PriceDA = 150,
            NameFr = "Doliprane 1000mg",
            NameAr = "دوليبران 1000 ملغ"
        };

        return new Invoice
        {
            InvoiceNumber = numFact,
            InvoiceDate = DateTime.Today,
            UserId = Guid.NewGuid(),
            Customer = new Customer
            {
                InsuranceNumber = "123456789012",
                IsInsured = true,
                FullName = "Test Patient"
            },
            Lines = new List<InvoiceLine>
            {
                new()
                {
                    Quantity = 2,
                    UnitPriceDA = 150,
                    LineTotalDA = 300,
                    ProductId = product.Id,
                    Product = product
                }
            }
        };
    }

    [Fact]
    public async Task WF001_FullWorkflow_Succeeds_ThroughVisibleInChifa()
    {
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.Success.Should().BeTrue();
        invoice.ChifaState.Should().Be(ChifaWorkflowState.Signed);
        invoice.ChifaNumFact.Should().Be("00000001");
        invoice.ChifaMontFact.Should().Be(300);
        result.RequiresHumanAction.Should().BeTrue();
        result.HumanActionMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task WF002_FullWorkflow_RecordsAllSteps()
    {
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.Steps.Should().HaveCountGreaterOrEqualTo(4);
        result.Steps.Should().Contain(s => s.StepName == "VALIDATE_LOCAL");
        result.Steps.Should().Contain(s => s.StepName == "PREPARE_CHIFA");
        result.Steps.Should().Contain(s => s.StepName == "WRITE_CHIFA");
        result.Steps.Should().Contain(s => s.StepName == "CHECK_VISIBILITY");
    }

    [Fact]
    public async Task WF003_FullWorkflow_SetsIntegrationStateToSimulated()
    {
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        invoice.ChifaIntegrationState.Should().Be(ChifaIntegrationState.Simulated);
    }

    [Fact]
    public async Task WF004_FullWorkflow_FailsWhenCHIFALinesEmpty()
    {
        var invoice = new Invoice
        {
            InvoiceNumber = "00000001",
            InvoiceDate = DateTime.Today,
            UserId = Guid.NewGuid(),
            Lines = new List<InvoiceLine>()
        };

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no lines");
    }

    [Fact]
    public async Task WF005_FullWorkflow_FailsWhenOffline()
    {
        _fake.SimulateOffline();
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not available");
    }

    [Fact]
    public async Task WF006_FullWorkflow_FailsWhenNumFactEmpty()
    {
        var invoice = CreateValidInvoice("");
        invoice.InvoiceNumber = "";

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task WF007_FullWorkflow_SetsChifaLastUpdated()
    {
        var invoice = CreateValidInvoice();

        await _workflow.ExecuteFullWorkflowAsync(invoice);

        invoice.ChifaLastUpdated.Should().NotBeNull();
        invoice.ChifaLastUpdated!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task WF008_FullWorkflow_GeneratesCorrelationId()
    {
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.CorrelationId.Should().NotBeNullOrEmpty();
        result.CorrelationId.Should().HaveLength(8);
    }

    [Fact]
    public async Task WF009_FullWorkflow_RecordDuration()
    {
        var invoice = CreateValidInvoice();

        var result = await _workflow.ExecuteFullWorkflowAsync(invoice);

        result.TotalDurationMs.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task WF010_AssignBordereau_Succeeds()
    {
        var invoice = CreateValidInvoice();
        await _workflow.ExecuteFullWorkflowAsync(invoice);

        var bordereau = new Bordereau
        {
            BordereauNumber = "000216",
            BordereauDate = DateTime.Today,
            CnasType = "BORD_CNAS",
            UserId = Guid.NewGuid()
        };

        var result = await _workflow.AssignBordereauAsync(bordereau, new[] { invoice });

        result.Success.Should().BeTrue();
        invoice.ChifaState.Should().Be(ChifaWorkflowState.BordereauAssigned);
        invoice.ChifaNumBord.Should().Be("000216");
    }

    [Fact]
    public async Task WF011_AssignBordereau_WithMultipleInvoices()
    {
        var inv1 = CreateValidInvoice("00000001");
        var inv2 = CreateValidInvoice("00000002");
        await _workflow.ExecuteFullWorkflowAsync(inv1);
        await _workflow.ExecuteFullWorkflowAsync(inv2);

        var bordereau = new Bordereau
        {
            BordereauNumber = "000216",
            BordereauDate = DateTime.Today,
            CnasType = "BORD_CNAS",
            UserId = Guid.NewGuid()
        };

        var result = await _workflow.AssignBordereauAsync(bordereau, new[] { inv1, inv2 });

        result.Success.Should().BeTrue();
        inv1.ChifaNumBord.Should().Be("000216");
        inv2.ChifaNumBord.Should().Be("000216");
    }
}
