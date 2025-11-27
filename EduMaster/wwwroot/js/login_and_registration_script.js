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
    document.getElementById("form_signup").classList.add("hidden");

    document.querySelector(".toggle-btn:nth-child(2)").classList.add("active");
    document.querySelector(".toggle-btn:nth-child(3)").classList.remove("active");

    document.getElementById("btn").style.left = "5px";
}

function switchToRegister() {
    document.getElementById("form_signin").classList.add("hidden");
    document.getElementById("form_signup").classList.remove("hidden");

    document.querySelector(".toggle-btn:nth-child(2)").classList.remove("active");
    document.querySelector(".toggle-btn:nth-child(3)").classList.add("active");

    document.getElementById("btn").style.left = "calc(50% + 5px)";
}

/* ===================== AJAX — РЕГИСТРАЦИЯ ===================== */

const formSignup = document.getElementById("form_signup");

if (formSignup) {
    formSignup.addEventListener("submit", async function (e) {
        e.preventDefault();

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

            if (!data.isSuccess) {
                if (errBox) {
                    data.errors.forEach(err => {
                        errBox.innerHTML += `<div>${err}</div>`;
                    });
                } else {
                    alert(data.errors.join("\n"));
                }
                return;
            }

            // УСПЕХ: Переходим в личный кабинет
            window.location.href = '/Home/Dashboard';

        } catch (error) {
            console.error("Ошибка при регистрации:", error);
            alert("Произошла ошибка соединения с сервером.");
        }
    });
}

/* ===================== AJAX — ВХОД ===================== */

const formSignin = document.getElementById("form_signin");

if (formSignin) {
    formSignin.addEventListener("submit", async function (e) {
        e.preventDefault();

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
                if (errBox) {
                    data.errors.forEach(err => {
                        errBox.innerHTML += `<div>${err}</div>`;
                    });
                } else {
                    alert(data.errors.join("\n"));
                }
                return;
            }

            // УСПЕХ: Переходим в личный кабинет
            window.location.href = '/Home/Dashboard';

        } catch (error) {
            console.error("Ошибка при входе:", error);
            alert("Произошла ошибка соединения с сервером.");
        }
    });
}