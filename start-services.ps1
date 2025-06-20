#!/usr/bin/env pwsh
# PowerShell script to start all InventoryPro services
# This script starts all required services for the InventoryPro application

Write-Host "Starting InventoryPro Services..." -ForegroundColor Green

# Define service paths
$services = @(
    @{
        Name = "AuthService"
        Path = "src/Services/InventoryPro.AuthService"
        Port = 5141
    },
    @{
        Name = "ProductService" 
        Path = "src/Services/InventoryPro.ProductService"
        Port = 5089
    },
    @{
        Name = "SalesService"
        Path = "src/Services/InventoryPro.SalesService" 
        Port = 5282
    },
    @{
        Name = "ReportService"
        Path = "src/Services/InventoryPro.ReportService"
        Port = 5179
    },
    @{
        Name = "Gateway"
        Path = "src/Gateway/InventoryPro.Gateway"
        Port = 5000
    }
)

# Function to check if port is in use
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Array to store background jobs
$jobs = @()

# Start each service
foreach ($service in $services) {
    Write-Host "Starting $($service.Name) on port $($service.Port)..." -ForegroundColor Yellow
    
    # Check if port is already in use
    if (Test-Port -Port $service.Port) {
        Write-Host "$($service.Name) appears to already be running on port $($service.Port)" -ForegroundColor Cyan
        continue
    }
    
    # Start the service in background
    $job = Start-Job -ScriptBlock {
        param($servicePath)
        Set-Location $servicePath
        dotnet run --no-launch-profile
    } -ArgumentList $service.Path -Name $service.Name
    
    $jobs += $job
    Write-Host "$($service.Name) started (Job ID: $($job.Id))" -ForegroundColor Green
    
    # Wait a bit between starting services
    Start-Sleep -Seconds 2
}

Write-Host "`nAll services started!" -ForegroundColor Green
Write-Host "Services running:" -ForegroundColor Cyan

foreach ($service in $services) {
    Write-Host "  - $($service.Name): http://localhost:$($service.Port)" -ForegroundColor White
}

Write-Host "`nPress Ctrl+C to stop all services" -ForegroundColor Yellow
Write-Host "Or run: Get-Job | Stop-Job; Get-Job | Remove-Job" -ForegroundColor Yellow

# Wait for user input to stop services
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host "`nStopping all services..." -ForegroundColor Red
    $jobs | Stop-Job
    $jobs | Remove-Job
    Write-Host "All services stopped." -ForegroundColor Red
}