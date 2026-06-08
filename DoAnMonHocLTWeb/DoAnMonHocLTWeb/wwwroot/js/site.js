// Site-wide JavaScript
$(document).ready(function () {
    // Sticky header on scroll
    let lastScroll = 0;
    const topBarHeight = $('.top-bar').outerHeight() || 0;

    $(window).scroll(function () {
        let currentScroll = $(this).scrollTop();
        if (currentScroll > 150) {
            $('.main-header').addClass('sticky-header');
        } else {
            $('.main-header').removeClass('sticky-header');
        }
    });

    // Add to cart animation
    $('.add-to-cart-btn').click(function (e) {
        e.preventDefault();
        let button = $(this);
        let originalText = button.html();

        button.html('<i class="fas fa-spinner fa-spin"></i> Adding...');
        button.prop('disabled', true);

        setTimeout(function () {
            button.html('<i class="fas fa-check"></i> Added!');
            setTimeout(function () {
                button.html(originalText);
                button.prop('disabled', false);
            }, 2000);
        }, 800);
    });

    // Update cart count function
    window.updateCartCount = function (count) {
        $('#cartCount').text(count);
        if (count > 0) {
            $('#cartCount').show();
        } else {
            $('#cartCount').hide();
        }
    };

    // Newsletter form submit
    $('.newsletter-form').submit(function (e) {
        e.preventDefault();
        let email = $(this).find('input[type="email"]').val();
        if (email) {
            alert('Cảm ơn bạn đã đăng ký nhận tin!');
            $(this).find('input[type="email"]').val('');
        }
    });

    // Countdown timer (nếu có)
    function startCountdown(days, hours, minutes, seconds) {
        let targetDate = new Date();
        targetDate.setDate(targetDate.getDate() + days);
        targetDate.setHours(targetDate.getHours() + hours);
        targetDate.setMinutes(targetDate.getMinutes() + minutes);
        targetDate.setSeconds(targetDate.getSeconds() + seconds);

        function updateCountdown() {
            let now = new Date();
            let diff = targetDate - now;

            if (diff <= 0) {
                $('.days').text('00');
                $('.hours').text('00');
                $('.minutes').text('00');
                $('.seconds').text('00');
                return;
            }

            let d = Math.floor(diff / (1000 * 60 * 60 * 24));
            let h = Math.floor((diff % (86400000)) / (1000 * 60 * 60));
            let m = Math.floor((diff % (3600000)) / (1000 * 60));
            let s = Math.floor((diff % (60000)) / 1000);

            $('.days').text(String(d).padStart(2, '0'));
            $('.hours').text(String(h).padStart(2, '0'));
            $('.minutes').text(String(m).padStart(2, '0'));
            $('.seconds').text(String(s).padStart(2, '0'));
        }

        updateCountdown();
        setInterval(updateCountdown, 1000);
    }

    // Khởi động countdown nếu có timer (30 days from now)
    if ($('.countdown-timer').length) {
        startCountdown(30, 0, 0, 0);
    }

    // Tooltip initialization
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});