# OutdoorShop

OutdoorShop е уеб магазин на ASP.NET Core MVC за туристическа и къмпинг екипировка. Проектът използва SQLite и създава базата автоматично при първо стартиране.

## Features

- Browse products by category, search the catalog, and sort results.
- View product details, prices, and stock availability.
- Add items to a session-based cart and check out as a guest or signed-in customer.
- Save orders transactionally, update stock, and prevent duplicate checkout submissions.
- Register and manage a customer profile, order history, and order details.
- Manage products, categories, and order statuses through role-protected admin pages.
- Seed missing catalog examples and roles without overwriting existing catalog records.

## Technologies

- C#, .NET 9 и ASP.NET Core MVC
- Entity Framework Core 9 и SQLite
- ASP.NET Core Identity за регистрация, вход и роли
- Razor Views, Bootstrap, CSS и JavaScript
- xUnit тестове

## Стартиране с Visual Studio

1. Инсталирай Visual Studio 2022 версия 17.12 или по-нова с работното натоварване **ASP.NET and web development** и .NET 9 SDK.
2. Изтегли ZIP архива на проекта от GitHub и го разархивирай.
3. Отвори `OutdoorShop.sln` във Visual Studio.
4. Изчакай възстановяването на NuGet пакетите.
5. Натисни **F5**.

Не са нужни XAMPP, MySQL, MariaDB, Docker, User Secrets или променливи на средата. При първото стартиране приложението създава `OutdoorShop/App_Data/outdoorshop.db`, прилага SQLite миграциите и добавя примерните категории и продукти. Следващите стартирания използват същия файл. Обикновеното изграждане на проекта не го изтрива. Не изтривай файла, ако искаш да запазиш локалните поръчки и профили.

Приложението включва регистрация и вход на клиенти, разглеждане на продукти и категории, количка и поръчки. Ролите `Admin` и `Customer` се създават автоматично. За защита на администраторските страници е необходим потребител с роля `Admin`; не е зададена обща парола в проекта. По желание администратор може да бъде зададен чрез `AdminSeed:Email` и `AdminSeed:Password` във Visual Studio Debug environment variables. Паролата не трябва да се записва в GitHub.

## База данни и запазване на старите данни

SQLite базата е отделна от старата MySQL/MariaDB база. Приложението не се свързва с нея и не променя нейни файлове. Миграциите създават структурата за SQLite; те не пренасят записите от старата база.

Ако трябва да се запазят старите 30 продукта, 14 категории, 4 поръчки и потребителите, нужни са еднократен експорт от работещо копие/възстановен SQL архив и импортер към вече създадената SQLite схема. Експортът трябва да включва `Categories`, `Products`, `Orders`, `OrderItems`, `CustomerProfiles` и Identity таблиците (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`). Импортерът трябва да запази ID стойностите, паролни хешове и връзките между тези записи; примерният каталог трябва да се съпостави по ID/име, за да се избегнат дублирания. В този репозиторий няма SQL архив или автоматичен конвертор, затова тези записи не могат да се пренесат само от GitHub ZIP. Старата база остава непроменена.

При празна SQLite база се добавят примерни данни от `IdentitySeedData`. Това са демонстрационни записи, а не копие на старите 30 продукта или поръчки.

## Стартиране от терминал

```powershell
dotnet restore .\OutdoorShop.sln
dotnet run --project .\OutdoorShop\OutdoorShop.csproj
```

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
