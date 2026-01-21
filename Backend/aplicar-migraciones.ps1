﻿Write-Host "🚀 Aplicando Migraciones de Entity Framework..." -ForegroundColor Cyan
Write-Host ""

# Cambiar al directorio de la aplicación
Set-Location "C:\Users\Ignacio Grudine\OneDrive\Escritorio\Agendify\Agendify\Backend\Application"

# Verificar que SQL Server está corriendo
Write-Host "📊 Verificando SQL Server..." -ForegroundColor Yellow
$sqlRunning = docker ps --filter "name=agendify-sqlserver" --format "{{.Status}}"

if (-not $sqlRunning) {
    Write-Host "⚠️  SQL Server no está corriendo. Iniciando..." -ForegroundColor Yellow
    Set-Location ".."
    docker-compose up -d sqlserver
    Write-Host "⏳ Esperando 60 segundos para que SQL Server inicie..." -ForegroundColor Yellow
    Start-Sleep -Seconds 60
    Set-Location "Application"
} else {
    Write-Host "✅ SQL Server está corriendo" -ForegroundColor Green
}

Write-Host ""
Write-Host "🗑️  Eliminando base de datos anterior si existe..." -ForegroundColor Yellow
dotnet ef database drop --force --verbose

Write-Host ""
Write-Host "🧹 Eliminando migraciones anteriores..." -ForegroundColor Yellow
$migrationsPath = "Migrations"
if (Test-Path $migrationsPath) {
    Remove-Item -Path $migrationsPath -Recurse -Force
    Write-Host "✅ Migraciones anteriores eliminadas" -ForegroundColor Green
} else {
    Write-Host "✅ No hay migraciones anteriores" -ForegroundColor Green
}

Write-Host ""
Write-Host "📝 Creando nueva migración..." -ForegroundColor Cyan
dotnet ef migrations add InitialCreate --verbose

Write-Host ""
Write-Host "📦 Aplicando migraciones..." -ForegroundColor Cyan
dotnet ef database update --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ ¡Migraciones aplicadas correctamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ahora puedes conectarte a la base de datos:" -ForegroundColor Cyan
    Write-Host "  Server: localhost,1433" -ForegroundColor White
    Write-Host "  Database: AgendifyDb" -ForegroundColor White
    Write-Host "  User: sa" -ForegroundColor White
    Write-Host "  Password: YourStrong@Passw0rd" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ Error al aplicar migraciones" -ForegroundColor Red
    Write-Host "Revisa el error anterior" -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Presiona Enter para salir"

