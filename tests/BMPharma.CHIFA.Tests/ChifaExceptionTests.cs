using FluentAssertions;
using BMPharma.CHIFA.Services;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaExceptionTests
{
    [Fact]
    public void EXC001_ChifaConnectionException_Properties()
    {
        var inner = new Exception("inner");
        var ex = new ChifaConnectionException("server1", "connection failed", inner);
        ex.Server.Should().Be("server1");
        ex.Message.Should().Be("connection failed");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void EXC002_ChifaConnectionException_WithoutInner()
    {
        var ex = new ChifaConnectionException("server1", "fail");
        ex.Server.Should().Be("server1");
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void EXC003_ChifaValidationException_WithErrors()
    {
        var errors = new List<ChifaValidationError>
        {
            new() { Code = "REQUIRED", Field = "NumFact", Message = "Required" }
        };
        var ex = new ChifaValidationException("validation failed", errors);
        ex.Errors.Should().HaveCount(1);
        ex.Errors[0].Code.Should().Be("REQUIRED");
    }

    [Fact]
    public void EXC004_ChifaValidationException_WithoutErrors()
    {
        var ex = new ChifaValidationException("fail");
        ex.Errors.Should().BeEmpty();
    }

    [Fact]
    public void EXC005_ChifaValidationError_Properties()
    {
        var err = new ChifaValidationError
        {
            Code = "INVALID", Field = "Montant", Message = "Must be positive"
        };
        err.Code.Should().Be("INVALID");
        err.Field.Should().Be("Montant");
        err.Message.Should().Be("Must be positive");
    }

    [Fact]
    public void EXC006_ChifaWriteException_Properties()
    {
        var inner = new Exception("db error");
        var ex = new ChifaWriteException("Facture", "FAC001", "write failed", inner);
        ex.EntityType.Should().Be("Facture");
        ex.EntityKey.Should().Be("FAC001");
        ex.Message.Should().Contain("write failed");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void EXC007_ChifaConcurrencyException_Properties()
    {
        var ex = new ChifaConcurrencyException("Facture", "concurrent modification");
        ex.Resource.Should().Be("Facture");
        ex.Message.Should().Contain("concurrent modification");
    }

    [Fact]
    public void EXC008_ChifaSynchronizationException_Properties()
    {
        var ex = new ChifaSynchronizationException("CHIFA_API", "sync failed");
        ex.Source.Should().Be("CHIFA_API");
    }

    [Fact]
    public void EXC009_ChifaVisibilityException_Properties()
    {
        var ex = new ChifaVisibilityException("Facture", "FAC001", "not visible");
        ex.EntityType.Should().Be("Facture");
        ex.EntityKey.Should().Be("FAC001");
    }

    [Fact]
    public void EXC010_ChifaVisibilityException_WithInner()
    {
        var inner = new Exception("inner");
        var ex = new ChifaVisibilityException("Facture", "FAC001", "not visible", inner);
        ex.InnerException.Should().Be(inner);
    }
}
