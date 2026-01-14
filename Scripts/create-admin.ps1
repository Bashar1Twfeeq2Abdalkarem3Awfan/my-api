# Script to create Admin Person and User

Write-Host "Creating Admin Person and User..." -ForegroundColor Cyan

# Step 1: Create Admin Person
Write-Host "`nStep 1: Creating Admin Person..." -ForegroundColor Yellow

$personBody = @{
    firstName = "Bashar"
    secondName = "Admin"
    thirdWithLastname = "System"
    email = "bashar@system.com"
    phoneNumber = "0000000000"
    address = "System"
    personType = "Employee"
    isActive = $true
} | ConvertTo-Json

try {
    $personResponse = Invoke-RestMethod -Uri "http://localhost:5011/api/Person" `
        -Method POST `
        -ContentType "application/json" `
        -Body $personBody
    
    $personId = $personResponse.id
    Write-Host "Admin Person created successfully! ID: $personId" -ForegroundColor Green
}
catch {
    Write-Host "Error creating Person. Assuming Person already exists..." -ForegroundColor Yellow
    
    $persons = Invoke-RestMethod -Uri "http://localhost:5011/api/Person" -Method GET
    if ($persons.Count -gt 0) {
        $personId = $persons[0].id
        Write-Host "Using existing Person ID: $personId" -ForegroundColor Green
    }
    else {
        Write-Host "No persons found in database!" -ForegroundColor Red
        exit 1
    }
}

# Step 2: Create Admin User
Write-Host "`nStep 2: Creating Admin User..." -ForegroundColor Yellow

$userBody = @{
    personId = $personId
    username = "basharadmin"
    password = "admin123"
    loginName = "Bashar Administrator"
} | ConvertTo-Json

try {
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5011/api/Users" `
        -Method POST `
        -ContentType "application/json" `
        -Body $userBody
    
    $userId = $userResponse.id
    Write-Host "Admin User created successfully! ID: $userId" -ForegroundColor Green
    Write-Host "   Username: basharadmin" -ForegroundColor Cyan
    Write-Host "   Password: admin123" -ForegroundColor Cyan
}
catch {
    Write-Host "Error creating User: $_" -ForegroundColor Red
    Write-Host "User may already exist." -ForegroundColor Yellow
    
    # Try to get existing admin user
    $users = Invoke-RestMethod -Uri "http://localhost:5011/api/Users" -Method GET
    $adminUser = $users | Where-Object { $_.username -eq "basharadmin" } | Select-Object -First 1
    if ($adminUser) {
        $userId = $adminUser.id
        Write-Host "Using existing basharadmin user ID: $userId" -ForegroundColor Green
    }
}

# Step 3: Assign Manager Role
Write-Host "`nStep 3: Assigning Manager role..." -ForegroundColor Yellow

try {
    $roles = Invoke-RestMethod -Uri "http://localhost:5011/api/Role" -Method GET
    $managerRole = $roles | Where-Object { $_.roleName -like "*مدير*" -or $_.roleName -like "*Manager*" } | Select-Object -First 1
    
    if ($managerRole) {
        $roleId = $managerRole.id
        Write-Host "Found Manager role with ID: $roleId" -ForegroundColor Green
        
        $userRoleBody = @{
            userId = $userId
            roleId = $roleId
        } | ConvertTo-Json
        
        try {
            $userRoleResponse = Invoke-RestMethod -Uri "http://localhost:5011/api/UserRole" `
                -Method POST `
                -ContentType "application/json" `
                -Body $userRoleBody
            
            Write-Host "Manager role assigned successfully!" -ForegroundColor Green
        }
        catch {
            Write-Host "Role may already be assigned: $_" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Manager role not found!" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error assigning role: $_" -ForegroundColor Yellow
}

# Step 4: Test Login
Write-Host "`nStep 4: Testing login..." -ForegroundColor Yellow

$loginBody = @{
    username = "basharadmin"
    password = "admin123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5011/api/Users/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody
    
    if ($loginResponse.success) {
        Write-Host "Login successful!" -ForegroundColor Green
        Write-Host "   User ID: $($loginResponse.user.id)" -ForegroundColor Cyan
        Write-Host "   Username: $($loginResponse.user.username)" -ForegroundColor Cyan
        Write-Host "   Person: $($loginResponse.user.person.firstName) $($loginResponse.user.person.secondName)" -ForegroundColor Cyan
        Write-Host "   Last Login: $($loginResponse.user.lastLogin)" -ForegroundColor Cyan
    }
    else {
        Write-Host "Login failed: $($loginResponse.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error testing login: $_" -ForegroundColor Red
}

Write-Host "`nAdmin setup complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Login Credentials:" -ForegroundColor Yellow
Write-Host "  Username: basharadmin" -ForegroundColor White
Write-Host "  Password: admin123" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
