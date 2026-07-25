using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class FakeChifaIntegrationProviderTests
{
    private readonly FakeChifaIntegrationProvider _fake;

    public FakeChifaIntegrationProviderTests()
    {
        var logger = new Mock<ILogger<FakeChifaIntegrationProvider>>();
        _fake = new FakeChifaIntegrationProvider(logger.Object);
    }

    [Fact]
    public async Task FAKE001_InitiallyOnline()
    {
        (await _fake.IsChifaAvailableAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task FAKE002_SimulateOffline()
    {
        _fake.SimulateOffline();
        (await _fake.IsChifaAvailableAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task FAKE003_SimulateOnline()
    {
        _fake.SimulateOffline();
        _fake.SimulateOnline();
        (await _fake.IsChifaAvailableAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task FAKE004_InitiallyNoToken()
    {
        (await _fake.IsTokenAvailableAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task FAKE005_SimulateTokenPresent()
    {
        _fake.SimulateTokenPresent();
        (await _fake.IsTokenAvailableAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task FAKE006_CreateInvoice_WhenOnline_Succeeds()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new() { NumEnr = "00001", MedicCode = 1, PrixUnit = 150, Quantite = 2 }
            }
        };

        var result = await _fake.CreateInvoiceAsync(request);

        result.Success.Should().BeTrue();
        result.ChifaNumFact.Should().Be("00000001");
        _fake.InvoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task FAKE007_CreateInvoice_WhenOffline_Fails()
    {
        _fake.SimulateOffline();

        var result = await _fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("offline");
    }

    [Fact]
    public async Task FAKE008_CreateInvoice_Duplicate_Fails()
    {
        var request = new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600
        };

        await _fake.CreateInvoiceAsync(request);
        var result = await _fake.CreateInvoiceAsync(request);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task FAKE009_InvoiceExistsInChifa_AfterCreate()
    {
        await _fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123456789012",
            CodeCentre = 11600
        });

        (await _fake.InvoiceExistsInChifaAsync("00000001")).Should().BeTrue();
    }

    [Fact]
    public async Task FAKE010_InvoiceExistsInChifa_BeforeCreate()
    {
        (await _fake.InvoiceExistsInChifaAsync("99999999")).Should().BeFalse();
    }

    [Fact]
    public async Task FAKE011_CreateBordereau_WithValidInvoices_Succeeds()
    {
        await _fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123",
            CodeCentre = 11600
        });

        var result = await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string> { "00000001" }
        });

        result.Success.Should().BeTrue();
        _fake.BordereauCount.Should().Be(1);
    }

    [Fact]
    public async Task FAKE012_CreateBordereau_WithMissingInvoice_Fails()
    {
        var result = await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string> { "99999999" }
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task FAKE013_GetNextBordereauNumber_Increments()
    {
        var n1 = await _fake.GetNextBordereauNumberAsync();
        var n2 = await _fake.GetNextBordereauNumberAsync();

        n1.Should().Be("000001");
        n2.Should().Be("000002");
    }

    [Fact]
    public async Task FAKE014_SignBordereau_WithoutToken_Fails()
    {
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        var result = await _fake.SignBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("token");
    }

    [Fact]
    public async Task FAKE015_SignBordereau_WithToken_Succeeds()
    {
        _fake.SimulateTokenPresent();
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        var result = await _fake.SignBordereauAsync("000216");

        result.Success.Should().BeTrue();
        result.SignatureId.Should().StartWith("SIM-");
    }

    [Fact]
    public async Task FAKE016_CloseBordereau_BeforeSigning_Fails()
    {
        _fake.SimulateTokenPresent();
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        var result = await _fake.CloseBordereauAsync("000216");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("signed");
    }

    [Fact]
    public async Task FAKE017_CloseBordereau_AfterSigning_Succeeds()
    {
        _fake.SimulateTokenPresent();
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });
        await _fake.SignBordereauAsync("000216");

        var result = await _fake.CloseBordereauAsync("000216");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FAKE018_SigningStatus_NotSigned_BeforeSigning()
    {
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        var status = await _fake.GetSigningStatusAsync("000216");

        status.Should().Be(ChifaSigningStatus.SigningRequired);
    }

    [Fact]
    public async Task FAKE019_SigningStatus_Signed_AfterSigning()
    {
        _fake.SimulateTokenPresent();
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });
        await _fake.SignBordereauAsync("000216");

        var status = await _fake.GetSigningStatusAsync("000216");

        status.Should().Be(ChifaSigningStatus.Signed);
    }

    [Fact]
    public async Task FAKE020_GetTokenInfo_WhenNotPresent_ReturnsNull()
    {
        var info = await _fake.GetTokenInfoAsync();
        info.Should().BeNull();
    }

    [Fact]
    public async Task FAKE021_GetTokenInfo_WhenPresent_ReturnsInfo()
    {
        _fake.SimulateTokenPresent();
        var info = await _fake.GetTokenInfoAsync();
        info.Should().NotBeNull();
        info!.SerialNumber.Should().Be("SIM-TOKEN-001");
    }

    [Fact]
    public async Task FAKE022_GetHealthStatus_WhenOnline()
    {
        var health = await _fake.GetHealthStatusAsync();
        health.IsOnline.Should().BeTrue();
        health.IsDatabaseConnected.Should().BeTrue();
    }

    [Fact]
    public async Task FAKE023_GetHealthStatus_WhenOffline()
    {
        _fake.SimulateOffline();
        var health = await _fake.GetHealthStatusAsync();
        health.IsOnline.Should().BeFalse();
        health.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task FAKE024_Reset_ClearsAll()
    {
        _fake.SimulateTokenPresent();
        await _fake.CreateInvoiceAsync(new ChifaInvoiceRequest { NumFact = "00000001" });

        _fake.Reset();

        _fake.InvoiceCount.Should().Be(0);
        _fake.BordereauCount.Should().Be(0);
    }

    [Fact]
    public async Task FAKE025_AuditLog_RecordsOperations()
    {
        await _fake.CreateInvoiceAsync(new ChifaInvoiceRequest
        {
            NumFact = "00000001",
            NumAssure = "123",
            CodeCentre = 11600
        });

        _fake.AuditLog.Should().HaveCount(1);
        _fake.AuditLog[0].Operation.Should().Be("CREATE_INVOICE");
        _fake.AuditLog[0].Result.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task FAKE026_CreateBordereau_Duplicate_Fails()
    {
        await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        var result = await _fake.CreateBordereauAsync(new ChifaBordereauRequest
        {
            NumBord = "000216",
            InvoiceNumbers = new List<string>()
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task FAKE027_SignBordereau_NotFound_Fails()
    {
        var result = await _fake.SignBordereauAsync("999999");
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FAKE028_CloseBordereau_NotFound_Fails()
    {
        var result = await _fake.CloseBordereauAsync("999999");
        result.Success.Should().BeFalse();
    }
}
