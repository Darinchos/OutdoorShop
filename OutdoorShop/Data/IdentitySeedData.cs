using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutdoorShop.Models;

namespace OutdoorShop.Data;

public static class IdentitySeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var roleName in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded && !await roleManager.RoleExistsAsync(roleName))
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create the {roleName} role: {errors}");
                }
            }
        }

        await SeedCatalogAsync(context);

        var adminEmail = configuration["AdminSeed:Email"]?.Trim();
        var adminPassword = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the configured admin account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not assign the Admin role: {errors}");
            }
        }
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        var categories = await context.Categories.ToListAsync();
        var needsStarterProducts = !await context.Products.AnyAsync();
        var categoryDefinitions = new (string Name, string Description)[]
        {
            ("Палатки", "Палатки и заслони за нощувки и почивка сред природата."),
            ("Раници", "Раници и чанти за преходи и къмпинг."),
            ("Спални чували", "Екипировка за почивка и нощуване на открито."),
            ("Челници и фенери", "Преносимо осветление за лагер и маршрут."),
            ("Дрехи", "Практични дрехи за променливи условия навън."),
            ("Шапки", "Шапки и защита от слънце за дейности на открито."),
            ("Обувки", "Обувки за разходки и планински маршрути."),
            ("Туристическа екипировка", "Полезна екипировка за пешеходни преходи."),
            ("Къмпинг оборудване", "Принадлежности за лагеруване и почивка сред природата."),
            ("Аксесоари", "Малки принадлежности за пътувания и маршрути."),
            ("Къмпинг мебели", "Сгъваеми мебели за почивка на къмпинг."),
            ("Постелки и комфорт", "Постелки и принадлежности за почивка и сън."),
            ("Хидратация", "Бутилки и съдове за вода по време на път."),
            ("Туристическо готвене", "Компактни принадлежности за приготвяне на храна.")
        };

        foreach (var definition in categoryDefinitions)
        {
            var existing = categories.FirstOrDefault(category =>
                string.Equals(category.Name.Trim(), definition.Name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                existing = new Category { Name = definition.Name, Description = definition.Description };
                context.Categories.Add(existing);
                categories.Add(existing);
            }
        }
        await context.SaveChangesAsync();

        var categoryByName = categories.ToDictionary(category => category.Name, StringComparer.OrdinalIgnoreCase);
        if (needsStarterProducts)
        {
            context.Products.AddRange(
                new Product { Name = "Палатка Alpine 2", Description = "Лека двуслойна палатка за двама.", Price = 249.90m, Stock = 8, CategoryId = categoryByName["Палатки"].Id, ImageUrl = "/images/tent.jpg" },
                new Product { Name = "Спален чувал Trek", Description = "Компактен спален чувал за трисезонни преходи.", Price = 129.00m, Stock = 12, CategoryId = categoryByName["Спални чували"].Id, ImageUrl = "/images/sleepingbag.jpg" },
                new Product { Name = "Туристическа раница 35L", Description = "Удобна раница за еднодневни и уикенд маршрути.", Price = 179.50m, Stock = 10, CategoryId = categoryByName["Раници"].Id, ImageUrl = "/images/backpack.jpg" },
                new Product { Name = "Трекинг щеки", Description = "Регулируеми щеки с удобни дръжки.", Price = 69.90m, Stock = 15, CategoryId = categoryByName["Туристическа екипировка"].Id, ImageUrl = "/images/poles.jpg" },
                new Product { Name = "Ветроустойчиво яке", Description = "Леко яке за защита от вятър и кратък дъжд.", Price = 159.00m, Stock = 7, CategoryId = categoryByName["Дрехи"].Id, ImageUrl = "/images/jacket.jpg" },
                new Product { Name = "Термоблуза", Description = "Бързосъхнещ базов слой за активности навън.", Price = 59.90m, Stock = 20, CategoryId = categoryByName["Дрехи"].Id, ImageUrl = "/images/fleece.jpg" });
        }

        var products = new (string Name, string Description, decimal Price, int Stock, string Category, string Image)[]
        {
            ("Палатка за трима за къмпинг", "Просторна палатка за къмпинг и кратки почивки сред природата.", 329.90m, 6, "Палатки", "/images/tent-family.jpg"),
            ("Туристически бивак чувал", "Лек допълнителен заслон за нощуване и защита на екипировката.", 89.90m, 9, "Палатки", "/images/shelter.jpg"),
            ("Раница за еднодневен преход", "Компактна раница за вода, храна и основни принадлежности.", 119.90m, 14, "Раници", "/images/backpack-day.jpg"),
            ("Водоустойчива торба за багаж", "Лека затваряща се торба за подреждане и защита на багажа.", 34.90m, 22, "Раници", "/images/drybag.jpg"),
            ("Поларено горнище за преход", "Меко междинно облекло за прохладни дни и вечер край лагера.", 94.90m, 11, "Дрехи", "/images/fleece.jpg"),
            ("Дъждобран за туристически преход", "Лек външен слой за променливо време по маршрута.", 79.90m, 16, "Дрехи", "/images/rain-jacket.jpg"),
            ("Туристически обувки за лек терен", "Удобни обувки за разходки и леки маршрути на открито.", 189.90m, 8, "Обувки", "/images/hiking-shoes.jpg"),
            ("Гети за планински преход", "Допълнителен слой за защита на обувките и долната част на крачола.", 49.90m, 13, "Обувки", "/images/gaiters.jpg"),
            ("Сгъваем къмпинг стол", "Преносим стол за почивка край палатката или на къмпинг.", 69.90m, 10, "Къмпинг мебели", "/images/camp-chair.jpg"),
            ("Сгъваема къмпинг маса", "Компактна маса за хранене и подреждане на лагерното оборудване.", 109.90m, 7, "Къмпинг мебели", "/images/camp-table.jpg"),
            ("Туристическа постелка за сън", "Подложка за по-удобна почивка при къмпингуване.", 74.90m, 12, "Постелки и комфорт", "/images/sleeping-mat.jpg"),
            ("Компактна възглавница за къмпинг", "Лесна за пренасяне възглавница за нощувки извън дома.", 29.90m, 18, "Постелки и комфорт", "/images/camp-pillow.jpg"),
            ("Метална бутилка за вода", "Многократна бутилка за вода при разходки и пътуване.", 39.90m, 25, "Хидратация", "/images/water-bottle.jpg"),
            ("Сгъваем съд за вода", "Сгъваем съд за пренасяне на вода при къмпинг и преходи.", 24.90m, 20, "Хидратация", "/images/water-container.jpg"),
            ("Комплект съдове за къмпинг", "Компактни съдове за приготвяне и сервиране на храна на открито.", 84.90m, 9, "Туристическо готвене", "/images/cookware.jpg"),
            ("Туристическа чаша от метал", "Лека чаша за топли и студени напитки при пътуване.", 19.90m, 30, "Туристическо готвене", "/images/camping-mug.jpg"),
            ("Къмпинг фенер", "Преносим фенер за осветяване на лагерното място.", 54.90m, 12, "Челници и фенери", "/images/lantern.jpg"),
            ("Компактна челна лампа", "Свободни ръце и локално осветление за вечерни дейности.", 44.90m, 17, "Челници и фенери", "/images/headlamp-compact.jpg"),
            ("Калъф за туристическа раница", "Защитен калъф за раница при дъждовно време.", 32.90m, 15, "Раници", "/images/pack-cover.jpg"),
            ("Туристически ръкавици", "Леки ръкавици за комфорт при преходи в прохладно време.", 39.90m, 14, "Дрехи", "/images/hiking-gloves.jpg")
        };

        var existingNames = await context.Products.Select(product => product.Name).ToListAsync();
        var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        foreach (var item in products)
        {
            if (existingNameSet.Contains(item.Name))
                continue;

            context.Products.Add(new Product
            {
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                CategoryId = categoryByName[item.Category].Id,
                ImageUrl = item.Image
            });
            existingNameSet.Add(item.Name);
        }
        await context.SaveChangesAsync();
    }
}
