(function () {
    const tariff = document.getElementById('tariff-select');
    const hours = document.getElementById('hours-input');
    const cost = document.getElementById('summary-cost');

    function recalc() {
        const price = Number(tariff.options[tariff.selectedIndex].dataset.price || 0);
        const h = Math.max(1, Math.min(24, Number(hours.value || 1)));
        cost.textContent = (price * h).toFixed(0);
    }

    tariff.addEventListener('change', recalc);
    hours.addEventListener('input', recalc);
    recalc();
})();