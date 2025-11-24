/* ===================== ОТКРЫТИЕ / ЗАКРЫТИЕ МОДАЛКИ ===================== */

function openModal() {
    document.getElementById("login-registration-container").style.display = "flex";
}

function closeModal() {
    document.getElementById("login-registration-container").style.display = "none";
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

document.getElementById("form_signup").addEventListener("submit", async function (e) {
    e.preventDefault();

    let dto = {
        email: document.getElementById("register-email").value,
        login: document.getElementById("register-login").value,
        password: document.getElementById("register-password").value,
        passwordConfirm: document.getElementById("register-password-confirm").value
    };

    let res = await fetch("/Home/Register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto)
    });

    let data = await res.json();

    let errBox = document.getElementById("error-messages-register");
    errBox.innerHTML = "";

    if (!data.isSuccess) {
        data.errors.forEach(err => {
            errBox.innerHTML += `<div>${err}</div>`;
        });
        return;
    }

    // Регистрация успешна
    location.reload();
});

/* ===================== AJAX — ВХОД ===================== */

document.getElementById("form_signin").addEventListener("submit", async function (e) {
    e.preventDefault();

    let dto = {
        loginOrEmail: document.getElementById("login-email").value,
        password: document.getElementById("login-password").value
    };

    let res = await fetch("/Home/Login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto)
    });

    let data = await res.json();

    let errBox = document.getElementById("error-messages-login");
    errBox.innerHTML = "";

    if (!data.isSuccess) {
        data.errors.forEach(err => {
            errBox.innerHTML += `<div>${err}</div>`;
        });
        return;
    }

    // Успешный вход
    location.reload();
});
