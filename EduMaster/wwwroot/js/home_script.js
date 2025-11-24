document.addEventListener("DOMContentLoaded", () => {
    // плавная прокрутка по якорям
    document.querySelectorAll('.bottom-nav a[href^="#"]').forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute("href"));
            if (target) {
                const y = target.getBoundingClientRect().top + window.scrollY - 50;
                window.scrollTo({ top: y, behavior: "smooth" });
            }
        });
    });

    // кнопка "наверх"
    const scrollBtn = document.createElement("button");
    scrollBtn.id = "scrollTopBtn";
    scrollBtn.textContent = "↑";
    document.body.appendChild(scrollBtn);

    window.addEventListener("scroll", () => {
        if (window.scrollY > 400) {
            scrollBtn.style.opacity = "1";
            scrollBtn.style.pointerEvents = "auto";
        } else {
            scrollBtn.style.opacity = "0";
            scrollBtn.style.pointerEvents = "none";
        }
    });

    scrollBtn.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
});
