setInterval(function () {
    var timers = document.querySelectorAll('.js-session-timer');

    timers.forEach(function (timer) {
        var targetTimeStr = timer.getAttribute('data-target-time');
        var startTimeStr = timer.getAttribute('data-start-time');
        var now = new Date();

        var diffMs = 0;

        if (targetTimeStr) {
            var targetTime = new Date(targetTimeStr);
            diffMs = targetTime - now;

            if (diffMs <= 0) {
                timer.textContent = "00:00:00";
                window.location.reload();
                return;
            }
        } else {
            var startTime = new Date(startTimeStr);
            diffMs = now - startTime;
            if (diffMs < 0) diffMs = 0;
        }

        var hours = Math.floor(diffMs / (1000 * 60 * 60));
        var minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
        var seconds = Math.floor((diffMs % (1000 * 60)) / 1000);

        var formattedTime =
            String(hours).padStart(2, '0') + ':' +
            String(minutes).padStart(2, '0') + ':' +
            String(seconds).padStart(2, '0');

        timer.textContent = formattedTime;
    });
}, 1000);

function toggleReviewForm(sessionId) {
    var form = document.getElementById('review-form-' + sessionId);
    if (form.style.display === 'none') {
        form.style.display = 'block';
    } else {
        form.style.display = 'none';
    }
}

function setRating(sessionId, value) {
    document.getElementById('rating-' + sessionId).value = value;
    var stars = document.querySelectorAll('#stars-' + sessionId + ' .star-btn');
    stars.forEach(function (star, index) {
        if (index < value) {
            star.classList.add('star-active');
        } else {
            star.classList.remove('star-active');
        }
    });
}