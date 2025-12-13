using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using EduMaster.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ
// ==========================================

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index";
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

// ==========================================
// 2. АВТОЗАПОЛНЕНИЕ БАЗЫ ДАННЫХ
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

        // Создаем базу заново
        context.Database.EnsureCreated();

        // Функция чтения файла
        byte[]? GetImageBytes(string filename)
        {
            var path = Path.Combine(env.WebRootPath, "img", filename);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        // --- 1. КАТЕГОРИИ ---
        if (!context.CategoryDb.Any())
        {
            context.CategoryDb.AddRange(
                new CategoryDb { Id = Guid.NewGuid(), name = "Программирование" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Дизайн и UX/UI" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Мобильная разработка" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Маркетинг и бизнес" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Кибербезопасность" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Психология" },
                new CategoryDb { Id = Guid.NewGuid(), name = "Иностранные языки" }
            );
            context.SaveChanges();
        }

        // --- 2. КУРСЫ С ИНДИВИДУАЛЬНЫМИ КАРТИНКАМИ ---
        if (!context.CourseDb.Any())
        {
            // Получаем ID категорий
            var catProg = context.CategoryDb.FirstOrDefault(c => c.name == "Программирование")?.Id ?? Guid.NewGuid();
            var catDesign = context.CategoryDb.FirstOrDefault(c => c.name == "Дизайн и UX/UI")?.Id ?? Guid.NewGuid();
            var catMobile = context.CategoryDb.FirstOrDefault(c => c.name == "Мобильная разработка")?.Id ?? Guid.NewGuid();
            var catMarketing = context.CategoryDb.FirstOrDefault(c => c.name == "Маркетинг и бизнес")?.Id ?? Guid.NewGuid();
            var catSecurity = context.CategoryDb.FirstOrDefault(c => c.name == "Кибербезопасность")?.Id ?? Guid.NewGuid();
            var catPsychology = context.CategoryDb.FirstOrDefault(c => c.name == "Психология")?.Id ?? Guid.NewGuid();
            var catLanguages = context.CategoryDb.FirstOrDefault(c => c.name == "Иностранные языки")?.Id ?? Guid.NewGuid();

            // === СПИСОК ВСЕХ КУРСОВ И ИХ КАРТИНОК ===
            // В поле Img указывайте точное название файла из папки wwwroot/img
            var coursesData = new[]
            {
                // === ПРОГРАММИРОВАНИЕ ===
                new { CatId = catProg, Title = "Python-разработчик с нуля", Desc = "Изучите основы Python, работу с данными и создайте своего первого Telegram-бота.", Price = 15000m, Img = "python_start.jpg" },
                new { CatId = catProg, Title = "C# и .NET для профи", Desc = "Глубокое погружение в платформу .NET, базы данных и архитектуру.", Price = 25000m, Img = "2.jpg" },
                new { CatId = catProg, Title = "Java Enterprise", Desc = "Разработка серверных приложений на Java. Spring Framework, Hibernate.", Price = 28000m, Img = "3.jpg" },
                new { CatId = catProg, Title = "Fullstack JavaScript", Desc = "React.js на фронтенде и Node.js на бэкенде.", Price = 20000m, Img = "4.jpg" },
                new { CatId = catProg, Title = "DevOps инженер", Desc = "Docker, Kubernetes, CI/CD. Настройка серверов.", Price = 32000m, Img = "5.jpg" },
                new { CatId = catProg, Title = "Алгоритмы и структуры данных", Desc = "Подготовка к собеседованиям в Big Tech компании.", Price = 10000m, Img = "6.jpg" },

                // === ДИЗАЙН ===
                new { CatId = catDesign, Title = "Веб-дизайн в Figma", Desc = "Создавайте современные и удобные интерфейсы сайтов.", Price = 12000m, Img = "figma.jpg" },
                new { CatId = catDesign, Title = "Графический дизайн", Desc = "Работа в Adobe Photoshop и Illustrator. Айдентика бренда.", Price = 14000m, Img = "graphic_design.jpg" },
                new { CatId = catDesign, Title = "UX-исследования", Desc = "CJM, юзабилити-тестирование и аналитика.", Price = 16000m, Img = "ux_research.jpg" },
                new { CatId = catDesign, Title = "3D-моделирование (Blender)", Desc = "Создание 3D персонажей и сцен.", Price = 22000m, Img = "blender.jpg" },
                new { CatId = catDesign, Title = "Motion Design", Desc = "Анимация интерфейсов и рекламных роликов.", Price = 19000m, Img = "motion.jpg" },

                // === МОБИЛЬНАЯ РАЗРАБОТКА ===
                new { CatId = catMobile, Title = "Android на Kotlin", Desc = "Нативная разработка. От макета до Google Play.", Price = 22000m, Img = "android.jpg" },
                new { CatId = catMobile, Title = "iOS-разработчик (Swift)", Desc = "Создание приложений для iPhone и iPad.", Price = 24000m, Img = "ios.jpg" },
                new { CatId = catMobile, Title = "Flutter: Кроссплатформа", Desc = "Одно приложение для iOS и Android на языке Dart.", Price = 20000m, Img = "flutter.jpg" },
                new { CatId = catMobile, Title = "React Native", Desc = "Мобильная разработка на JavaScript.", Price = 18000m, Img = "react_native.jpg" },

                // === МАРКЕТИНГ ===
                new { CatId = catMarketing, Title = "SMM-менеджер", Desc = "Продвижение в соцсетях. Контент-план и таргетинг.", Price = 18000m, Img = "smm.jpg" },
                new { CatId = catMarketing, Title = "Интернет-маркетолог", Desc = "SEO, контекстная реклама и аналитика.", Price = 20000m, Img = "marketing.jpg" },
                new { CatId = catMarketing, Title = "Project Manager (PM)", Desc = "Управление IT-проектами. Agile, Scrum.", Price = 25000m, Img = "pm.jpg" },
                new { CatId = catMarketing, Title = "Финансовая грамотность", Desc = "Личный бюджет и инвестиции для начинающих.", Price = 8000m, Img = "finance.jpg" },
                new { CatId = catMarketing, Title = "Запуск стартапа", Desc = "От идеи до первого инвестора.", Price = 30000m, Img = "startup.jpg" },

                // === КИБЕРБЕЗОПАСНОСТЬ ===
                new { CatId = catSecurity, Title = "Белый хакер (Pentest)", Desc = "Поиск уязвимостей и защита веб-ресурсов.", Price = 30000m, Img = "hacker.jpg" },
                new { CatId = catSecurity, Title = "Сетевая безопасность", Desc = "Firewalls, VPN, защита корпоративных сетей.", Price = 28000m, Img = "network_sec.jpg" },
                new { CatId = catSecurity, Title = "Криптография", Desc = "Блокчейн, цифровые подписи и шифрование.", Price = 25000m, Img = "crypto.jpg" },
                new { CatId = catSecurity, Title = "Анализ вирусов", Desc = "Реверс-инжиниринг вредоносного ПО.", Price = 35000m, Img = "virus_analysis.jpg" },

                // === ПСИХОЛОГИЯ ===
                new { CatId = catPsychology, Title = "Психология общения", Desc = "Эффективные коммуникации и разрешение конфликтов.", Price = 9000m, Img = "psycho_comm.jpg" },
                new { CatId = catPsychology, Title = "Когнитивная психология", Desc = "Память, внимание, принятие решений.", Price = 11000m, Img = "cognitive.jpg" },
                new { CatId = catPsychology, Title = "Эмоциональный интеллект", Desc = "Управление эмоциями и развитие эмпатии.", Price = 10000m, Img = "eq.jpg" },
                new { CatId = catPsychology, Title = "Борьба со стрессом", Desc = "Техники релаксации и профилактика выгорания.", Price = 7000m, Img = "stress.jpg" },

                // === ЯЗЫКИ ===
                new { CatId = catLanguages, Title = "Английский для IT", Desc = "Техническая документация и собеседования.", Price = 11000m, Img = "english_it.jpg" },
                new { CatId = catLanguages, Title = "Разговорный английский", Desc = "Практика речи с носителями.", Price = 15000m, Img = "english_speak.jpg" },
                new { CatId = catLanguages, Title = "Немецкий: Уровень A1", Desc = "Базовый курс для путешествий.", Price = 12000m, Img = "german.jpg" },
                new { CatId = catLanguages, Title = "Китайский для бизнеса", Desc = "Деловой этикет и переговоры.", Price = 18000m, Img = "chinese.jpg" }
            };

            // Перебираем данные и сохраняем в БД
            foreach (var item in coursesData)
            {
                // 1. Создаем курс
                var newCourse = new CourseDb
                {
                    Id = Guid.NewGuid(),
                    title = item.Title,
                    description = item.Desc,
                    price = item.Price,
                    is_active = true,
                    created_at = DateTime.UtcNow,
                    category_id = item.CatId
                };
                context.CourseDb.Add(newCourse);

                // 2. Ищем картинку на диске
                byte[]? imgBytes = GetImageBytes(item.Img);

                // Если вашей картинки нет, можно раскомментировать строку ниже, 
                // чтобы поставить заглушку (например, course1.jpg), если хотите:
                // if (imgBytes == null) imgBytes = GetImageBytes("course1.jpg");

                if (imgBytes != null)
                {
                    var newImage = new CourseImageDb
                    {
                        Id = Guid.NewGuid(),
                        CourseId = newCourse.Id,
                        Data = imgBytes,
                        ContentType = "image/jpeg"
                    };
                    context.CourseImageDb.Add(newImage);
                }
            }

            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при автозаполнении БД.");
    }
}

// ==========================================
// 3. ЗАПУСК
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();