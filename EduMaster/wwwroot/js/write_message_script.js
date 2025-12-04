document.addEventListener("DOMContentLoaded", function () {
    const messageForm = document.getElementById("messageForm");
    const responseMsg = document.getElementById("responseMsg");

    if (messageForm) {
        messageForm.addEventListener("submit", async function (e) {
            e.preventDefault(); // Останавливаем стандартную отправку

            // Собираем данные. .value сработает даже для readonly полей
            const formData = {
                Name: document.getElementById("name").value,
                Email: document.getElementById("email").value,
                Subject: document.getElementById("subject").value,
                Message: document.getElementById("message").value
            };

            // Визуальная индикация
            const submitBtn = messageForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Отправка...";
            responseMsg.innerText = "";

            try {
                const response = await fetch("/Home/SendMessage", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json" // Обязательно указываем, что это JSON
                    },
                    body: JSON.stringify(formData)
                });

                const result = await response.json();

                if (result.success) {
                    responseMsg.style.color = "green";
                    responseMsg.innerText = result.message;

                    // Очищаем только текстовые поля (тему и сообщение)
                    document.getElementById("subject").value = "";
                    document.getElementById("message").value = "";
                } else {
                    responseMsg.style.color = "red";
                    responseMsg.innerText = result.message || "Ошибка при отправке.";
                }
            } catch (error) {
                console.error("Ошибка:", error);
                responseMsg.style.color = "red";
                responseMsg.innerText = "Ошибка соединения с сервером.";
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerText = "Отправить";
            }
        });
    }
});