# Test Login for basharadmin
# اختبار تسجيل الدخول

$loginBody = @{
    username = "basharadmin"
    password = "admin123"
} | ConvertTo-Json

Write-Host "Testing login for basharadmin..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5011/api/Users/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody
    
    if ($response.success) {
        Write-Host "`n✅ Login successful!" -ForegroundColor Green
        Write-Host "User ID: $($response.user.id)" -ForegroundColor Cyan
        Write-Host "Username: $($response.user.username)" -ForegroundColor Cyan
        Write-Host "Token: $($response.token.Substring(0, 50))..." -ForegroundColor Gray
        
        # Test getting user permissions
        Write-Host "`nFetching user permissions..." -ForegroundColor Yellow
        $userId = $response.user.id
        $permsResponse = Invoke-RestMethod -Uri "http://localhost:5011/api/Users/$userId/permissions" -Method GET
        
        Write-Host "✅ Total Permissions: $($permsResponse.allPermissions.Count)" -ForegroundColor Green
        Write-Host "`nFirst 10 permissions:" -ForegroundColor Cyan
        $permsResponse.allPermissions | Select-Object -First 10 | ForEach-Object { Write-Host "  - $_" }
    }
    else {
        Write-Host "`n❌ Login failed: $($response.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
}
