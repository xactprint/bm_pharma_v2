using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaBordereauMapperTests
{
    private readonly ChifaBordereauMapper _mapper = new();

    [Fact]
    public void BMAP001_MapNumBord_PadsZeros()
    {
        var result = _mapper.MapNumBord("216");
        result.Should().Be("000216");
    }

    [Fact]
    public void BMAP002_MapNumBord_TruncatesTo6()
    {
        var result = _mapper.MapNumBord("12345678");
        result.Should().Be("345678");
    }

    [Fact]
    public void BMAP003_MapNumBord_RemovesDashes()
    {
        var result = _mapper.MapNumBord("12-34");
        result.Should().Be("001234");
    }

    [Fact]
    public void BMAP004_MapNumBord_EmptyReturnsEmpty()
    {
        var result = _mapper.MapNumBord("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void BMAP005_MapToChifaRequest_IncludesInvoiceNumbers()
    {
        var bordereau = new Bordereau
        {
            BordereauNumber = "000216",
            BordereauDate = DateTime.Today,
            CnasType = "BORD_CNAS"
        };

        var invoices = new List<Invoice>
        {
            new() { InvoiceNumber = "00000001", ChifaNumFact = "00000001" },
            new() { InvoiceNumber = "00000002", ChifaNumFact = "00000002" }
        };

        var result = _mapper.MapToChifaRequest(bordereau, invoices);

        result.NumBord.Should().Be("000216");
        result.InvoiceNumbers.Should().HaveCount(2);
        result.InvoiceNumbers.Should().Contain("00000001");
        result.InvoiceNumbers.Should().Contain("00000002");
    }

    [Fact]
    public void BMAP006_MapToChifaRequest_ExcludesInvoicesWithoutChifaNum()
    {
        var bordereau = new Bordereau
        {
            BordereauNumber = "000216",
            CnasType = "BORD_CNAS"
        };

        var invoices = new List<Invoice>
        {
            new() { ChifaNumFact = "00000001" },
            new() { ChifaNumFact = null }
        };

        var result = _mapper.MapToChifaRequest(bordereau, invoices);

        result.InvoiceNumbers.Should().HaveCount(1);
    }

    [Fact]
    public void BMAP007_MapToChifaRequest_ThrowsIfBordereauNull()
    {
        var act = () => _mapper.MapToChifaRequest(null!, Enumerable.Empty<Invoice>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BMAP008_MapToChifaRequest_UsesDefaultType()
    {
        var bordereau = new Bordereau
        {
            BordereauNumber = "000216",
            CnasType = null
        };

        var result = _mapper.MapToChifaRequest(bordereau, Enumerable.Empty<Invoice>());

        result.TypeBord.Should().Be("BORD_CNAS");
    }

    [Fact]
    public void BMAP009_ApplyChifaFieldsToBordereau_SetsSignatureId()
    {
        var bordereau = new Bordereau { BordereauNumber = "000216" };
        var result = new ChifaBordereauResult
        {
            Success = true,
            NumBord = "000216",
            SignatureId = "SIG-123"
        };

        _mapper.ApplyChifaFieldsToBordereau(bordereau, result);

        bordereau.SignatureId.Should().Be("SIG-123");
    }
}
