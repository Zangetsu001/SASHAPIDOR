document.addEventListener('DOMContentLoaded', function () {

    // --- 1. Логика кнопки "Подробнее" ---
    const detailButtons = document.querySelectorAll('.btn-details');

    detailButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            const modalElement = document.getElementById('courseDetailsModal');

            // Если модалка не найдена в DOM - выходим
            if (!modalElement) return;

            // Запрос на получение данных о курсе
            fetch('/Courses/GetCourseDetails?id=' + id)
                .then(response => {
                    if (!response.ok) throw new Error('Ошибка загрузки данных');
                    return response.json();
                })
                .then(data => {
                    // Заполняем модальное окно данными
                    document.getElementById('modalCourseTitle').innerText = data.title;
                    document.getElementById('modalCourseDesc').innerText = data.description;
                    document.getElementById('modalCoursePrice').innerText = new Intl.NumberFormat('ru-RU').format(data.price) + ' ₽';

                    const catBadge = document.getElementById('modalCourseCategory');
                    if (catBadge) catBadge.innerText = data.category;

                    // Показываем окно через Bootstrap API
                    const modal = new bootstrap.Modal(modalElement);
                    modal.show();
                })
                .catch(error => {
                    console.error('Ошибка:', error);
                    alert('Не удалось загрузить информацию о курсе.');
                });
        });
    });


    // --- 2. Логика кнопки "Записаться" ---
    const enrollButtons = document.querySelectorAll('.btn-enroll');

    enrollButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            // Если у кнопки есть атрибут onclick (например, openModal), 
            // значит пользователь не авторизован -> ничего не делаем, пусть работает onclick
            if (this.getAttribute('onclick')) return;

            e.preventDefault();
            const id = this.getAttribute('data-id');
            const originalText = this.innerText;

            // Визуальный эффект загрузки
            this.innerText = '...';
            this.disabled = true;

            fetch('/Home/AddCourse?id=' + id, { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert('Вы успешно записались! Курс доступен в личном кабинете.');
                        this.innerText = 'Записан ✓';
                        this.style.backgroundColor = '#6c757d'; // Серый цвет
                    } else {
                        alert('Произошла ошибка при записи.');
                        this.innerText = originalText;
                        this.disabled = false;
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert('Ошибка сети.');
                    this.innerText = originalText;
                    this.disabled = false;
                });
        });
    });

});