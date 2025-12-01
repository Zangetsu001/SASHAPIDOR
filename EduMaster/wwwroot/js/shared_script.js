document.addEventListener('DOMContentLoaded', function () {
    const headerTop = document.querySelector('.header-top');
    const headerTopHeight = headerTop ? headerTop.offsetHeight : 0;
    const scrollTopBtn = document.getElementById("scrollTopBtn");

    // Обработчик скролла
    window.addEventListener('scroll', function () {
        const scrollPosition = window.scrollY;

        // 1. Логика шапки
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
            scrollTopBtn.style.display = scrollPosition > 300 ? "block" : "none";
        }
    });
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
    fetch("/Home/Dashboard") // <-- Исправлено на Dashboard
        .then(r => r.text())
        .then(html => {
            document.getElementById("dashboard-content").innerHTML = html;
            document.getElementById("dashboard-modal").style.display = "flex";
        });
}

// Закрыть модалку
function closeDashboard() {
    const modal = document.getElementById("dashboard-modal");

    if (modal) {
        modal.style.display = "none";
    }
}

// Загрузка partial через AJAX
function loadDashboardData() {
    fetch("/Home/Dashboard")
        .then(r => r.text())
        .then(html => {
            const content = document.getElementById("dashboard-content");
            if (content) content.innerHTML = html;
        })
        .catch(err => console.error("Ошибка загрузки Dashboard:", err));
}

// === ДОБАВИТЬ КУРС ===
function addCourseToDashboard(id) {
    fetch("/Home/AddCourse?id=" + id, { method: "POST" })
        .then(r => r.json())
        .then(() => loadDashboardData());
}

// === УДАЛИТЬ КУРС ===
function removeCourse(id) {
    fetch("/Home/RemoveCourse?id=" + id, { method: "POST" })
        .then(r => r.json())
        .then(() => loadDashboardData());
}
