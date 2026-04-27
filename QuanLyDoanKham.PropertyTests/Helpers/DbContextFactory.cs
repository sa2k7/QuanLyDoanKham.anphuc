using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.PropertyTests.Helpers;

/// <summary>Factory tạo ApplicationDbContext in-memory cho property tests</summary>
public static class DbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
