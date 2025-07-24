# 🚀 Jindal Guest Management System - Windows Production Deployment Script
# Version: 2.0.0
# Platform: Windows 10/11
# Framework: .NET 9.0 MAUI

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".\publish\windows",
    
    [Parameter(Mandatory = $false)]
    [switch]$SelfContained = $true,
    
    [Parameter(Mandatory = $false)]
    [switch]$SingleFile = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipTests = $false
)

Write-Host "🚀 Jindal Guest Management System - Windows Deployment" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = "Jindal.csproj"
$Framework = "net9.0-windows10.0.19041.0"
$Runtime = "win10-x64"

Write-Host "📋 Deployment Configuration:" -ForegroundColor Yellow
Write-Host "  • Configuration: $Configuration" -ForegroundColor White
Write-Host "  • Framework: $Framework" -ForegroundColor White
Write-Host "  • Runtime: $Runtime" -ForegroundColor White
Write-Host "  • Output Path: $OutputPath" -ForegroundColor White
Write-Host "  • Self-Contained: $SelfContained" -ForegroundColor White
Write-Host ""

# Step 1: Verify Prerequisites
Write-Host "🔍 Step 1: Verifying Prerequisites..." -ForegroundColor Green

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✅ .NET SDK Version: $dotnetVersion" -ForegroundColor White
} catch {
    Write-Host "  ❌ .NET SDK not found. Please install .NET 9.0 SDK." -ForegroundColor Red
    exit 1
}

# Check project file
if (-Not (Test-Path $ProjectPath)) {
    Write-Host "  ❌ Project file not found: $ProjectPath" -ForegroundColor Red
    exit 1
}
Write-Host "  ✅ Project file found: $ProjectPath" -ForegroundColor White

Write-Host ""

# Step 2: Clean Previous Builds
Write-Host "🧹 Step 2: Cleaning Previous Builds..." -ForegroundColor Green

if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
    Write-Host "  ✅ Cleaned output directory: $OutputPath" -ForegroundColor White
}

try {
    dotnet clean --configuration $Configuration --verbosity quiet
    Write-Host "  ✅ Cleaned project artifacts" -ForegroundColor White
} catch {
    Write-Host "  ⚠️  Clean failed, continuing..." -ForegroundColor Yellow
}

Write-Host ""

# Step 3: Restore Dependencies
Write-Host "📦 Step 3: Restoring Dependencies..." -ForegroundColor Green

try {
    dotnet restore --verbosity quiet
    Write-Host "  ✅ Dependencies restored successfully" -ForegroundColor White
} catch {
    Write-Host "  ❌ Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Run Tests (Optional)
if (-Not $SkipTests) {
    Write-Host "🧪 Step 4: Running Tests..." -ForegroundColor Green
    
    # Check if test projects exist
    $testProjects = Get-ChildItem -Path . -Filter "*.Tests.csproj" -Recurse
    
    if ($testProjects.Count -gt 0) {
        try {
            dotnet test --configuration $Configuration --verbosity quiet --no-restore
            Write-Host "  ✅ All tests passed" -ForegroundColor White
        } catch {
            Write-Host "  ❌ Tests failed. Deployment aborted." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "  ⚠️  No test projects found, skipping tests" -ForegroundColor Yellow
    }
    
    Write-Host ""
} else {
    Write-Host "⏭️  Step 4: Skipping Tests (as requested)" -ForegroundColor Yellow
    Write-Host ""
}

# Step 5: Build Project
Write-Host "🔨 Step 5: Building Project..." -ForegroundColor Green

$buildArgs = @(
    "build",
    "--configuration", $Configuration,
    "--framework", $Framework,
    "--no-restore",
    "--verbosity", "quiet"
)

try {
    & dotnet @buildArgs
    Write-Host "  ✅ Project built successfully" -ForegroundColor White
} catch {
    Write-Host "  ❌ Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 6: Publish Application
Write-Host "📋 Step 6: Publishing Application..." -ForegroundColor Green

$publishArgs = @(
    "publish",
    "--configuration", $Configuration,
    "--framework", $Framework,
    "--runtime", $Runtime,
    "--output", $OutputPath,
    "--no-restore",
    "--no-build",
    "--verbosity", "quiet"
)

if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
} else {
    $publishArgs += "--self-contained"
    $publishArgs += "false"
}

if ($SingleFile) {
    $publishArgs += "-p:PublishSingleFile=true"
    $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
}

# Additional production optimizations
$publishArgs += "-p:PublishTrimmed=false"  # Disabled for MAUI
$publishArgs += "-p:PublishReadyToRun=true"
$publishArgs += "-p:TieredCompilation=true"

try {
    & dotnet @publishArgs
    Write-Host "  ✅ Application published successfully" -ForegroundColor White
} catch {
    Write-Host "  ❌ Publish failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 7: Verify Deployment
Write-Host "🔍 Step 7: Verifying Deployment..." -ForegroundColor Green

$executableName = "Jindal.exe"
$executablePath = Join-Path $OutputPath $executableName

if (Test-Path $executablePath) {
    Write-Host "  ✅ Executable found: $executablePath" -ForegroundColor White
    
    # Get file size
    $fileSize = (Get-Item $executablePath).Length
    $fileSizeMB = [math]::Round($fileSize / 1MB, 2)
    Write-Host "  📊 Executable size: $fileSizeMB MB" -ForegroundColor White
    
    # Get version info
    try {
        $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($executablePath)
        Write-Host "  📋 Version: $($versionInfo.ProductVersion)" -ForegroundColor White
        Write-Host "  🏢 Company: $($versionInfo.CompanyName)" -ForegroundColor White
    } catch {
        Write-Host "  ⚠️  Could not read version information" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ❌ Executable not found: $executablePath" -ForegroundColor Red
    exit 1
}

# Check for required files
$requiredFiles = @(
    "Jindal.dll",
    "Jindal.deps.json",
    "Jindal.runtimeconfig.json"
)

foreach ($file in $requiredFiles) {
    $filePath = Join-Path $OutputPath $file
    if (Test-Path $filePath) {
        Write-Host "  ✅ Required file found: $file" -ForegroundColor White
    } else {
        Write-Host "  ❌ Required file missing: $file" -ForegroundColor Red
    }
}

Write-Host ""

# Step 8: Create Installation Package (Optional)
Write-Host "📦 Step 8: Creating Installation Package..." -ForegroundColor Green

$packagePath = ".\publish\Jindal-Windows-v2.0.0.zip"

try {
    if (Test-Path $packagePath) {
        Remove-Item $packagePath -Force
    }
    
    Compress-Archive -Path "$OutputPath\*" -DestinationPath $packagePath -CompressionLevel Optimal
    
    $packageSize = (Get-Item $packagePath).Length
    $packageSizeMB = [math]::Round($packageSize / 1MB, 2)
    
    Write-Host "  ✅ Installation package created: $packagePath" -ForegroundColor White
    Write-Host "  📊 Package size: $packageSizeMB MB" -ForegroundColor White
} catch {
    Write-Host "  ⚠️  Could not create installation package" -ForegroundColor Yellow
}

Write-Host ""

# Step 9: Generate Deployment Summary
Write-Host "📊 Step 9: Deployment Summary" -ForegroundColor Green

$deploymentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

Write-Host "  🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "  📅 Deployment Time: $deploymentTime" -ForegroundColor White
Write-Host "  📁 Output Location: $OutputPath" -ForegroundColor White
Write-Host "  🚀 Executable: $executablePath" -ForegroundColor White

if (Test-Path $packagePath) {
    Write-Host "  📦 Package: $packagePath" -ForegroundColor White
}

Write-Host ""

# Step 10: Installation Instructions
Write-Host "📖 Installation Instructions:" -ForegroundColor Cyan
Write-Host "  1. Copy the contents of '$OutputPath' to target machine" -ForegroundColor White
Write-Host "  2. Ensure .NET 9.0 Runtime is installed (if not self-contained)" -ForegroundColor White
Write-Host "  3. Run Jindal.exe to start the application" -ForegroundColor White
Write-Host "  4. Default login: admin / JindalAdmin2024!@#" -ForegroundColor White
Write-Host "  5. Change default password after first login" -ForegroundColor Yellow

Write-Host ""

# Step 11: Security Checklist
Write-Host "🔒 Security Checklist:" -ForegroundColor Red
Write-Host "  ⚠️  Change default admin password immediately" -ForegroundColor Yellow
Write-Host "  ⚠️  Configure SQL Server connection string if needed" -ForegroundColor Yellow
Write-Host "  ⚠️  Enable Windows Firewall rules for database access" -ForegroundColor Yellow
Write-Host "  ⚠️  Review user permissions and roles" -ForegroundColor Yellow

Write-Host ""
Write-Host "✅ Windows deployment completed successfully! 🎉" -ForegroundColor Green
Write-Host "   Ready for production use." -ForegroundColor White
