# OutdoorShop

OutdoorShop is an ASP.NET Core MVC storefront for camping and hiking equipment. It includes a Bulgarian-language, responsive shopping experience backed by MariaDB, with customer accounts, order management, and an admin area.

## Features

- Browse products by category, search the catalog, and sort results.
- View product details, prices, and stock availability.
- Add items to a session-based cart and check out as a guest or signed-in customer.
- Save orders transactionally, update stock, and prevent duplicate checkout submissions.
- Register and manage a customer profile, order history, and order details.
- Manage products, categories, and order statuses through role-protected admin pages.
- Seed missing catalog examples and roles without overwriting existing catalog records.

## Technologies

- C# and .NET 9 / ASP.NET Core MVC
- Entity Framework Core 9 and EF Core migrations
- MariaDB or MySQL through Pomelo.EntityFrameworkCore.MySql
- ASP.NET Core Identity
- Razor Views, Bootstrap, CSS, and JavaScript
- xUnit tests

## Requirements

- .NET 9 SDK
- MariaDB 10.4 or a compatible MySQL/MariaDB server
- Git
- Internet access for the initial NuGet package restore

## Installation

Clone the repository, restore packages, and build the solution:

```powershell
git clone https://github.com/Darinchos/OutdoorShop.git
cd OutdoorShop
dotnet restore .\OutdoorShop.sln
dotnet build .\OutdoorShop.sln
```

## Database setup

Create a local MariaDB database, for example:

```sql
CREATE DATABASE outdoorshop CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
```

Keep the connection string in ASP.NET Core User Secrets. The project already has a `UserSecretsId`, so there is no need to run `dotnet user-secrets init`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=127.0.0.1;port=3306;database=outdoorshop;user=root;password=YOUR_LOCAL_PASSWORD" --project .\OutdoorShop\OutdoorShop.csproj
```

Use your local MariaDB username and password. Do not commit credentials, User Secrets, database files, or SQL exports. For local deployments, environment variables such as `ConnectionStrings__DefaultConnection` can also provide configuration.

### Schema and migrations

The application does not apply migrations automatically. Review the current migration history and schema before updating an existing database:

```powershell
dotnet ef migrations list --project .\OutdoorShop\OutdoorShop.csproj --startup-project .\OutdoorShop\OutdoorShop.csproj
dotnet ef database update --project .\OutdoorShop\OutdoorShop.csproj --startup-project .\OutdoorShop\OutdoorShop.csproj
```

The checked-in migration chain starts from the application's earlier catalog schema: `Products` and `Categories` must already exist before the `InitialCreate` migration, which creates order tables. Later migrations add Identity and customer/order workflow tables and constraints. A completely empty database therefore needs those baseline catalog tables created first. For an existing store, verify its schema and applied migration history before running an update; do not treat a missing migration history as proof that the database is empty.

## Configure an administrator

Set the initial administrator account locally before starting the app. Keep these values out of source files and commits:

```powershell
$env:AdminSeed__Email = "admin@example.com"
$env:AdminSeed__Password = "Use-A-Strong-Local-Password"
```

At startup, the application creates missing `Admin` and `Customer` roles. The configured administrator is created only when both settings are provided. Public registration creates customer accounts only.

## Run the application

```powershell
dotnet run --project .\OutdoorShop\OutdoorShop.csproj
```

The development launch profile serves HTTP at `http://localhost:5249` and HTTPS at `https://localhost:7286`. A compatible database schema and a local connection string must be configured before startup.

## Run tests

```powershell
dotnet test .\OutdoorShop.sln
```

## Screenshots

Screenshots are not included yet. Add captures under `docs/screenshots/` and link them in this section when available.

## Image credits

Product and storefront images are illustrative and are used under the [Pexels License](https://www.pexels.com/license/). The images are stored locally under `OutdoorShop/wwwroot/images`; several related products share a category image.

- [Lukas Seitz — mountain tents](https://www.pexels.com/photo/tents-on-camping-in-mountains-17079655/) (hero and tent photos)
- [Sidde — tent in a mountain field](https://www.pexels.com/photo/a-camping-tent-on-grass-field-under-the-blue-sky-8220281/) (shelter)
- [Ioan-Dan Plesa — backpack on grass](https://www.pexels.com/photo/backpack-on-grass-14718806/) (backpack)
- [kwnos Iv — hiker with backpack and sleeping bag](https://www.pexels.com/photo/hiker-with-backpack-and-sleeping-bag-15782410/) (sleep system)
- [Buğra Özcan — lantern by a tent](https://www.pexels.com/photo/lamp-on-tent-on-camping-22669473/) (camp lighting)
- [Marek Piwnicki — headlamp on a night hike](https://www.pexels.com/photo/mysterious-nighttime-hike-with-headlamp-light-29176030/) and [Simon Migaj — hiker wearing a headlamp](https://www.pexels.com/photo/a-person-wearing-a-lighted-headlamp-standing-outdoors-762420/) (headlamps)
- [Gabriel Felix — rainy hike](https://www.pexels.com/photo/woman-in-gray-jacket-walking-on-mountain-area-4273239/) (outerwear)
- [NHP&Co — hiker wearing a cap](https://www.pexels.com/photo/stubbled-man-with-black-cap-and-sports-sunglasses-17671556/) (cap)
- [Vincent M.A. Janssen — hiking boots](https://www.pexels.com/photo/hiking-boots-in-close-up-4923342/) and [Никита Шелайкин — boots at a mountain tent](https://www.pexels.com/photo/shoes-in-front-of-a-tent-in-a-mountain-valley-20890180/) (footwear)
- [Omid Jannati — hikers with boots and gaiters](https://www.pexels.com/photo/legs-of-people-in-outdoor-clothing-and-shoes-17844768/) (gaiters)
- [Vintage Laka — hiker with trekking pole](https://www.pexels.com/photo/man-hiking-in-rocky-mountain-terrain-outdoors-34733420/) (trekking equipment)
- [Maël Balland — cooking outdoors](https://www.pexels.com/photo/frying-pan-on-bonfire-10807704/) (cookware)
- [Osman SRAN — pocket knife and field notebook](https://www.pexels.com/photo/pocket-knife-and-lighter-on-field-notebook-22798297/) (outdoor tool)
- [Osman SRAN — gloves and wooden cup in a forest](https://www.pexels.com/photo/pair-of-yellow-protective-gloves-and-a-wooden-cup-on-a-log-in-the-forest-23956824/) (gloves)
- [Alexander Mass — hiking gear](https://www.pexels.com/photo/outdoor-hiking-gear-with-backpacks-and-notes-38881957/) (outdoor accessories)
- [Rohan Sudan — mountain camping at night](https://www.pexels.com/photo/brown-ten-under-starry-sky-12058916/) (camp shelter)
- [Sidde — backpack and camping mat](https://www.pexels.com/photo/big-backpack-with-mat-on-a-grass-field-8220243/) (day pack)
- [Ali Kazal — backpack beside a tent](https://www.pexels.com/photo/a-backpack-near-the-tent-10762614/) (dry bag)
- [Andrea Román — mountain hiker](https://www.pexels.com/photo/man-in-jacket-hiking-15475227/) (fleece)
- [Guohua Song — mountain camp with chair and lantern](https://www.pexels.com/photo/tranquil-mountain-camping-at-sunset-37438097/) (camp furniture)
- [Uriel Mont — portable table and camp cookware](https://www.pexels.com/photo/portable-table-with-vacuum-flasks-on-coast-6271623/) (camp table)
- [Mykhailo Petrushchak — tent interior with sleeping mat and cookware](https://www.pexels.com/photo/cozy-camping-tent-setup-with-cookware-31443014/) (sleeping comfort and cooking)
- [Karson — sleeping bag](https://www.pexels.com/photo/woman-lying-in-a-sleeping-bag-in-snow-6299741/) (sleeping bag)
- [Marina Andreeva — bottle on mountain rocks](https://www.pexels.com/photo/flask-on-rock-in-mountains-25382222/) and [Ninety Seven Years — bottle outdoors](https://www.pexels.com/photo/white-water-bottle-on-wooden-bench-outdoors-34140877/) (hydration)
- [neslihan — folding camping chair](https://www.pexels.com/photo/green-folding-camping-chair-in-outdoor-setting-35223580/) (camp chair)
- [Alex Moliski — backpacks in forest](https://www.pexels.com/photo/backpacks-resting-against-tree-in-forest-setting-32290434/) (pack cover)
