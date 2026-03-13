using System;
using MikroClean.Infrastructure.Security;

// Script para generar el hash de la contraseña del SuperAdmin
// Ejecutar: dotnet run --project GenerateHash.csproj

var passwordHasher = new BCryptPasswordHasher();
var password = "Admin123!";
var hash = passwordHasher.HashPassword(password);

Console.WriteLine("==========================================");
Console.WriteLine("HASH GENERADO PARA SUPERADMIN");
Console.WriteLine("==========================================");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash BCrypt: {hash}");
Console.WriteLine("==========================================");
Console.WriteLine("Copiar el hash de arriba y actualizar SeedData.cs");
