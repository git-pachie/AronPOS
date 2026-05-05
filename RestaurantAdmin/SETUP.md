# Setup Instructions

## Prerequisites

1. **Install .NET 9 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Verify installation: `dotnet --version` (should show 9.x.x)

2. **SQL Server**
   - Option A: SQL Server LocalDB (comes with Visual Studio)
   - Option B: SQL Server Express (free): https://www.microsoft.com/sql-server/sql-server-downloads
   - Option C: Full SQL Server

## Installation Steps

### Step 1: Navigate to project folder
```bash
cd RestaurantAdmin
```

### Step 2: Restore NuGet packages
```bash
dotnet restore
```

### Step 3: Update connection string (if needed)

Edit `appsettings.json` and update the connection string:

**For LocalDB (default):**
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RestaurantAdminDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

**For SQL Server Express:**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=RestaurantAdminDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

**For SQL Server with credentials:**
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=RestaurantAdminDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Step 4: Run the application
```bash
dotnet run
```

The application will:
- ✅ Create the database automatically
- ✅ Apply all migrations
- ✅ Seed default roles (Admin, Manager, Staff)
- ✅ Create default admin account

### Step 5: Access the application

Open your browser and navigate to:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### Step 6: Login

Use these default credentials:
- **Email**: `admin@restaurant.com`
- **Password**: `Admin@123456`

⚠️ **IMPORTANT**: Change the admin password immediately after first login!

## Troubleshooting

### Database connection issues

**Error: "Cannot open database"**
- Ensure SQL Server is running
- Verify connection string is correct
- Check Windows Authentication or SQL Authentication settings

**Error: "Login failed for user"**
- Verify SQL Server authentication mode
- Check username/password in connection string
- Ensure user has database creation permissions

### Port already in use

If ports 5000/5001 are in use, edit `Properties/launchSettings.json` or run:
```bash
dotnet run --urls "http://localhost:5050;https://localhost:5051"
```

### Migration issues

To manually apply migrations:
```bash
dotnet ef database update
```

To recreate the database:
```bash
dotnet ef database drop
dotnet ef database update
```

## Development Tools (Optional)

- **Visual Studio 2022** (Community, Professional, or Enterprise)
- **Visual Studio Code** with C# extension
- **JetBrains Rider**
- **SQL Server Management Studio (SSMS)** for database management

## Next Steps

1. Change the default admin password
2. Create additional users
3. Set up menu categories
4. Add menu items
5. Customize the application as needed

## Production Deployment

Before deploying to production:

1. Update `appsettings.json` with production connection string
2. Change admin seed credentials
3. Set `ASPNETCORE_ENVIRONMENT=Production`
4. Enable HTTPS
5. Configure proper logging
6. Set up database backups
7. Review security settings

## Support

For issues or questions, refer to:
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [ASP.NET Identity Documentation](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
