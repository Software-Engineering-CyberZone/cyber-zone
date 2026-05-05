(function () {
    const tooltip = document.getElementById('pc-tooltip');
    const ttTitle = document.getElementById('tt-title');
    const ttMeta = document.getElementById('tt-meta');
    const ttBtn = document.getElementById('tt-btn');
    let activeEl = null;

    function hide() {
        tooltip.style.display = 'none';
        activeEl = null;
    }

    document.querySelectorAll('.pc-slot').forEach(function (node) {
        node.addEventListener('click', function (ev) {
            ev.stopPropagation();
            const rect = node.getBoundingClientRect();
            const containerRect = tooltip.parentElement.getBoundingClientRect();
            const pc = node.dataset.pc || '—';
            const status = node.dataset.status;
            const statusText = node.dataset.statusText;
            const price = node.dataset.price;
            const hwId = node.dataset.hardwareId;
            const clubId = node.dataset.clubId;

            ttTitle.textContent = 'ПК ' + pc;
            ttMeta.textContent = statusText + (price ? ' · від ' + price + ' грн/год' : '');

            if (status === 'Available' && hwId) {
                ttBtn.classList.remove('disabled');
                ttBtn.removeAttribute('aria-disabled');
                ttBtn.textContent = 'Забронювати';
                ttBtn.href = '/Booking/Create?clubId=' + clubId + '&hardwareId=' + hwId;
            } else {
                ttBtn.classList.add('disabled');
                ttBtn.setAttribute('aria-disabled', 'true');
                ttBtn.textContent = 'Недоступно';
                ttBtn.href = 'javascript:void(0)';
            }

            tooltip.style.display = 'block';
            const top = rect.top - containerRect.top - tooltip.offsetHeight - 10;
            const left = rect.left - containerRect.left + rect.width / 2 - tooltip.offsetWidth / 2;
            tooltip.style.top = Math.max(10, top) + 'px';
            tooltip.style.left = Math.max(10, left) + 'px';
            activeEl = node;
        });
    });

    document.addEventListener('click', function (ev) {
        if (!tooltip.contains(ev.target)) hide();
    });
    window.addEventListener('resize', hide);
})();