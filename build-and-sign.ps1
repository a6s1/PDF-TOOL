# PDF Tools Pro - Build and Sign Script
# This script builds the application and signs it with a self-signed certificate

Write-Host "=== PDF Tools Pro Build & Sign ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build
Write-Host "Step 1: Building application..." -ForegroundColor Yellow
$publishPath = ".\Publish"

dotnet publish .\PDFToolsPro\PDFToolsPro.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $publishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed!" -ForegroundColor Green
Write-Host ""

# Step 2: Sign
Write-Host "Step 2: Signing application..." -ForegroundColor Yellow
& .\sign.ps1 -ExePath "$publishPath\PDFToolsPro.exe"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Signing failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Copy to Desktop
Write-Host ""
Write-Host "Step 3: Copying to Desktop..." -ForegroundColor Yellow
Copy-Item "$publishPath\PDFToolsPro.exe" "$env:USERPROFILE\Desktop\PDFToolsPro.exe" -Force
Write-Host "Copied to Desktop!" -ForegroundColor Green

Write-Host ""
Write-Host "=== All Done! ===" -ForegroundColor Cyan
Write-Host "Signed EXE is at: $env:USERPROFILE\Desktop\PDFToolsPro.exe" -ForegroundColor White

