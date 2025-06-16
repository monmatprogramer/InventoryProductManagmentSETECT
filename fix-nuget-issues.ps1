# PowerShell script to fix NuGet package restore issues
# Run this script from the InventoryPro root directory

Write-Host "🔧 Starting NuGet package restore fix..." -ForegroundColor Cyan

# Step 1: Clear all NuGet caches
Write-Host "Step 1: Clearing NuGet caches..." -ForegroundColor Yellow
try {
    dotnet nuget locals all --clear
    Write-Host "✅ NuGet caches cleared successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to clear NuGet caches: $_" -ForegroundColor Red
}

# Step 2: Remove bin and obj folders
Write-Host "Step 2: Removing bin and obj folders..." -ForegroundColor Yellow
try {
    Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ bin and obj folders removed successfully" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Some bin/obj folders could not be removed (might be in use): $_" -ForegroundColor Yellow
}

# Step 3: Clean solution
Write-Host "Step 3: Cleaning solution..." -ForegroundColor Yellow
try {
    dotnet clean
    Write-Host "✅ Solution cleaned successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to clean solution: $_" -ForegroundColor Red
}

# Step 4: Restore packages with force and no cache
Write-Host "Step 4: Restoring packages with force..." -ForegroundColor Yellow
try {
    dotnet restore --force --no-cache --verbosity normal
    Write-Host "✅ Packages restored successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to restore packages: $_" -ForegroundColor Red
    Write-Host "Trying alternative restore methods..." -ForegroundColor Yellow
    
    # Try restoring individual projects
    $projects = @(
        "src\Shared\InventoryPro.Shared\InventoryPro.Shared.csproj",
        "src\Services\InventoryPro.ProductService\InventoryPro.ProductService.csproj",
        "src\Services\InventoryPro.SalesService\InventoryPro.SalesService.csproj",
        "src\Services\InventoryPro.AuthService\InventoryPro.AuthService.csproj",
        "src\Services\InventoryPro.ReportService\InventoryPro.ReportService.csproj",
        "src\Gateway\InventoryPro.Gateway\InventoryPro.Gateway.csproj",
        "src\Clients\InventoryPro.WinForms\InventoryPro.WinForms.csproj"
    )
    
    foreach ($project in $projects) {
        if (Test-Path $project) {
            Write-Host "Restoring $project..." -ForegroundColor Cyan
            try {
                dotnet restore $project --force --no-cache
                Write-Host "✅ $project restored" -ForegroundColor Green
            } catch {
                Write-Host "❌ Failed to restore $project" -ForegroundColor Red
            }
        }
    }
}

# Step 5: Try building
Write-Host "Step 5: Testing build..." -ForegroundColor Yellow
try {
    dotnet build --no-restore
    Write-Host "🎉 Build successful! NuGet issues resolved." -ForegroundColor Green
} catch {
    Write-Host "❌ Build failed. Additional steps may be needed." -ForegroundColor Red
    Write-Host "Try running 'Update-Package -Reinstall' in Visual Studio Package Manager Console" -ForegroundColor Yellow
}

Write-Host "🏁 NuGet fix script completed." -ForegroundColor Cyan
Write-Host "If issues persist, try opening the solution in Visual Studio and use:" -ForegroundColor White
Write-Host "Tools → NuGet Package Manager → Package Manager Console" -ForegroundColor White
Write-Host "Then run: Update-Package -Reinstall" -ForegroundColor White