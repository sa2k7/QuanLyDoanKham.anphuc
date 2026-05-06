using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.PropertyTests.Helpers;

/// <summary>Factory tạo ApplicationDbContext in-memory cho property tests</summary>
public static class DbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            // Suppress the "transactions are ignored" warning from the in-memory provider.
            // Services that call BeginTransactionAsync will still execute correctly —
            // the in-memory provider simply treats the transaction as a no-op.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }
}
