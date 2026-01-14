# Simple Setup Script - No Special Characters
# تشغيل Scripts قاعدة البيانات

$env:PGPASSWORD = "admin"

Write-Host "Step 1: Checking current state..." -ForegroundColor Yellow
psql -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\check-permissions.sql

Write-Host "`nPress Enter to continue..." -ForegroundColor Yellow
Read-Host

Write-Host "`nStep 2: Adding permissions..." -ForegroundColor Yellow
psql -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\seed-permissions.sql

Write-Host "`nPress Enter to continue..." -ForegroundColor Yellow
Read-Host

Write-Host "`nStep 3: Creating roles..." -ForegroundColor Yellow
psql -h localhost -p 5432 -U postgres -d sass_inventory_db -f .\seed-default-roles.sql

Write-Host "`nDone!" -ForegroundColor Green

$env:PGPASSWORD = $null
