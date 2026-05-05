# Restaurant Admin — .NET 9 MVC

A full-featured admin panel for restaurant management built with ASP.NET Core MVC 9, ASP.NET Identity, and MS SQL Server.

## Features

| Feature | Details |
|---|---|
| **User Management** | Create users, edit profile, delete accounts |
| **Role Assignment** | Assign/remove roles (Admin, Manager, Staff) per user |
| **Account Suspension** | Suspend with reason, unsuspend, forces immediate sign-out |
| **Password Reset** | Admin resets any user's password directly |
| **Menu Categories** | Create, edit, delete categories with active/inactive toggle |
| **Menu Items** | Create, edit, delete items with availability toggle |
| **Price Updates** | Update item prices with full audit trail |
| **Price History** | View complete price change history per item |
| **Dashboard** | Summary stats for users, items, and categories |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, Express, or full)

## Getting Started

### 1. Configure the connection string

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RestaurantAdminDb;Trusted_Connection=True;"
}
```

For a full SQL Server instance:
```
Server=YOUR_SERVER;Database=RestaurantAdminDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

### 2. Run the application

```bash
cd RestaurantAdmin
dotnet run
```

The app will automatically:
- Apply EF Core migrations (create the database)
- Seed the default roles: Admin, Manager, Staff
- Seed the default admin account

### 3. Login

| Field | Value |
|---|---|
| Email | `admin@restaurant.com` |
| Password | `Admin@123456` |

> Change the seed credentials in `appsettings.json` before deploying.

## Project Structure

```
RestaurantAdmin/
├── Controllers/
│   ├── AccountController.cs      # Login / Logout
│   ├── DashboardController.cs    # Dashboard stats
│   ├── UsersController.cs        # Full user management
│   └── MenuController.cs         # Categories + items + pricing
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core DbContext
│   └── DbSeeder.cs               # Roles + admin seed
├── Migrations/                   # EF Core migrations
├── Models/
│   ├── ApplicationUser.cs        # Extended Identity user
│   ├── MenuCategory.cs
│   ├── MenuItem.cs
│   └── MenuItemPriceHistory.cs
├── ViewModels/                   # Strongly-typed view models
├── Views/                        # Razor views
│   ├── Account/
│   ├── Dashboard/
│   ├── Menu/
│   ├── Users/
│   └── Shared/_Layout.cshtml
└── wwwroot/css/site.css
```

## Roles

- **Admin** — Full access to all features
- **Manager** — Access to menu management
- **Staff** — No admin panel access (extend as needed)

## Security Notes

- Suspended users are immediately blocked from logging in
- Suspending a user invalidates their security stamp (forces sign-out)
- Admins cannot suspend or delete their own account
- Passwords require minimum 6 characters with at least one digit
- Account lockout after 5 failed attempts (15-minute lockout)
- All forms use CSRF anti-forgery tokens
