// ── Mobile Nav ──────────────────────────────────────────────────────────────
(function () {
    const burger   = document.getElementById('nav-burger');
    const drawer   = document.getElementById('mobile-nav');
    const navLinks = drawer ? drawer.querySelectorAll('a, button[type="submit"]') : [];

    if (!burger || !drawer) return;

    function openNav() {
        burger.classList.add('is-open');
        drawer.classList.add('is-open');
        document.body.classList.add('nav-open');
        burger.setAttribute('aria-expanded', 'true');
    }

    function closeNav() {
        burger.classList.remove('is-open');
        drawer.classList.remove('is-open');
        document.body.classList.remove('nav-open');
        burger.setAttribute('aria-expanded', 'false');
    }

    function toggleNav() {
        burger.classList.contains('is-open') ? closeNav() : openNav();
    }

    burger.addEventListener('click', toggleNav);

    // Close when any nav link is clicked
    navLinks.forEach(function (el) {
        el.addEventListener('click', closeNav);
    });

    // Close on Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeNav();
    });
})();

// ── Scroll Reveal (IntersectionObserver) ────────────────────────────────────
(function () {
    var elements = document.querySelectorAll('[data-reveal]');
    if (!elements.length || !('IntersectionObserver' in window)) {
        // Fallback: just make them all visible
        elements.forEach(function (el) { el.classList.add('is-visible'); });
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    elements.forEach(function (el) { observer.observe(el); });
})();

// ── Filter Sidebar Toggle (Cars/Index) ──────────────────────────────────────
(function () {
    var btn     = document.getElementById('filter-toggle-btn');
    var sidebar = document.getElementById('filter-sidebar');
    var icon    = document.getElementById('filter-toggle-icon');

    if (!btn || !sidebar) return;

    btn.addEventListener('click', function () {
        var isOpen = sidebar.classList.toggle('is-open');
        if (icon) icon.textContent = isOpen ? 'expand_less' : 'expand_more';
        btn.setAttribute('aria-expanded', String(isOpen));
    });
})();
