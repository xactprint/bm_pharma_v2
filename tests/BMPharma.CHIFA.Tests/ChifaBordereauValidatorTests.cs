using FluentAssertions;
using BMPharma.CHIFA.Services;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaBordereauValidatorTests
{
    private readonly ChifaBordereauValidator _validator = new();

    [Fact]
    public void CH012_Valid_Bordereau_Passes()
    {
        var result = _validator.Validate("000216", "11600", new List<string> { "00000001" });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CH013_Empty_NumBord_Rejected()
    {
        var result = _validator.Validate("", "11600", new List<string> { "00000001" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_BORD_EMPTY");
    }

    [Fact]
    public void CH014_NumBord_Too_Long_Rejected()
    {
        var result = _validator.Validate("0000001", "11600", new List<string> { "00000001" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NUM_BORD_TOO_LONG");
    }

    [Fact]
    public void CH015_Empty_CodeCentre_Rejected()
    {
        var result = _validator.Validate("000216", "", new List<string> { "00000001" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "CODE_CENTRE_EMPTY");
    }

    [Fact]
    public void CH016_No_Invoices_Rejected()
    {
        var result = _validator.Validate("000216", "11600", new List<string>());

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "NO_INVOICES");
    }
}
