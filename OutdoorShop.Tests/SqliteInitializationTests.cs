using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutdoorShop.Data;

namespace OutdoorShop.Tests;

public sealed class SqliteInitializationTests
{
    [Fact]
    public async Task FreshSqliteDatabase_AppliesMigrationsAndSeedsCatalogAndRoles()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        await using var serviceProvider = services.BuildServiceProvider();
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
        }

        await IdentitySeedData.InitializeAsync(serviceProvider, new ConfigurationBuilder().Build());

        await using var verifyScope = serviceProvider.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = verifyScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Assert.Equal(14, await verifyContext.Categories.CountAsync());
        Assert.Equal(26, await verifyContext.Products.CountAsync());
        Assert.True(await roleManager.RoleExistsAsync("Admin"));
        Assert.True(await roleManager.RoleExistsAsync("Customer"));
        Assert.Single(await verifyContext.Database.GetAppliedMigrationsAsync());
    }
}
