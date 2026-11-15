using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Data;

var builder = WebApplication.CreateBuilder(args);

var databaseDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(databaseDirectory);
var defaultDatabasePath = Path.Combine(databaseDirectory, "outdoorshop.db");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? $"Data Source={defaultDatabasePath}";

var isRailway = !string.IsNullOrWhiteSpace(builder.Configuration["RAILWAY_ENVIRONMENT_ID"]);
if (isRailway)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Railway does not publish stable ingress proxy IP ranges. Trust only its
        // single forwarded scheme value, and treat this Railway environment as
        // the trust boundary. Keep untrusted services and TCP proxies out of it.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var secureCookiePolicy = builder.Environment.IsProduction()
    ? CookieSecurePolicy.Always
    : CookieSecurePolicy.SameAsRequest;
builder.Services.ConfigureApplicationCookie(options =>
    options.Cookie.SecurePolicy = secureCookiePolicy);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = secureCookiePolicy;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (isRailway)
    app.UseForwardedHeaders();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await IdentitySeedData.InitializeAsync(app.Services, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
