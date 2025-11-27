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
                headerTop.style.backgroundColor = '#0066cc'; // Используем ваш синий цвет
                headerTop.style.borderBottom = '1px solid rgba(255,255,255,0.1)';
                headerTop.classList.add('scrolled');
            } else {
                headerTop.style.backgroundColor = 'rgba(0, 102, 204, 0.98)'; // Возвращаем полупрозрачность
                headerTop.style.borderBottom = 'none';
                headerTop.classList.remove('scrolled');
            }
        }

        // 2. Логика кнопки "Наверх"
        if (scrollTopBtn) {
            if (scrollPosition > 300) { // Показываем кнопку после 300px прокрутки
                scrollTopBtn.style.display = "block";
            } else {
                scrollTopBtn.style.display = "none";
            }
        }
    });
});

// Функция клика по кнопке (глобальная область видимости)
function topFunction() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}