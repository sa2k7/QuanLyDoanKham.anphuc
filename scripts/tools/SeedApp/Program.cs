using System;
using System.Text;
using Microsoft.Data.Sqlite;

Console.OutputEncoding = Encoding.UTF8;
var dbPath = @"d:\QuanLyDoanKham\QuanLyDoanKham.API\QuanLyDoanKham.db";
using var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();
using var cmd = connection.CreateCommand();
cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
using var reader = cmd.ExecuteReader();
var sb = new StringBuilder();
sb.AppendLine("=== TABLES ===");
while (reader.Read()) sb.AppendLine(reader.GetString(0));
System.IO.File.WriteAllText(@"d:\QuanLyDoanKham\tables_list.txt", sb.ToString(), Encoding.UTF8);
Console.WriteLine("Written to tables_list.txt");
