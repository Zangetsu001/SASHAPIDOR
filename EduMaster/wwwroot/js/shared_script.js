document.addEventListener('DOMContentLoaded', function () {
    const headerTop = document.querySelector('.header-top'); // Если используется где-то еще
    const headerTopHeight = headerTop ? headerTop.offsetHeight : 0;
    const scrollTopBtn = document.getElementById("scrollTopBtn");

    // Обработчик скролла
    window.addEventListener('scroll', function () {
        const scrollPosition = window.scrollY;

        // 1. Логика изменения цвета шапки (если есть header-top)
        if (headerTop) {
            if (scrollPosition > headerTopHeight) {
                headerTop.style.backgroundColor = '#0066cc';
                headerTop.style.borderBottom = '1px solid rgba(255,255,255,0.1)';
                headerTop.classList.add('scrolled');
            } else {
                headerTop.style.backgroundColor = 'rgba(0, 102, 204, 0.98)';
                headerTop.style.borderBottom = 'none';
                headerTop.classList.remove('scrolled');
            }
        }

        // 2. Логика кнопки "Наверх"
        if (scrollTopBtn) {
            scrollTopBtn.style.display = scrollPosition > 300 ? "flex" : "none";
        }
    });

    // === ЛОГИКА БУРГЕР МЕНЮ ===
    const burgerBtn = document.getElementById('burgerBtn');
    const headerMenu = document.getElementById('headerMenu');
    const navLinks = document.querySelectorAll('.nav-link-item'); // Ссылки внутри меню

    if (burgerBtn && headerMenu) {
        // Клик по бургеру
        burgerBtn.addEventListener('click', function (e) {
            e.stopPropagation(); // Остановка всплытия, чтобы клик по кнопке не считался кликом "вне"
            headerMenu.classList.toggle('active');

            // Смена иконки
            const icon = burgerBtn.querySelector('i');
            if (headerMenu.classList.contains('active')) {
                icon.classList.remove('fa-bars');
                icon.classList.add('fa-times');
            } else {
                icon.classList.remove('fa-times');
                icon.classList.add('fa-bars');
            }
        });

        // Закрываем меню при клике на любую ссылку навигации
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                closeMenu();
            });
        });

        // Закрываем меню при клике вне его области
        document.addEventListener('click', function (event) {
            const isClickInside = headerMenu.contains(event.target) || burgerBtn.contains(event.target);
            if (!isClickInside && headerMenu.classList.contains('active')) {
                closeMenu();
            }
        });

        function closeMenu() {
            headerMenu.classList.remove('active');
            const icon = burgerBtn.querySelector('i');
            icon.classList.remove('fa-times');
            icon.classList.add('fa-bars');
        }
    }
});

// === Кнопка "Наверх" ===
function topFunction() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}


// ====================================================================
//                     МОДАЛКА ЛИЧНОГО КАБИНЕТА
// ====================================================================

// Открыть модалку и загрузить данные
function openDashboard() {
    fetch("/Home/Dashboard")
        .then(r => r.text())
        .then(html => {
            const content = document.getElementById("dashboard-content");
            const modal = document.getElementById("dashboard-modal");
            if (content && modal) {
                content.innerHTML = html;
                modal.style.display = "flex";
            }
        })
        .catch(err => console.error("Ошибка загрузки Dashboard:", err));
}

// Закрыть модалку
function closeDashboard() {
    const modal = document.getElementById("dashboard-modal");
    if (modal) {
        modal.style.display = "none";
    }
}

// Загрузка partial через AJAX (для обновления данных внутри)
function loadDashboardData() {
    fetch("/Home/Dashboard")
        .then(r => r.text())
        .then(html => {
            const content = document.getElementById("dashboard-content");
            if (content) content.innerHTML = html;
        })
        .catch(err => console.error("Ошибка обновления Dashboard:", err));
}

// === ДОБАВИТЬ КУРС ===
function addCourseToDashboard(id) {
    fetch("/Home/AddCourse?id=" + id, { method: "POST" })
        .then(r => r.json())
        .then(() => loadDashboardData())
        .catch(err => console.error(err));
}

// === УДАЛИТЬ КУРС ===
function removeCourse(id) {
    fetch("/Home/RemoveCourse?id=" + id, { method: "POST" })
        .then(r => r.json())
        .then(() => loadDashboardData())
        .catch(err => console.error(err));
}