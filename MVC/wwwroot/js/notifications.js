(function () {
    if (!window.signalR) {
        console.warn("SignalR client not loaded; notifications disabled.");
        return;
    }

    const container = document.getElementById("toast-container");
    if (!container) return;

    const levelToClass = {
        info: "text-bg-info",
        success: "text-bg-success",
        warning: "text-bg-warning",
        danger: "text-bg-danger"
    };

    function showToast(dto) {
        const cls = levelToClass[dto.level] || levelToClass.info;
        const el = document.createElement("div");
        el.className = `toast ${cls} border-0`;
        el.setAttribute("role", "alert");
        el.setAttribute("aria-live", "assertive");
        el.setAttribute("aria-atomic", "true");
        el.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${escapeHtml(dto.title)}</strong><br>
                    ${escapeHtml(dto.message)}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>`;
        container.appendChild(el);

        const toast = new bootstrap.Toast(el, { delay: 6000 });
        toast.show();
        el.addEventListener("hidden.bs.toast", () => el.remove());
    }

    function escapeHtml(s) {
        return String(s ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("notify", showToast);

    connection.start().catch(err => console.error("SignalR connect failed:", err));
})();
