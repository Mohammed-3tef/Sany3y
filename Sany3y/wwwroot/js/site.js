// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // --- Scroll to Top Button Logic ---
    var scrollTopBtn = $('#scrollTopBtn');

    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            scrollTopBtn.fadeIn();
        } else {
            scrollTopBtn.fadeOut();
        }
    });

    scrollTopBtn.click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });

    // --- Fade In Up Animation on Scroll ---
    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target); // Only animate once
            }
        });
    }, observerOptions);

    $('.fade-in-up').each(function () {
        observer.observe(this);
    });

    // --- Add hover effect class to all primary buttons ---
    $('.btn-primary, .btn-warning, .btn-success').addClass('btn-animate');
});
