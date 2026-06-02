// Sidebar toggle
document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                sidebar.classList.toggle('mobile-open');
            } else {
                sidebar.classList.toggle('collapsed');
                content.style.marginLeft =
                    sidebar.classList.contains('collapsed') ? '0' : '260px';
            }
        });
    }
});

// Dark mode
const darkModeKey = 'apartman_dark_mode';

function applyDarkMode(isDark) {
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
    const icon = document.getElementById('darkModeIcon');
    if (icon) {
        icon.className = isDark
            ? 'fas fa-sun'
            : 'fas fa-moon';
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar toggle
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                sidebar.classList.toggle('mobile-open');
            } else {
                sidebar.classList.toggle('collapsed');
                content.style.marginLeft =
                    sidebar.classList.contains('collapsed') ? '0' : '260px';
            }
        });
    }

    // Dark mode başlangıç
    const savedDark = localStorage.getItem(darkModeKey) === 'true';
    applyDarkMode(savedDark);

    // Dark mode toggle butonu
    const darkBtn = document.getElementById('darkModeToggle');
    if (darkBtn) {
        darkBtn.addEventListener('click', function () {
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
            applyDarkMode(!isDark);
            localStorage.setItem(darkModeKey, !isDark);
        });
    }
});