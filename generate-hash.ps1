# Script para generar el hash de la contraseña del SuperAdmin
# Ejecutar: .\generate-hash.ps1

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "GENERAR HASH PARA SUPERADMIN" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Compilar y ejecutar el programa de generación de hash
dotnet run --project MikroClean.Infrastructure --no-build --framework net8.0 -- generate-hash "Admin123!"

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "Instrucciones:" -ForegroundColor Yellow
Write-Host "1. Copia el hash generado arriba" -ForegroundColor White
Write-Host "2. Abre MikroClean.Infrastructure\Data\SeedData.cs" -ForegroundColor White
Write-Host "3. Reemplaza el PasswordHash del usuario superadmin" -ForegroundColor White
Write-Host "4. Ejecuta: dotnet ef database update" -ForegroundColor White
Write-Host "===========================================" -ForegroundColor Cyan
