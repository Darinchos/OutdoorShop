// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("submit", (event) => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) return;

    const button = form.querySelector("button[data-loading-label]");
    if (!(button instanceof HTMLButtonElement) || button.disabled) return;

    button.disabled = true;
    button.setAttribute("aria-busy", "true");
    button.innerHTML = `<span class="loading-spinner" aria-hidden="true"></span>${button.dataset.loadingLabel}`;
});

const siteHeader = document.querySelector(".site-header-home");
if (siteHeader) {
    const updateHeader = () => siteHeader.classList.toggle("is-scrolled", window.scrollY > 24);
    updateHeader();
    window.addEventListener("scroll", updateHeader, { passive: true });
}

document.querySelectorAll(".site-header-home .navbar-collapse .nav-link").forEach((link) => {
    link.addEventListener("click", () => {
        const menu = document.querySelector(".site-header-home .navbar-collapse.show");
        if (menu && window.bootstrap?.Collapse) window.bootstrap.Collapse.getOrCreateInstance(menu).hide();
    });
});

const heroVideo = document.querySelector(".hero-video");
if (heroVideo) {
    heroVideo.addEventListener("error", () => heroVideo.remove(), { once: true });
    const motionPreference = window.matchMedia("(prefers-reduced-motion: reduce)");
    if (motionPreference.matches) heroVideo.pause();
}

document.addEventListener("click", (event) => {
    const step = event.target.closest("[data-quantity-step]");
    if (!step) return;
    const input = step.closest(".quantity-control")?.querySelector("input[type='number']");
    if (!input) return;
    const min = Number(input.min) || 1;
    const max = Number(input.max) || Number.MAX_SAFE_INTEGER;
    const next = Math.max(min, Math.min(max, (Number(input.value) || min) + Number(step.dataset.quantityStep)));
    input.value = String(next);
    input.dispatchEvent(new Event("change", { bubbles: true }));
});

if ("IntersectionObserver" in window && !window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    const revealTargets = document.querySelectorAll(".home-discovery, .home-featured, .brand-story-copy, .promo-inner, .newsletter-section");
    revealTargets.forEach((element) => element.classList.add("reveal-pending"));
    const revealObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;
            entry.target.classList.add("is-revealed");
            observer.unobserve(entry.target);
        });
    }, { threshold: 0.12 });
    revealTargets.forEach((element) => revealObserver.observe(element));
}

const parallaxImage = document.querySelector(".brand-story-image");
if (parallaxImage && !window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    let parallaxFrame = 0;
    const updateParallax = () => {
        if (parallaxFrame) return;
        parallaxFrame = window.requestAnimationFrame(() => {
            const bounds = parallaxImage.getBoundingClientRect();
            const offset = Math.max(-12, Math.min(12, (bounds.top + bounds.height / 2 - window.innerHeight / 2) * -0.025));
            parallaxImage.style.setProperty("--parallax-offset", `${offset.toFixed(1)}px`);
            parallaxFrame = 0;
        });
    };
    window.addEventListener("scroll", updateParallax, { passive: true });
    updateParallax();
}
