# PDF Tools Pro - Code Signing Script
# This script signs the PDFToolsPro.exe with a self-signed certificate

param(
    [string]$ExePath = ".\Publish\PDFToolsPro.exe",
    [string]$CertSubject = "CN=PDF Tools Pro, O=A6S1, C=SA"
)

Write-Host "=== PDF Tools Pro Signing Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if certificate exists
$cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object {$_.Subject -eq $CertSubject}

if (-not $cert) {
    Write-Host "Certificate not found. Creating new self-signed certificate..." -ForegroundColor Yellow
    $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject $CertSubject -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddYears(5) -KeyUsage DigitalSignature -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")
    Write-Host "Certificate created with thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
}
else {
    Write-Host "Using existing certificate: $($cert.Thumbprint)" -ForegroundColor Green
}

# Check if EXE exists
if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: File not found: $ExePath" -ForegroundColor Red
    Write-Host "Please build the project first or specify the correct path." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Signing: $ExePath" -ForegroundColor Cyan

# Sign the file
try {
    $result = Set-AuthenticodeSignature -FilePath $ExePath -Certificate $cert -HashAlgorithm SHA256
    
    if ($result.Status -eq "Valid" -or $result.Status -eq "UnknownError") {
        Write-Host ""
        Write-Host "File signed successfully!" -ForegroundColor Green
        Write-Host "Status: $($result.Status)" -ForegroundColor Yellow
        Write-Host "Note: 'UnknownError' is normal for self-signed certificates" -ForegroundColor Yellow
        Write-Host ""
        
        # Verify signature
        $verify = Get-AuthenticodeSignature $ExePath
        Write-Host "Verification:" -ForegroundColor Cyan
        Write-Host "  Signer: $($verify.SignerCertificate.Subject)" -ForegroundColor White
        Write-Host "  Valid From: $($verify.SignerCertificate.NotBefore)" -ForegroundColor White
        Write-Host "  Valid To: $($verify.SignerCertificate.NotAfter)" -ForegroundColor White
    }
    else {
        Write-Host "Signing failed with status: $($result.Status)" -ForegroundColor Red
        Write-Host "Message: $($result.StatusMessage)" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan

