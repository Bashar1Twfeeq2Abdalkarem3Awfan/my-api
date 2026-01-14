# Railway Deployment Guide

## Pre-Deployment Checklist

✅ **Configuration Files Updated**
- `Program.cs` - Railway port binding, environment-based CORS, optional RateLimiting
- `appsettings.json` - Development defaults
- `appsettings.Production.json` - Production configuration (uses environment variables)

✅ **Files to Include**
- Ensure `appsettings.RateLimiting.json` is included in deployment (now optional)
- All `.csproj` dependencies are properly referenced

## Deployment Steps

### 1. Prepare Your Repository
```bash
# Make sure all changes are committed
git add .
git commit -m "Configure for Railway deployment"
git push
```

### 2. Create Railway Project
1. Go to [Railway.app](https://railway.app)
2. Click "New Project"
3. Choose "Deploy from GitHub repo"
4. Select your repository

### 3. Add PostgreSQL Database
1. In your Railway project, click "New"
2. Select "Database" → "PostgreSQL"
3. Railway will automatically create a `DATABASE_URL` variable

### 4. Configure Environment Variables

**Required Variables:**

```bash
# ASP.NET Environment
ASPNETCORE_ENVIRONMENT=Production

# Database Connection (convert Railway's DATABASE_URL to Npgsql format)
# Railway provides: postgresql://user:pass@host:port/db
# You need: Host=host;Port=port;Database=db;Username=user;Password=pass
ConnectionStrings__DefaultConnection=Host=YOUR_HOST;Port=5432;Database=railway;Username=postgres;Password=YOUR_PASSWORD

# JWT Configuration
Jwt__SecretKey=YOUR-SUPER-SECRET-KEY-MINIMUM-32-CHARACTERS-REQUIRED

# CORS (your frontend domain)
ALLOWED_ORIGINS=https://yourfrontend.com
```

**Optional Variables:**
```bash
Jwt__Issuer=MyAPIv3
Jwt__Audience=sass_bt_mobile
Jwt__ExpiryMinutes=1440
```

### 5. Deploy
Railway will automatically:
- Detect your .NET project
- Build the application
- Deploy to a public URL

### 6. Verify Deployment
After deployment, test your endpoints:
```bash
# Health check (if you have one)
curl https://your-app.railway.app/api/health

# Test login
curl -X POST https://your-app.railway.app/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"yourpassword"}'
```

## Important Notes

### Database Connection
Railway provides `DATABASE_URL` in PostgreSQL format, but Npgsql needs a different format:

**Railway provides:**
```
postgresql://postgres:password@containers-us-west-123.railway.app:5432/railway
```

**You need to set:**
```
ConnectionStrings__DefaultConnection=Host=containers-us-west-123.railway.app;Port=5432;Database=railway;Username=postgres;Password=password
```

### CORS Configuration
- **Development:** `ALLOWED_ORIGINS=*` (allows all origins)
- **Production:** `ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com`

### Port Configuration
- Railway automatically sets the `PORT` environment variable
- The app is configured to read from `PORT` or default to 5000
- **Do not manually set PORT** unless you have a specific reason

### HTTPS
- Railway handles HTTPS termination automatically
- The app is configured to NOT use `UseHttpsRedirection()` to avoid conflicts
- All external traffic is automatically HTTPS

## Troubleshooting

### App won't start
1. Check Railway logs for errors
2. Verify all required environment variables are set
3. Ensure `ConnectionStrings__DefaultConnection` is in correct Npgsql format

### Database connection fails
1. Verify PostgreSQL service is running in Railway
2. Check connection string format
3. Ensure database credentials are correct

### CORS errors
1. Add your frontend domain to `ALLOWED_ORIGINS`
2. Use comma-separated list for multiple domains
3. For testing, temporarily use `*`

### JWT authentication fails
1. Verify `Jwt__SecretKey` is set and matches between environments
2. Check token expiry settings
3. Ensure secret key is at least 32 characters

## Post-Deployment

### Database Migrations
If you need to run migrations:
```bash
# Railway CLI
railway run dotnet ef database update
```

Or connect to Railway PostgreSQL and run migrations manually.

### Monitor Logs
```bash
# Install Railway CLI
npm i -g @railway/cli

# Login
railway login

# View logs
railway logs
```

### Update Environment Variables
You can update environment variables anytime through:
1. Railway Dashboard → Your Project → Variables
2. Changes take effect after redeployment

## Security Recommendations

1. ✅ Use strong JWT secret (32+ characters, random)
2. ✅ Limit CORS to specific domains in production
3. ✅ Enable HSTS (already configured)
4. ✅ Keep dependencies updated
5. ✅ Use Railway's built-in PostgreSQL (automatic backups)
6. ✅ Never commit secrets to Git

## Need Help?

- Railway Docs: https://docs.railway.app
- Railway Discord: https://discord.gg/railway
- Check Railway logs for detailed error messages
