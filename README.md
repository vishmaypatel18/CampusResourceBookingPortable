# Campus Resource Booking System (Portable setup)

A beginner-friendly ASP.NET Core MVC college mini-project with Identity, MySQL, resource CRUD, booking requests, administrator approvals, search and dashboard.

## Portability plan

The project files are portable. `setup-portable-dotnet.cmd` downloads a local .NET 10 SDK into `.tools\dotnet`, so it does not require a system-wide SDK installation. MySQL Server is a separate database service: this project intentionally keeps MySQL to match the assignment. You can point the app at a local MySQL installation or a portable MySQL/MariaDB server you manage separately.

## Requirements

- Windows 10/11
- Internet for first-time SDK/NuGet restore and Bootstrap CDN assets
- MySQL Server 8.x or compatible MariaDB running locally
- A MySQL database and restricted user (instructions below)

## 1. Install the portable SDK

Double-click `setup-portable-dotnet.cmd`. It downloads the official Microsoft dotnet-install script and installs the .NET 10 SDK under this project folder.

Open a new terminal in this folder and run:

```powershell
$env:DOTNET_ROOT = "$PWD\.tools\dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:DOTNET_ROOT\tools;$env:PATH"
dotnet --info
```

No system-wide .NET SDK is needed. NuGet packages are restored into the normal user package cache by default; this is not required for the project to run from its folder. To keep the package cache local too:

```powershell
$env:NUGET_PACKAGES = "$PWD\.packages"
```

Run these commands in the same terminal session.

## 2. Prepare MySQL

In MySQL Workbench or the MySQL client:

```sql
CREATE DATABASE campus_resource_booking
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE USER 'campus_app'@'localhost' IDENTIFIED BY 'Choose-A-Strong-Local-Password';
GRANT ALL PRIVILEGES ON campus_resource_booking.* TO 'campus_app'@'localhost';
FLUSH PRIVILEGES;
```

The grant is scoped to this application's database. For a production deployment, use a more restrictive migration/runtime permission plan.

Set the connection string for the current PowerShell session (replace the password):

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=campus_resource_booking;User=campus_app;Password=Choose-A-Strong-Local-Password;"
```

## 3. Set up an administrator safely

Choose a strong password and set these environment variables in the same terminal before first run:

```powershell
$env:ADMIN_EMAIL = "admin@college.local"
$env:ADMIN_PASSWORD = "Use-A-Strong-Unique-Password-123"
```

On startup, the app creates the Admin role and this administrator if the account does not already exist. Never commit these values to Git. Clear them after startup if desired:

```powershell
Remove-Item Env:ADMIN_PASSWORD
Remove-Item Env:ADMIN_EMAIL
```

Normal registration creates a user account but does not grant the Admin role. All new accounts can browse and request bookings. Assign Student/Faculty roles manually or extend registration if your college requires role selection; never allow public registration to select Admin.

## 4. Run the application

From the folder containing `CampusResourceBooking.csproj`:

```powershell
dotnet restore
dotnet run
```

Open the localhost URL shown in the terminal. If HTTPS certificate prompts cause trouble on your own machine, use the HTTP URL printed by the app.

On first run, `EnsureCreatedAsync()` creates the Identity and application tables in an empty database. This avoids requiring a generated migration just to get the portable starter running. **Important:** `EnsureCreated` is for a fresh project database and does not manage schema changes over time. If your teacher requires migrations, after the first successful build we can switch this to migrations and generate the initial migration with `dotnet ef migrations add InitialCreate` followed by `dotnet ef database update`.

## 5. Add sample resources

After signing in as admin, choose **Add resource** and create some records:

| Name | Category | Location | Capacity |
|---|---|---|---:|
| Computer Lab A | Computer Lab | Block A, Floor 2 | 40 |
| Seminar Hall 1 | Seminar Hall | Main Building | 120 |
| Projector P-04 | Projector | IT Department Office | 1 |
| Project Lab 2 | Classroom | Block C, Room 204 | 30 |
| Electronics Kit Set | Lab Equipment | Electronics Lab | 5 |

## Features included

- Identity registration/login/logout
- Admin role authorization
- Resource create/read/update/delete with history protection
- Resource name/category/location filters
- Booking request forms and server-side date/time validation
- Pending/Approved/Rejected/Cancelled statuses
- Overlap checks on request and approval
- Serializable transaction around booking changes to reduce concurrent conflicts
- Users see only their own bookings; admins see all bookings
- Cancellation rules
- Dashboard counters
- TempData feedback and responsive Bootstrap layout

## Important technical note

The serializable transaction is a practical baseline, not a substitute for production-grade concurrency testing under every MySQL isolation/configuration. If this becomes a real campus service, add automated integration tests, a resource-row locking strategy, audit logging, rate limiting and a formal reservation policy.

## Common fixes

- **Cannot connect to MySQL:** check the server service, port 3306, username/password and database name.
- **SDK not found:** rerun `setup-portable-dotnet.cmd`, then set `DOTNET_ROOT` and `PATH` as shown above.
- **Package restore fails:** check internet access and package versions with `dotnet list package`.
- **Migration/table error:** back up any existing data before changing the database. For a fresh college project database only, you can drop and recreate the empty database, then rerun.
- **Admin not created:** set `ADMIN_EMAIL` and `ADMIN_PASSWORD` before the first `dotnet run`; check terminal output for password policy errors.

## Viva summary

The application follows MVC. Controllers handle HTTP requests, Razor views display forms and tables, models represent resources and bookings, and EF Core maps those models to MySQL. Identity handles password hashing and authentication. Role-based authorization restricts administrator routes. BookingService checks date/time rules and overlapping bookings on the server, and uses a serializable transaction to reduce race conditions. TempData displays one-time messages after redirects.
