/* ===================== ОТКРЫТИЕ / ЗАКРЫТИЕ МОДАЛКИ ===================== */
function openModal() {
    const container = document.getElementById("login-registration-container");
    if (container) container.style.display = "flex";
}

function closeModal() {
    const container = document.getElementById("login-registration-container");
    if (container) container.style.display = "none";
}

/* ======================== ПЕРЕКЛЮЧЕНИЕ ВКЛАДОК ======================== */
function switchToLogin() {
    document.getElementById("form_signin").classList.remove("hidden");
    const formSignup = document.getElementById("form_signup");
    formSignup.classList.add("hidden");
    formSignup.style.display = "";

    const codeBlock = document.getElementById("codeVerificationBlock");
    if (codeBlock) codeBlock.style.display = "none";

    document.querySelector(".toggle-btn:nth-child(2)").classList.add("active");
    document.querySelector(".toggle-btn:nth-child(3)").classList.remove("active");
    document.getElementById("btn").style.left = "5px";
}

function switchToRegister() {
    document.getElementById("form_signin").classList.add("hidden");
    const formSignup = document.getElementById("form_signup");
    formSignup.classList.remove("hidden");
    formSignup.style.display = "";

    const codeBlock = document.getElementById("codeVerificationBlock");
    if (codeBlock) codeBlock.style.display = "none";

    document.querySelector(".toggle-btn:nth-child(2)").classList.remove("active");
    document.querySelector(".toggle-btn:nth-child(3)").classList.add("active");
    document.getElementById("btn").style.left = "calc(50% + 5px)";
}

/* ===================== AJAX — РЕГИСТРАЦИЯ (ИСПРАВЛЕННАЯ) ===================== */
const formSignup = document.getElementById("form_signup");

if (formSignup) {
    formSignup.addEventListener("submit", async function (e) {
        e.preventDefault();

        // 1. БЛОКИРУЕМ КНОПКУ, ЧТОБЫ НЕ БЫЛО 4 ПИСЬМА
        const submitBtn = document.getElementById("btnRegisterSubmit");
        const originalBtnText = submitBtn.innerText;
        submitBtn.disabled = true;
        submitBtn.innerText = "Отправка...";

        let dto = {
            email: document.getElementById("register-email").value,
            login: document.getElementById("register-login").value,
            password: document.getElementById("register-password").value,
            passwordConfirm: document.getElementById("register-password-confirm").value
        };

        try {
            let res = await fetch("/Home/Register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(dto)
            });

            let data = await res.json();
            let errBox = document.getElementById("error-messages-register");
            if (errBox) errBox.innerHTML = "";

            // Разблокируем кнопку, если ошибка
            if (!data.isSuccess) {
                submitBtn.disabled = false;
                submitBtn.innerText = originalBtnText;

                if (errBox) {
                    data.errors.forEach(err => {
                        errBox.innerHTML += `<div>${err}</div>`;
                    });
                } else {
                    alert(data.errors.join("\n"));
                }
                return;
            }

            // === УСПЕХ: ПОКАЗЫВАЕМ ВВОД КОДА ===
            if (data.requireCode) {
                formSignup.style.display = "none"; // Скрываем форму

                const codeBlock = document.getElementById("codeVerificationBlock");
                if (codeBlock) {
                    codeBlock.style.display = "block"; // Показываем блок кода

                    // Вставляем Email красиво
                    const emailSpan = document.getElementById("displayEmail");
                    if (emailSpan) emailSpan.innerText = data.email;
                }
                // Кнопку не разблокируем, так как форма уже скрыта
                return;
            }

            window.location.reload();

        } catch (error) {
            console.error("Ошибка:", error);
            submitBtn.disabled = false;
            submitBtn.innerText = originalBtnText;
            alert("Ошибка соединения с сервером.");
        }
    });
}

/* ===================== AJAX — ПОДТВЕРЖДЕНИЕ КОДА ===================== */
const btnConfirmCode = document.getElementById("btnConfirmCode");

if (btnConfirmCode) {
    btnConfirmCode.addEventListener("click", async function (e) {
        e.preventDefault();

        // Блокируем кнопку подтверждения
        btnConfirmCode.disabled = true;
        btnConfirmCode.innerText = "Проверка...";

        const email = document.getElementById("displayEmail").innerText;
        const code = document.getElementById("verificationCode").value;
        const errorBox = document.getElementById("codeError");

        if (errorBox) errorBox.innerText = "";

        if (!code) {
            if (errorBox) errorBox.innerText = "Введите код";
            btnConfirmCode.disabled = false;
            btnConfirmCode.innerText = "Подтвердить код";
            return;
        }

        try {
            let res = await fetch("/Home/ConfirmRegistration", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email: email, code: code })
            });

            let data = await res.json();

            if (data.isSuccess) {
                window.location.reload();
            } else {
                if (errorBox) errorBox.innerText = data.message || "Неверный код";
                btnConfirmCode.disabled = false;
                btnConfirmCode.innerText = "Подтвердить код";
            }
        } catch (error) {
            if (errorBox) errorBox.innerText = "Ошибка сети";
            btnConfirmCode.disabled = false;
            btnConfirmCode.innerText = "Подтвердить код";
        }
    });
}

/* ===================== AJAX — ВХОД (ТОЖЕ БЛОКИРУЕМ) ===================== */
const formSignin = document.getElementById("form_signin");

if (formSignin) {
    formSignin.addEventListener("submit", async function (e) {
        e.preventDefault();

        const submitBtn = document.getElementById("btnLoginSubmit");
        const originalBtnText = submitBtn.innerText;
        submitBtn.disabled = true;
        submitBtn.innerText = "Вход...";

        let dto = {
            loginOrEmail: document.getElementById("login-email").value,
            password: document.getElementById("login-password").value
        };

        try {
            let res = await fetch("/Home/Login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(dto)
            });

            let data = await res.json();
            let errBox = document.getElementById("error-messages-login");
            if (errBox) errBox.innerHTML = "";

            if (!data.isSuccess) {
                submitBtn.disabled = false;
                submitBtn.innerText = originalBtnText;

                if (errBox) {
                    data.errors.forEach(err => {
                        errBox.innerHTML += `<div>${err}</div>`;
                    });
                }
                return;
            }

            window.location.reload();

        } catch (error) {
            submitBtn.disabled = false;
            submitBtn.innerText = originalBtnText;
            alert("Ошибка соединения.");
        }
    });
}

/* ===================== GOOGLE LOGIN HANDLER ===================== */
function handleGoogleLogin(response) {
    console.log("Google Token:", response.credential);

    // Создаем данные формы для отправки на сервер
    const formData = new FormData();
    formData.append("credential", response.credential);

    // Отправляем токен на наш бэкенд
    fetch("/Home/GoogleLogin", {
        method: "POST",
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                window.location.reload();
            } else {
                const errBox = document.getElementById("error-messages-login");
                if (errBox) errBox.innerText = data.message;
                else alert(data.message);
            }
        })
        .catch(err => {
            console.error("Ошибка Google Auth:", err);
            alert("Ошибка соединения с сервером.");
        });
}