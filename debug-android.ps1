# Debug script for Android deployment
Write-Host "=== Jindal Android Debug Script ===" -ForegroundColor Green

# Step 1: Check if device is connected
Write-Host "Checking connected Android devices..." -ForegroundColor Yellow
$devices = dotnet build -t:GetAndroidDevices -f net9.0-android

if ($devices -match "No devices found") {
    Write-Host "ERROR: No Android devices connected!" -ForegroundColor Red
    Write-Host "Please connect your Android device via USB and enable USB debugging" -ForegroundColor Red
    exit 1
}

# Step 2: Clean and rebuild
Write-Host "Cleaning project..." -ForegroundColor Yellow
dotnet clean -f net9.0-android

Write-Host "Building project..." -ForegroundColor Yellow
dotnet build -f net9.0-android

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Deploy to device
Write-Host "Deploying to Android device..." -ForegroundColor Yellow
dotnet build -t:Install -f net9.0-android

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: App deployed to Android device" -ForegroundColor Green
Write-Host ""
Write-Host "To view logs, run:" -ForegroundColor Yellow
Write-Host "  dotnet build -t:AndroidGetLogcat -f net9.0-android" -ForegroundColor Cyan
Write-Host ""
Write-Host "To clear logs and start fresh:" -ForegroundColor Yellow
Write-Host "  dotnet build -t:AndroidClearLogcat -f net9.0-android" -ForegroundColor Cyan
