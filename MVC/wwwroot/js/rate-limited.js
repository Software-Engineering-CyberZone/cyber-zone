(function () {
    const el = document.getElementById('rl-seconds');
    let remaining = Number(el.textContent) || 60;
    const timer = setInterval(() => {
        remaining -= 1;
        if (remaining <= 0) {
            clearInterval(timer);
            el.textContent = '0';
            return;
        }
        el.textContent = remaining;
    }, 1000);
})();