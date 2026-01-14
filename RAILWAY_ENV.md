# Environment Variables for Railway Deployment

## Required Environment Variables

### Database Configuration
```
DATABASE_URL=postgresql://username:password@host:port/database
```
Or individual components:
```
DB_HOST=your-postgres-host
DB_PORT=5432
DB_NAME=sass_inventory_db
DB_USER=your-username
DB_PASSWORD=your-password
```

### JWT Configuration
```
JWT_SECRET_KEY=your-super-secret-256-bit-key-minimum-32-characters-required-change-this
JWT_ISSUER=MyAPIv3
JWT_AUDIENCE=sass_bt_mobile
JWT_EXPIRY_MINUTES=1440
```

### CORS Configuration
```
ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```
For development/testing, you can use:
```
ALLOWED_ORIGINS=*
```

### Port (Automatically set by Railway)
```
PORT=5000
```

### ASP.NET Core Environment
```
ASPNETCORE_ENVIRONMENT=Production
```

## Railway Setup Instructions

1. **Create a new project on Railway**
2. **Add PostgreSQL database** (Railway will auto-configure DATABASE_URL)
3. **Set environment variables** in Railway dashboard:
   - `JWT_SECRET_KEY` - Generate a strong random key (minimum 32 characters)
   - `ALLOWED_ORIGINS` - Your frontend domain(s), comma-separated
   - `ASPNETCORE_ENVIRONMENT` - Set to `Production`

4. **Connection String Format**:
   Railway provides `DATABASE_URL` in this format:
   ```
   postgresql://user:password@host:port/database
   ```
   
   You need to convert it to Npgsql format in Railway variables:
   ```
   ConnectionStrings__DefaultConnection=Host=host;Port=port;Database=database;Username=user;Password=password
   ```

## Example Railway Environment Variables

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production

# Database (Railway auto-provides DATABASE_URL, but we need Npgsql format)
# Extract values from DATABASE_URL and set:
ConnectionStrings__DefaultConnection=Host=containers-us-west-123.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxxxx

# JWT
JWT_SECRET_KEY=my-super-secret-production-key-with-at-least-32-characters-for-security
Jwt__SecretKey=my-super-secret-production-key-with-at-least-32-characters-for-security

# CORS (replace with your actual frontend domain)
ALLOWED_ORIGINS=https://yourapp.com,https://www.yourapp.com

# Port (Railway sets this automatically)
PORT=5000
```

## Important Notes

1. **Never commit sensitive keys to Git**
2. **Use Railway's built-in PostgreSQL** for easy setup
3. **Generate a strong JWT secret** using a password generator
4. **Update ALLOWED_ORIGINS** with your actual frontend domain before going live
5. **Railway automatically sets PORT** - don't override it unless necessary

## Testing Locally with Production Settings

Create a `.env` file (add to .gitignore):
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=sass_inventory_db;Username=postgres;Password=admin
Jwt__SecretKey=local-development-secret-key-minimum-32-characters
ALLOWED_ORIGINS=*
```
