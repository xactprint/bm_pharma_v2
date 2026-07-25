using FluentAssertions;
using BMPharma.Shared.Results;
using Xunit;

namespace BMPharma.Application.Tests;

public class ResultTests
{
    [Fact]
    public void Result_Success_Should_Have_Value()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_Failure_Should_Have_Error()
    {
        var result = Result<string>.Failure("something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Result_NonGeneric_Success_Should_Work()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }
}
