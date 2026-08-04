using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Sneaker_Store.Data;

namespace Unittest;

// Fælles opsætning der gælder for ALLE integrationstests.
// VIGTIGT: Databasen oprettes i [SetUp], IKKE i en constructor.
// NUnit genbruger som standard samme instans af testklassen til alle tests i klassen,
// så en constructor kører kun ÉN gang for hele klassen (data ville "lække" mellem tests).
// [SetUp] kører derimod før HVER enkelt test, hvilket giver en frisk, tom database
// pr. test - det er det, der sikrer ingen delt state (jf. anti-pattern-slidet fra pensum).
public abstract class IntegrationTestBase
{
    protected AppDbContext Db { get; private set; } = null!;

    [SetUp]
    public void BaseSetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unikt navn => ingen delt state mellem tests
            .Options;
        Db = new AppDbContext(options);
    }

    [TearDown]
    public void BaseTearDown()
    {
        Db.Dispose();
    }
}