# ============================================================
# Setup Permissions System - Step by Step
# إعداد نظام الصلاحيات - خطوة بخطوة
# ============================================================

# Database connection settings
$DB_HOST = "localhost"
$DB_PORT = "5432"
$DB_NAME = "sass_inventory_db"
$DB_USER = "postgres"
$DB_PASSWORD = "admin"

# Set PostgreSQL password environment variable
$env:PGPASSWORD = $DB_PASSWORD

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        إعداد نظام الصلاحيات - Permissions Setup          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "ℹ️  Database: $DB_NAME" -ForegroundColor Cyan
Write-Host "ℹ️  Host: ${DB_HOST}:${DB_PORT}" -ForegroundColor Cyan
Write-Host "ℹ️  User: $DB_USER`n" -ForegroundColor Cyan

# Check if psql is available
Write-Host "Checking PostgreSQL client..." -ForegroundColor Yellow
try {
    $psqlVersion = & psql --version 2>&1
    Write-Host "✅ PostgreSQL client found: $psqlVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ PostgreSQL client (psql) not found!" -ForegroundColor Red
    Write-Host "ℹ️  Please install PostgreSQL client or add it to PATH" -ForegroundColor Cyan
    exit 1
}

# Test database connection
Write-Host "`nTesting database connection..." -ForegroundColor Yellow
try {
    $testResult = & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT version();" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Cannot connect to database!" -ForegroundColor Red
        Write-Host $testResult -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Error connecting to database: $_" -ForegroundColor Red
    exit 1
}

# ============================================================
# Step 1: Check Current State
# ============================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STEP 1: Checking current database state" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

$checkScript = ".\Scripts\check-permissions.sql"
if (Test-Path $checkScript) {
    Write-Host "ℹ️  Executing: $checkScript" -ForegroundColor Cyan
    & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $checkScript
    Write-Host "`n✅ Current state checked" -ForegroundColor Green
} else {
    Write-Host "⚠️  File not found: $checkScript" -ForegroundColor Yellow
}

Write-Host "`nPress any key to continue to Step 2..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# ============================================================
# Step 2: Seed Permissions
# ============================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STEP 2: Adding all permissions" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "ℹ️  This will add 46 permissions across 11 categories" -ForegroundColor Cyan
Write-Host "ℹ️  Safe to run multiple times (uses ON CONFLICT DO NOTHING)`n" -ForegroundColor Cyan

$seedPermissionsScript = ".\Scripts\seed-permissions.sql"
if (Test-Path $seedPermissionsScript) {
    Write-Host "ℹ️  Executing: $seedPermissionsScript" -ForegroundColor Cyan
    & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $seedPermissionsScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Permissions added successfully" -ForegroundColor Green
        
        # Verify
        Write-Host "`nℹ️  Verifying permissions count..." -ForegroundColor Cyan
        & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) as total_permissions FROM permissions;"
    } else {
        Write-Host "`n❌ Failed to seed permissions!" -ForegroundColor Red
        Write-Host "Do you want to continue anyway? (y/n): " -NoNewline -ForegroundColor Yellow
        $response = Read-Host
        if ($response -ne 'y') {
            exit 1
        }
    }
} else {
    Write-Host "❌ File not found: $seedPermissionsScript" -ForegroundColor Red
    exit 1
}

Write-Host "`nPress any key to continue to Step 3..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# ============================================================
# Step 3: Seed Default Roles
# ============================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STEP 3: Creating default roles" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "ℹ️  This will create 5 default roles:" -ForegroundColor Cyan
Write-Host "  1. Admin (46 permissions)" -ForegroundColor White
Write-Host "  2. Manager (32 permissions)" -ForegroundColor White
Write-Host "  3. Cashier (7 permissions)" -ForegroundColor White
Write-Host "  4. Accountant (11 permissions)" -ForegroundColor White
Write-Host "  5. Inventory Manager (14 permissions)`n" -ForegroundColor White

$seedRolesScript = ".\Scripts\seed-default-roles.sql"
if (Test-Path $seedRolesScript) {
    Write-Host "ℹ️  Executing: $seedRolesScript" -ForegroundColor Cyan
    & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $seedRolesScript
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Default roles created successfully" -ForegroundColor Green
        
        # Verify
        Write-Host "`nℹ️  Verifying roles..." -ForegroundColor Cyan
        & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT r.role_name, COUNT(rp.permission_id) as permission_count FROM roles r LEFT JOIN role_permissions rp ON r.id = rp.role_id GROUP BY r.role_name ORDER BY r.role_name;"
    } else {
        Write-Host "`n❌ Failed to seed roles!" -ForegroundColor Red
        Write-Host "Do you want to continue anyway? (y/n): " -NoNewline -ForegroundColor Yellow
        $response = Read-Host
        if ($response -ne 'y') {
            exit 1
        }
    }
} else {
    Write-Host "❌ File not found: $seedRolesScript" -ForegroundColor Red
    exit 1
}

Write-Host "`nPress any key to continue to Step 4..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# ============================================================
# Step 4: Create Admin User (Optional)
# ============================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "STEP 4: Creating Admin user (OPTIONAL)" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "⚠️  WARNING: This will create an admin user with default password!" -ForegroundColor Yellow
Write-Host "   Username: admin" -ForegroundColor White
Write-Host "   Password: admin123`n" -ForegroundColor White

Write-Host "Do you want to create this user? (y/n): " -NoNewline -ForegroundColor Yellow
$response = Read-Host

if ($response -eq 'y') {
    Write-Host "`nℹ️  Creating admin user via API..." -ForegroundColor Cyan
    Write-Host "ℹ️  Make sure the API is running on http://localhost:5011`n" -ForegroundColor Cyan
    
    Write-Host "Is the API running? (y/n): " -NoNewline -ForegroundColor Yellow
    $apiRunning = Read-Host
    
    if ($apiRunning -eq 'y') {
        $createAdminScript = ".\Scripts\create-admin.ps1"
        if (Test-Path $createAdminScript) {
            & $createAdminScript
        } else {
            Write-Host "❌ File not found: $createAdminScript" -ForegroundColor Red
        }
    } else {
        Write-Host "ℹ️  Skipping admin user creation" -ForegroundColor Cyan
        Write-Host "ℹ️  You can run create-admin.ps1 later when API is running" -ForegroundColor Cyan
    }
} else {
    Write-Host "ℹ️  Skipping admin user creation" -ForegroundColor Cyan
}

# ============================================================
# Final Verification
# ============================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "FINAL VERIFICATION" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "ℹ️  Checking final state...`n" -ForegroundColor Cyan

Write-Host "Total Permissions:" -ForegroundColor White
& psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) as total_permissions FROM permissions;"

Write-Host "`nTotal Roles:" -ForegroundColor White
& psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) as total_roles FROM roles;"

Write-Host "`nRoles Summary:" -ForegroundColor White
& psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT r.role_name, COUNT(rp.permission_id) as permission_count FROM roles r LEFT JOIN role_permissions rp ON r.id = rp.role_id GROUP BY r.role_name ORDER BY r.role_name;"

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ✅ Setup Completed Successfully!              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "ℹ️  Next Steps:" -ForegroundColor Cyan
Write-Host "1. Start your API: dotnet run" -ForegroundColor White
Write-Host "2. Run create-admin.ps1 to create admin user (if not done)" -ForegroundColor White
Write-Host "3. Test login with admin/admin123" -ForegroundColor White
Write-Host "4. Update task.md to mark tasks as complete`n" -ForegroundColor White

# Clean up
$env:PGPASSWORD = $null
