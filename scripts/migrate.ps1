# InventoryPro - Database Migration Script
# This script runs all Entity Framework migrations for all services

Write-Host "InventoryPro - Running Database Migrations" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Check if dotnet ef is installed
$efInstalled = dotnet ef --version 2>$null
if (-not $efInstalled) {
    Write-Host "Entity Framework Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Get solution root directory
$solutionRoot = Split-Path -Parent $PSScriptRoot

# Function to run migrations for a service
function Run-Migration {
    param (
        [string]$ServiceName,
        [string]$ServicePath
    )
    
    Write-Host "`nMigrating $ServiceName..." -ForegroundColor Green
    
    Push-Location $ServicePath
    try {
        # Check if migrations folder exists
        if (Test-Path "Migrations") {
            Write-Host "Running migrations for $ServiceName" -ForegroundColor Yellow
            dotnet ef database update
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "$ServiceName migration completed successfully!" -ForegroundColor Green
            } else {
                Write-Host "$ServiceName migration failed!" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "Creating initial migration for $ServiceName" -ForegroundColor Yellow
            dotnet ef migrations add InitialCreate
            dotnet ef database update
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "$ServiceName initial migration completed successfully!" -ForegroundColor Green
            } else {
                Write-Host "$ServiceName initial migration failed!" -ForegroundColor Red
                exit 1
            }
        }
    }
    finally {
        Pop-Location
    }
}

# Run migrations for each service
Run-Migration "Auth Service" "$solutionRoot\src\Services\InventoryPro.AuthService"
Run-Migration "Product Service" "$solutionRoot\src\Services\InventoryPro.ProductService"
Run-Migration "Sales Service" "$solutionRoot\src\Services\InventoryPro.SalesService"

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "All migrations completed successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

# Ask if user wants to run the SQL setup script
$runSqlScript = Read-Host "`nDo you want to run the SQL setup script for sample data? (Y/N)"
if ($runSqlScript -eq 'Y' -or $runSqlScript -eq 'y') {
    $sqlScriptPath = "$solutionRoot\scripts\database-setup.sql"
    if (Test-Path $sqlScriptPath) {
        Write-Host "Running SQL setup script..." -ForegroundColor Yellow
        sqlcmd -S .\SQLEXPRESS -i $sqlScriptPath
        Write-Host "SQL setup script completed!" -ForegroundColor Green
    } else {
        Write-Host "SQL setup script not found at: $sqlScriptPath" -ForegroundColor Red
    }
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")