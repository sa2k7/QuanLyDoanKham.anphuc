using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using Xunit;
using System.Linq;

namespace QuanLyDoanKham.Api.Tests.Services
{
    public class SchemaDiagnosticTests
    {
        [Fact]
        public void VerifySchemaCreation()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var db = new ApplicationDbContext(options);
            db.Database.EnsureCreated();

            var tables = db.Database.GetDbConnection().CreateCommand();
            tables.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            
            var results = new System.Collections.Generic.List<string>();
            using (var reader = tables.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(reader.GetString(0));
                }
            }

            // Print table names to standard output (captured by test runner)
            foreach (var table in results)
            {
                // In generic terms, just verify a core table exists
                if (table == "MedicalGroups") return;
            }

            Assert.True(results.Contains("MedicalGroups"), $"Table 'MedicalGroups' not found. Tables found: {string.Join(", ", results)}");
        }
    }
}
