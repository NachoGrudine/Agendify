Write-Host "🚀 Recreando Base de Datos y Migraciones..." -ForegroundColor Cyan
Write-Host ""

# Guardar directorio actual
$rootPath = $PSScriptRoot

# Verificar que Docker está corriendo
Write-Host "🐋 Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "✅ Docker está corriendo" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker no está corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Write-Host "📊 Verificando SQL Server..." -ForegroundColor Yellow
$sqlStatus = docker ps --filter "name=agendify-sqlserver" --format "{{.Status}}"

if (-not $sqlStatus) {
    Write-Host "⚠️  SQL Server no está corriendo. Iniciando..." -ForegroundColor Yellow
    Set-Location $rootPath
    docker-compose up -d sqlserver
    Write-Host "⏳ Esperando 30 segundos para que SQL Server inicie..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
} else {
    Write-Host "✅ SQL Server está corriendo" -ForegroundColor Green
}

# Cambiar al directorio del Backend
Set-Location "$rootPath\Backend\Agendify"

Write-Host ""
Write-Host "🧹 Eliminando migraciones anteriores (locales)..." -ForegroundColor Yellow
$migrationsPath = "Migrations"
if (Test-Path $migrationsPath) {
    Remove-Item -Path $migrationsPath -Recurse -Force
    Write-Host "✅ Migraciones anteriores eliminadas" -ForegroundColor Green
} else {
    Write-Host "✅ No hay migraciones anteriores" -ForegroundColor Green
}

Write-Host ""
Write-Host "🗑️  Eliminando base de datos..." -ForegroundColor Yellow
Write-Host "⚠️  Conectando temporalmente para eliminar la BD..." -ForegroundColor Yellow

# Crear un appsettings temporal con la connection string
$tempSettings = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AgendifyDb;User Id=sa;Password=Ag3nd1fyDB@S3cur3#2026!;TrustServerCertificate=True;"
  }
}
"@

$tempSettingsPath = "appsettings.temp.json"
$tempSettings | Out-File -FilePath $tempSettingsPath -Encoding UTF8

try {
    dotnet ef database drop --force --connection "Server=localhost,1433;Database=AgendifyDb;User Id=sa;Password=Ag3nd1fyDB@S3cur3#2026!;TrustServerCertificate=True;" 2>&1 | Out-Null
    Write-Host "✅ Base de datos eliminada" -ForegroundColor Green
} catch {
    Write-Host "✅ No había base de datos existente" -ForegroundColor Green
} finally {
    if (Test-Path $tempSettingsPath) {
        Remove-Item $tempSettingsPath -Force
    }
}

Write-Host ""
Write-Host "📝 Creando nueva migración InitialCreate..." -ForegroundColor Cyan
dotnet ef migrations add InitialCreate

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Error al crear la migración" -ForegroundColor Red
    Set-Location $rootPath
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host "✅ Migración creada exitosamente" -ForegroundColor Green

Write-Host ""
Write-Host "🔨 Reconstruyendo contenedores con la nueva migración..." -ForegroundColor Yellow
Set-Location $rootPath
docker-compose up -d --build

Write-Host ""
Write-Host "⏳ Esperando 30 segundos para que el API aplique las migraciones automáticamente..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host ""
Write-Host "📜 Verificando logs del API..." -ForegroundColor Cyan
Write-Host ""
docker logs agendify-api --tail 40 | Select-String -Pattern "Migr|Error|Aplicando|Database|exitosamente"

Write-Host ""
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ ¡Proceso completado!" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📌 Resumen:" -ForegroundColor Cyan
Write-Host "  • Migraciones anteriores eliminadas" -ForegroundColor White
Write-Host "  • Base de datos eliminada" -ForegroundColor White
Write-Host "  • Nueva migración 'InitialCreate' creada" -ForegroundColor White
Write-Host "  • Contenedores reconstruidos" -ForegroundColor White
Write-Host "  • Migraciones aplicadas automáticamente al iniciar" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Conexión a la base de datos:" -ForegroundColor Cyan
Write-Host "  Server: localhost,1433" -ForegroundColor White
Write-Host "  Database: AgendifyDb" -ForegroundColor White
Write-Host "  User: sa" -ForegroundColor White
Write-Host "  Password: (ver archivo .env)" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Servicios disponibles:" -ForegroundColor Cyan
Write-Host "  • API: http://localhost:5000" -ForegroundColor White
Write-Host "  • Frontend: http://localhost:4200" -ForegroundColor White
Write-Host "  • Swagger: http://localhost:5000/swagger" -ForegroundColor White

# Volver al directorio raíz
Set-Location $rootPath

Write-Host ""
Read-Host "Presiona Enter para salir"




