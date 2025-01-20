using Model.Data;

namespace Model;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
   Mandatory installation (one-time only):
   dotnet tool install --global dotnet-ef

   Migration management commands:
   dotnet ef migrations add InitialMigration --project ./Model --startup-project ./Controller
   dotnet ef database update --project ./Model --startup-project ./Controller
   dotnet ef database update PreviousMigrationName --project ./Model --startup-project ./Controller
*/
public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}