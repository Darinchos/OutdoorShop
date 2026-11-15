# OutdoorShop

OutdoorShop е учебен онлайн магазин за туристическа и къмпинг екипировка. Проектът е изграден с ASP.NET Core MVC и Razor, с интерфейс на български език и responsive оформление за телефон, таблет и настолен компютър.

## Технологии

- .NET 9 и C#
- ASP.NET Core MVC и Razor Views
- Entity Framework Core 9
- MariaDB/MySQL чрез Pomelo
- ASP.NET Core Identity и роли `Customer` / `Admin`
- Bootstrap, CSS и малко JavaScript

## Функционалности

- Начална страница с категории и избрани налични продукти.
- Каталог с търсене по име/описание, филтър по категория и сортиране.
- Детайли за продукт, наличност и избор на количество.
- Количка в сесията с контрол на наличностите.
- Поръчка като гост или влязъл клиент; цените и складовите количества се четат от базата.
- Запис на поръчката и намаляване на наличността в транзакция. Checkout ключът предотвратява повторно създаване на една и съща поръчка.
- Клиентски профил, история и детайли на собствените поръчки.
- Административно табло, управление на продукти и категории и промяна на статус на поръчка.
- Публичната регистрация създава само клиенти. Административните страници изискват роля `Admin`.

## Изисквания

- .NET 9 SDK
- XAMPP с MariaDB 10.4 или съвместима MariaDB/MySQL база
- Достъп до NuGet при първо възстановяване на пакетите

## Настройване на MariaDB с XAMPP

1. Стартирай MySQL/MariaDB от XAMPP.
2. Създай база `outdoorshop` (например през phpMyAdmin или `CREATE DATABASE outdoorshop CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`).
3. Настрой `ConnectionStrings:DefaultConnection` чрез User Secrets или environment variables. В git няма локална база, пароли или connection string.

PowerShell с User Secrets:

```powershell
cd .\OutdoorShop
dotnet user-secrets init --project .\OutdoorShop\OutdoorShop.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=outdoorshop;user=root;password=ТВОЯТА_ПАРОЛА" --project .\OutdoorShop\OutdoorShop.csproj
```

Може да се използват и променливите `ConnectionStrings__DefaultConnection`, `AdminSeed__Email` и `AdminSeed__Password`.

## Миграции и съществуваща база

Приложението **не прилага миграции автоматично**. Преди промяна на съществуваща база направи резервно копие и провери таблиците и историята на миграциите. Не изтривай база или таблици, за да заобиколиш несъответствие.

От корена на хранилището провери миграциите:

```powershell
dotnet ef migrations list --project .\OutdoorShop\OutdoorShop.csproj --startup-project .\OutdoorShop\OutdoorShop.csproj
```

При съществуващ магазин първо потвърди, че очакваните предишни миграции са приложени и схемата съответства. Новата миграция `CustomerAccountsAndOrderWorkflow` добавя профили, връзка между клиент и поръчка, статус и ключ за защита от дублиране. Провери SQL преди прилагане:

Имай предвид, че `InitialCreate` създава таблиците за поръчки, но използва вече съществуващата таблица `Products` за външния ключ. Миграцията `IdentityAndDataConstraints` добавя Identity и намалява максималните дължини и прецизността на някои колони. Преди прилагането ѝ провери старите стойности за съвместимост. Не приемай, че цялата верига може да създаде празна база от нулата или че липсващата история означава празна база.

```powershell
dotnet ef migrations script --project .\OutdoorShop\OutdoorShop.csproj --startup-project .\OutdoorShop\OutdoorShop.csproj
```

След архивиране и потвърждение, че схемата е съвместима, администраторът може да приложи миграцията ръчно:

```powershell
dotnet ef database update --project .\OutdoorShop\OutdoorShop.csproj --startup-project .\OutdoorShop\OutdoorShop.csproj
```

Включената фабрика за design-time контекст позволява на EF CLI да генерира миграции, без да стартира приложението и неговото начално зареждане на данни.

## Администраторски акаунт

Настрой имейл и силна парола локално преди стартиране. Не ги записвай в конфигурационни файлове, commit или логове.

```powershell
$env:AdminSeed__Email = "admin@example.com"
$env:AdminSeed__Password = "Change-This-Strong-Password"
dotnet run --project .\OutdoorShop\OutdoorShop.csproj
```

При старт се създават липсващите роли и се добавят липсващи примерни категории/продукти. Каталогът се зарежда добавъчно; съществуващите продукти, категории и поръчки не се презаписват. Администраторът се конфигурира само чрез тези настройки.

## Стартиране

От корена на хранилището:

```powershell
dotnet restore .\OutdoorShop.sln
dotnet build .\OutdoorShop.sln
dotnet run --project .\OutdoorShop\OutdoorShop.csproj
```

Локалните адреси са `http://localhost:5249` и `https://localhost:7286` според профила в `launchSettings.json`. Необходима е подготвена база със съвместима схема преди стартиране, понеже началното зареждане използва базата.

## Основни адреси

- `/` — начало
- `/Products` — каталог, търсене и филтри
- `/Products/Details/{id}` — продукт
- `/Categories` — категории
- `/Cart` — количка
- `/Order/Checkout` — поръчка
- `/Account/Register`, `/Account/Login`, `/Account/Profile` — регистрация, вход и профил
- `/Account/MyOrders` — поръчки на влезлия клиент
- `/Admin` — административно табло
- `/Orders` — администраторски списък и детайли на поръчки

## Проверки

```powershell
dotnet restore .\OutdoorShop.sln
dotnet build .\OutdoorShop.sln --no-restore
```

Автоматизирани integration тестове с MariaDB не са включени. Потокът за реална база (регистрация, checkout и администраторски операции) трябва да се провери след локална настройка на XAMPP и ръчно прилагане на миграцията.

## Снимки и лицензи

Снимките за каталога и началната страница се съхраняват в `OutdoorShop/wwwroot/images`. Те са илюстративни и не представят непременно точен модел или марка. При липсващ файл се показва локалното изображение `product-placeholder.svg`.

Фотографиите са свалени от Pexels и се използват съгласно [Pexels License](https://www.pexels.com/license/). Автори и източници:

- [Lukas Seitz — mountain tents](https://www.pexels.com/photo/tents-on-camping-in-mountains-17079655/) (hero and tent photos)
- [Sidde — tent in a mountain field](https://www.pexels.com/photo/a-camping-tent-on-grass-field-under-the-blue-sky-8220281/) (shelter)
- [Ioan-Dan Plesa — backpack on grass](https://www.pexels.com/photo/backpack-on-grass-14718806/) (backpack)
- [kwnos Iv — hiker carrying backpack and sleeping bag](https://www.pexels.com/photo/hiker-with-backpack-and-sleeping-bag-15782410/) (sleep system)
- [Buğra Özcan — lantern by a tent](https://www.pexels.com/photo/lamp-on-tent-on-camping-22669473/) (camp lighting)
- [Marek Piwnicki — headlamp on a night hike](https://www.pexels.com/photo/mysterious-nighttime-hike-with-headlamp-light-29176030/) (headlamp)
- [Simon Migaj — hiker with headlamp](https://www.pexels.com/photo/a-person-wearing-a-lighted-headlamp-standing-outdoors-762420/) (headlamp)
- [Gabriel Felix — rainy hike](https://www.pexels.com/photo/woman-in-gray-jacket-walking-on-mountain-area-4273239/) (outerwear)
- [NHP&Co — hiker wearing a cap](https://www.pexels.com/photo/stubbled-man-with-black-cap-and-sports-sunglasses-17671556/) (cap)
- [Vincent M.A. Janssen — hiking boots](https://www.pexels.com/photo/hiking-boots-in-close-up-4923342/) (footwear)
- [Omid Jannati — hikers with boots and gaiters](https://www.pexels.com/photo/legs-of-people-in-outdoor-clothing-and-shoes-17844768/) (gaiters)
- [Vintage Laka — hiker with trekking pole](https://www.pexels.com/photo/man-hiking-in-rocky-mountain-terrain-outdoors-34733420/) (trekking equipment)
- [Maël Balland — cooking outdoors](https://www.pexels.com/photo/frying-pan-on-bonfire-10807704/) (cookware)
- [Osman SRAN — pocket knife and field notebook](https://www.pexels.com/photo/pocket-knife-and-lighter-on-field-notebook-22798297/) (outdoor tool)
- [Osman SRAN — gloves and wooden cup in a forest](https://www.pexels.com/photo/pair-of-yellow-protective-gloves-and-a-wooden-cup-on-a-log-in-the-forest-23956824/) (outdoor gloves)
- [Alexander Mass — hiking gear](https://www.pexels.com/photo/outdoor-hiking-gear-with-backpacks-and-notes-38881957/) (outdoor accessories)
- [Rohan Sudan — mountain camping at night](https://www.pexels.com/photo/brown-ten-under-starry-sky-12058916/) (camp shelter)
- [Sidde — backpack and camping mat](https://www.pexels.com/photo/big-backpack-with-mat-on-a-grass-field-8220243/) (day pack)
- [Ali Kazal — backpack beside a tent](https://www.pexels.com/photo/a-backpack-near-the-tent-10762614/) (dry bag)
- [Andrea Román — mountain hiker](https://www.pexels.com/photo/man-in-jacket-hiking-15475227/) (fleece)
- [Никита Шелайкин — boots at a mountain tent](https://www.pexels.com/photo/shoes-in-front-of-a-tent-in-a-mountain-valley-20890180/) (hiking shoes)
- [Guohua Song — mountain camp with chair and lantern](https://www.pexels.com/photo/tranquil-mountain-camping-at-sunset-37438097/) (camp furniture)
- [Uriel Mont — portable table and camp cookware](https://www.pexels.com/photo/portable-table-with-vacuum-flasks-on-coast-6271623/) (camp table)
- [Mykhailo Petrushchak — tent interior with sleeping mat and cookware](https://www.pexels.com/photo/cozy-camping-tent-setup-with-cookware-31443014/) (sleeping comfort and cooking)
- [Karson — sleeping bag](https://www.pexels.com/photo/woman-lying-in-a-sleeping-bag-in-snow-6299741/) (sleeping bag)
- [Marina Andreeva — bottle on mountain rocks](https://www.pexels.com/photo/flask-on-rock-in-mountains-25382222/) (hydration)
- [Ninety Seven Years — bottle outdoors](https://www.pexels.com/photo/white-water-bottle-on-wooden-bench-outdoors-34140877/) (water container)
- [neslihan — folding camping chair](https://www.pexels.com/photo/green-folding-camping-chair-in-outdoor-setting-35223580/) (camp chair)
- [Alex Moliski — backpacks in forest](https://www.pexels.com/photo/backpacks-resting-against-tree-in-forest-setting-32290434/) (pack cover)

Изображенията са запазени локално с максимална ширина 1200 пиксела. Няколко сходни продукта използват обща снимка за съответната категория.
