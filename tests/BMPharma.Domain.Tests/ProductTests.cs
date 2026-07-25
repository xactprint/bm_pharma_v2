using FluentAssertions;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Product_Should_Be_Created_With_Default_Values()
    {
        var product = new Product();

        product.Id.Should().NotBe(Guid.Empty);
        product.IsActive.Should().BeTrue();
        product.IsDeleted.Should().BeFalse();
        product.Batches.Should().BeEmpty();
        product.StockMovements.Should().BeEmpty();
    }

    [Fact]
    public void Product_Should_Have_Required_Properties()
    {
        var product = new Product
        {
            Code = "MED001",
            NameAr = "paracetamol",
            NameFr = "Paracetamol",
            Category = ProductCategory.Medicament,
            PriceDA = 150
        };

        product.Code.Should().Be("MED001");
        product.NameAr.Should().Be("paracetamol");
        product.NameFr.Should().Be("Paracetamol");
        product.Category.Should().Be(ProductCategory.Medicament);
        product.PriceDA.Should().Be(150);
    }
}
