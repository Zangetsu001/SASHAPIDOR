document.getElementById("messageForm")?.addEventListener("submit", async (e) => {
    e.preventDefault();

    const data = {
        name: document.getElementById("name").value,
        email: document.getElementById("email").value,
        subject: document.getElementById("subject").value,
        message: document.getElementById("message").value
    };

    const response = await fetch("/Home/SendMessage", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });

    const result = await response.json();

    const msg = document.getElementById("responseMsg");
    msg.textContent = result.message;
    msg.style.color = result.success ? "green" : "red";

    if (result.success) {
        document.getElementById("messageForm").reset();
    }
});
