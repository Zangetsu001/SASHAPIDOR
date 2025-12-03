
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

        // Гарантируем, что база создана (включая новые таблицы)
        context.Database.EnsureCreated();

        // Вспомогательная функция для чтения картинки с диска
        byte[]? GetImageBytes(string filename)
        {
            var path = Path.Combine(env.WebRootPath, "img", filename);
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null;
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

        // --- 2. КУРСЫ ---
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

            // Создаем список курсов
            var courses = new List<CourseDb>
            {
                // Программирование
                new CourseDb { Id = Guid.NewGuid(), title = "Python-разработчик с нуля", description = "Изучите основы Python, работу с данными и создайте своего первого Telegram-бота.", price = 15000, is_active = true, created_at = DateTime.UtcNow, category_id = catProg },
                new CourseDb { Id = Guid.NewGuid(), title = "C# и .NET для профи", description = "Глубокое погружение в платформу .NET, базы данных и архитектуру корпоративных приложений.", price = 25000, is_active = true, created_at = DateTime.UtcNow, category_id = catProg },
                new CourseDb { Id = Guid.NewGuid(), title = "Java Enterprise", description = "Разработка серверных приложений на Java. Spring Framework, Hibernate и микросервисы.", price = 28000, is_active = true, created_at = DateTime.UtcNow, category_id = catProg },
                new CourseDb { Id = Guid.NewGuid(), title = "Fullstack JavaScript", description = "Станьте универсальным разработчиком: React.js на фронтенде и Node.js на бэкенде.", price = 20000, is_active = true, created_at = DateTime.UtcNow, category_id = catProg },
                new CourseDb { Id = Guid.NewGuid(), title = "DevOps инженер", description = "Автоматизация развертывания, Docker, Kubernetes и CI/CD процессы.", price = 32000, is_active = true, created_at = DateTime.UtcNow, category_id = catProg },

                // Дизайн
                new CourseDb { Id = Guid.NewGuid(), title = "Веб-дизайн в Figma", description = "Создавайте современные и удобные интерфейсы сайтов. Основы композиции, типографики и цвета.", price = 12000, is_active = true, created_at = DateTime.UtcNow, category_id = catDesign },
                new CourseDb { Id = Guid.NewGuid(), title = "Графический дизайн", description = "Работа в Adobe Photoshop и Illustrator. Создание логотипов и айдентики бренда.", price = 14000, is_active = true, created_at = DateTime.UtcNow, category_id = catDesign },
                new CourseDb { Id = Guid.NewGuid(), title = "UX-исследования", description = "Как понять пользователя? Интервью, CJM, юзабилити-тестирование и аналитика.", price = 16000, is_active = true, created_at = DateTime.UtcNow, category_id = catDesign },
                new CourseDb { Id = Guid.NewGuid(), title = "3D-моделирование в Blender", description = "Создание 3D персонажей и окружения. Текстурирование и рендер сцен.", price = 22000, is_active = true, created_at = DateTime.UtcNow, category_id = catDesign },

                // Мобильная разработка
                new CourseDb { Id = Guid.NewGuid(), title = "Android на Kotlin", description = "Разработка нативных мобильных приложений. От Hello World до публикации в Google Play.", price = 22000, is_active = true, created_at = DateTime.UtcNow, category_id = catMobile },
                new CourseDb { Id = Guid.NewGuid(), title = "iOS-разработчик (Swift)", description = "Создание приложений для iPhone и iPad. Изучение Swift и фреймворка SwiftUI.", price = 24000, is_active = true, created_at = DateTime.UtcNow, category_id = catMobile },
                new CourseDb { Id = Guid.NewGuid(), title = "Flutter: Кроссплатформа", description = "Пишем одно приложение, которое работает и на Android, и на iOS. Язык Dart.", price = 20000, is_active = true, created_at = DateTime.UtcNow, category_id = catMobile },

                // Маркетинг
                new CourseDb { Id = Guid.NewGuid(), title = "SMM-менеджер", description = "Продвижение брендов в социальных сетях. Таргетинг, контент-план и аналитика.", price = 18000, is_active = true, created_at = DateTime.UtcNow, category_id = catMarketing },
                new CourseDb { Id = Guid.NewGuid(), title = "Интернет-маркетолог", description = "Комплексное продвижение бизнеса в интернете. SEO, контекстная реклама и email-маркетинг.", price = 20000, is_active = true, created_at = DateTime.UtcNow, category_id = catMarketing },
                new CourseDb { Id = Guid.NewGuid(), title = "Управление проектами (PM)", description = "Agile, Scrum, Kanban. Как управлять командой и доводить проекты до релиза.", price = 25000, is_active = true, created_at = DateTime.UtcNow, category_id = catMarketing },

                // Кибербезопасность
                new CourseDb { Id = Guid.NewGuid(), title = "Белый хакер (Pentest)", description = "Основы тестирования на проникновение и защиты веб-ресурсов от атак.", price = 30000, is_active = true, created_at = DateTime.UtcNow, category_id = catSecurity },
                new CourseDb { Id = Guid.NewGuid(), title = "Сетевая безопасность", description = "Настройка Firewalls, VPN, защита корпоративных сетей и протоколы шифрования.", price = 28000, is_active = true, created_at = DateTime.UtcNow, category_id = catSecurity },

                // Психология
                new CourseDb { Id = Guid.NewGuid(), title = "Психология общения", description = "Как выстраивать эффективные коммуникации, разрешать конфликты и понимать собеседника.", price = 9000, is_active = true, created_at = DateTime.UtcNow, category_id = catPsychology },
                new CourseDb { Id = Guid.NewGuid(), title = "Когнитивная психология", description = "Как работает наш мозг: память, внимание и принятие решений.", price = 11000, is_active = true, created_at = DateTime.UtcNow, category_id = catPsychology },

                // Языки
                new CourseDb { Id = Guid.NewGuid(), title = "Английский для IT", description = "Специализированный курс: техническая документация и общение с заказчиками.", price = 11000, is_active = true, created_at = DateTime.UtcNow, category_id = catLanguages },
                new CourseDb { Id = Guid.NewGuid(), title = "Разговорный английский", description = "Преодоление языкового барьера. Интенсивная практика с носителями.", price = 15000, is_active = true, created_at = DateTime.UtcNow, category_id = catLanguages }
            };

            context.CourseDb.AddRange(courses);
            context.SaveChanges(); // Сохраняем, чтобы у курсов появились ID

            // --- 3. ЗАГРУЖАЕМ КАРТИНКИ В БАЗУ ---
            // Берем ваши картинки из папки wwwroot/img
            string[] imageFiles = { "course1.jpg", "course2.jpg", "course3.jpg", "course4.jpg" };
            int imgIndex = 0;
            var imagesToAdd = new List<CourseImageDb>();

            foreach (var course in courses)
            {
                // Берем картинку по кругу (1, 2, 3, 4, 1, 2...)
                string fileName = imageFiles[imgIndex % imageFiles.Length];

                // Читаем байты с диска
                byte[]? imgBytes = GetImageBytes(fileName);

                if (imgBytes != null)
                {
                    imagesToAdd.Add(new CourseImageDb
                    {
                        Id = Guid.NewGuid(),
                        CourseId = course.Id,
                        Data = imgBytes,
                        ContentType = "image/jpeg"
                    });
                }
                imgIndex++;
            }

            if (imagesToAdd.Any())
            {
                context.CourseImageDb.AddRange(imagesToAdd);
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при автоматическом заполнении базы данных.");
    }
}

// ==========================================
// 3. MIDDLEWARE PIPELINE
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