using System;
using System.IO;
var hash = BCrypt.Net.BCrypt.HashPassword("admin123", 11);
Console.WriteLine(hash);
File.WriteAllText("admin_hash.txt", hash);
