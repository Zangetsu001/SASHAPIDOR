document.addEventListener('DOMContentLoaded', function () {

    // === КНОПКА "ИНФО" (Открывает модальное окно) ===
    const detailButtons = document.querySelectorAll('.btn-details');
    detailButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            fetch('/Courses/GetCourseDetails?id=' + id)
                .then(res => res.json())
                .then(data => {
                    document.getElementById('modalCourseTitle').innerText = data.title;
                    document.getElementById('modalCourseDesc').innerText = data.description;
                    // Красивая цена
                    document.getElementById('modalCoursePrice').innerText = new Intl.NumberFormat('ru-RU').format(data.price) + ' ₽';
                    document.getElementById('modalCourseCategory').innerText = data.category;

                    // Открываем окно
                    new bootstrap.Modal(document.getElementById('courseDetailsModal')).show();
                });
        });
    });

    // === КНОПКА "ЗАПИСАТЬСЯ" ===
    const enrollButtons = document.querySelectorAll('.btn-enroll');
    enrollButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {

            // Если это кнопка "Вход" (для гостей), не мешаем ей работать
            if (this.getAttribute('onclick')) return;

            e.preventDefault();
            const id = this.getAttribute('data-id');
            const btnElement = this;
            const originalText = btnElement.innerText;

            // 1. Показываем, что процесс идет
            btnElement.innerText = '...';
            btnElement.disabled = true;

            // 2. Отправляем запрос на сервер
            fetch('/Home/AddCourse?id=' + id, { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert('Курс успешно добавлен в Личный кабинет!');
                        btnElement.innerText = 'В кабинете ✓';
                        btnElement.style.backgroundColor = '#28a745'; // Зеленый
                        btnElement.style.borderColor = '#28a745';
                        btnElement.style.color = 'white';
                    } else {
                        alert('Ошибка при записи.');
                        btnElement.innerText = originalText;
                        btnElement.disabled = false;
                    }
                })
                .catch(err => {
                    console.error(err);
                    // Скорее всего пользователь не вошел в систему
                    alert('Ошибка. Убедитесь, что вы вошли в аккаунт.');
                    btnElement.innerText = originalText;
                    btnElement.disabled = false;
                });
        });
    });
});