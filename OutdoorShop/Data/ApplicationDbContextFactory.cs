using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OutdoorShop.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var contentRoot = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(contentRoot, "OutdoorShop.csproj")))
            contentRoot = Path.Combine(contentRoot, "OutdoorShop");

        var databaseDirectory = Path.Combine(contentRoot, "App_Data");
        Directory.CreateDirectory(databaseDirectory);
        var connectionString = $"Data Source={Path.Combine(databaseDirectory, "outdoorshop.db")}";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }
}
