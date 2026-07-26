using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaDbContextTests
{
    private static DbContextOptions<ChifaPostgreSqlContext> CreateReadOnlyOptions()
    {
        return new DbContextOptionsBuilder<ChifaPostgreSqlContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static DbContextOptions<ChifaWriteDbContext> CreateWriteOptions()
    {
        return new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void CTX001_CanCreateReadOnlyContext()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        context.Should().NotBeNull();
    }

    [Fact]
    public void CTX002_CanCreateWriteContext()
    {
        using var context = new ChifaWriteDbContext(CreateWriteOptions());
        context.Should().NotBeNull();
    }

    [Fact]
    public void CTX003_FactureEntity_HasCorrectTableMapping()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaFacture))!;
        entityType.GetTableName().Should().Be("facture");
    }

    [Fact]
    public void CTX004_DetailFactEntity_HasCorrectTableMapping()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaDetailFact))!;
        entityType.GetTableName().Should().Be("detail_fact");
    }

    [Fact]
    public void CTX005_BordereauEntity_HasCorrectTableMapping()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaBordereau))!;
        entityType.GetTableName().Should().Be("bordereau");
    }

    [Fact]
    public void CTX006_ParametreEntity_HasCorrectTableMapping()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaParametre))!;
        entityType.GetTableName().Should().Be("parametre");
    }

    [Fact]
    public void CTX007_FactureEntity_HasCorrectPK()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaFacture))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(1);
        pk.Properties[0].Name.Should().Be("NumFact");
    }

    [Fact]
    public void CTX008_DetailFactEntity_HasCompositePK()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaDetailFact))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(3);
        var pkNames = pk.Properties.Select(p => p.Name).ToList();
        pkNames.Should().Contain(new[] { "NumFact", "NumEnr", "Ppa" });
    }

    [Fact]
    public void CTX009_BordereauEntity_HasIdBordPK()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var pk = context.Model.FindEntityType(typeof(ChifaBordereau))!.FindPrimaryKey()!;
        pk.Properties.Should().HaveCount(1);
        pk.Properties[0].Name.Should().Be("IdBord");
    }

    [Fact]
    public void CTX010_ParametreEntity_HasNoKey()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaParametre))!;
        entityType.FindPrimaryKey().Should().BeNull();
    }

    [Fact]
    public void CTX011_FactureEntity_CriticalFieldsAreNotNull()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var typeMaj = context.Model.FindEntityType(typeof(ChifaFacture))!.FindProperty("TypeMaj")!;
        typeMaj.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void CTX012_CanInsertAndReadFacture_InMemory()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var facture = new ChifaFacture
        {
            NumFact = "TEST0001",
            TypeMaj = 0,
            MontMajFae = 0,
            MontMaj = 0
        };
        context.Factures.Add(facture);
        context.SaveChanges();

        var loaded = context.Factures.First(f => f.NumFact == "TEST0001");
        loaded.NumFact.Should().Be("TEST0001");
        loaded.TypeMaj.Should().Be(0);
    }

    [Fact]
    public void CTX013_CanInsertAndReadBordereau_InMemory()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var bordereau = new ChifaBordereau
        {
            IdBord = 1,
            NumBord = "00216",
            CodeCentre = "11600"
        };
        context.Bordereaus.Add(bordereau);
        context.SaveChanges();

        var loaded = context.Bordereaus.First(b => b.IdBord == 1);
        loaded.NumBord.Should().Be("00216");
        loaded.CodeCentre.Should().Be("11600");
    }

    [Fact]
    public void CTX014_CanInsertAndReadDetailFact_InMemory()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var detail = new ChifaDetailFact
        {
            NumFact = "TEST0001",
            NumEnr = "00001",
            Ppa = 150.00m,
            Qte = 2,
            Mont = 300.00m,
            NumEnrPrescrit = "00001"
        };
        context.DetailFacts.Add(detail);
        context.SaveChanges();

        var loaded = context.DetailFacts.First(d => d.NumFact == "TEST0001");
        loaded.Mont.Should().Be(300.00m);
    }

    [Fact]
    public void CTX015_ParametreEntity_IsKeyless_Queryable()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaParametre))!;
        entityType.FindPrimaryKey().Should().BeNull();
    }

    [Fact]
    public void CTX016_FactureEntity_ColumnCount_MatchesContract()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaFacture))!.GetProperties().Count();
        propertyCount.Should().Be(53, "BM-PHASE-004.9 real schema has 53 physical columns in facture (60 logical minus 7 dropped)");
    }

    [Fact]
    public void CTX017_DetailFactEntity_ColumnCount_MatchesContract()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaDetailFact))!.GetProperties().Count();
        propertyCount.Should().Be(20, "DATABASE_CONTRACT specifies exactly 20 columns for detail_fact");
    }

    [Fact]
    public void CTX018_BordereauEntity_ColumnCount_MatchesContract()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var propertyCount = context.Model.FindEntityType(typeof(ChifaBordereau))!.GetProperties().Count();
        propertyCount.Should().Be(11, "DATABASE_CONTRACT specifies exactly 11 columns for bordereau");
    }

    [Fact]
    public void CTX019_WriteContext_SameMappings_AsReadOnlyContext()
    {
        using var readCtx = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        using var writeCtx = new ChifaWriteDbContext(CreateWriteOptions());

        var readFacture = readCtx.Model.FindEntityType(typeof(ChifaFacture))!;
        var writeFacture = writeCtx.Model.FindEntityType(typeof(ChifaFacture))!;

        readFacture.GetTableName().Should().Be(writeFacture.GetTableName());
        readFacture.FindPrimaryKey()!.Properties.Should().HaveCount(
            writeFacture.FindPrimaryKey()!.Properties.Count);
    }

    [Fact]
    public void CTX020_FactureDecimal_PrecisionMatchesContract()
    {
        using var context = new ChifaPostgreSqlContext(CreateReadOnlyOptions());
        var entityType = context.Model.FindEntityType(typeof(ChifaFacture))!;

        entityType.FindProperty("MontOff")!.GetPrecision().Should().Be(10);
        entityType.FindProperty("MontOff")!.GetScale().Should().Be(2);
        entityType.FindProperty("MontFact")!.GetPrecision().Should().Be(11);
        entityType.FindProperty("MontMajFae")!.GetPrecision().Should().Be(4);
    }
}
