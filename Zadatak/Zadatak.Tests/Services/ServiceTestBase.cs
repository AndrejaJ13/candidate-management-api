using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Zadatak.Data;

namespace Zadatak.Tests.Services;

public abstract class ServiceTestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected ServiceTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new AppDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    protected AppDbContext DbContext { get; }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
