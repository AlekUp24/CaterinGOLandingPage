const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
const finePointer = window.matchMedia("(hover: hover) and (pointer: fine)");

if (!reducedMotion.matches && finePointer.matches) {
    document.querySelectorAll("[data-hero-phone]").forEach((hero) => {
        let frameRequested = false;
        let pointerX = 0;
        let pointerY = 0;

        const updatePosition = () => {
            hero.style.setProperty("--phone-x", `${pointerX * 7}px`);
            hero.style.setProperty("--phone-y", `${pointerY * 7}px`);
            hero.style.setProperty("--phone-rotate", `${pointerX * 0.75}deg`);
            hero.style.setProperty("--backdrop-x", `${pointerX * -5}px`);
            hero.style.setProperty("--backdrop-y", `${pointerY * -5}px`);
            frameRequested = false;
        };

        const requestUpdate = () => {
            if (!frameRequested) {
                frameRequested = true;
                window.requestAnimationFrame(updatePosition);
            }
        };

        hero.addEventListener("pointermove", (event) => {
            const bounds = hero.getBoundingClientRect();
            pointerX = Math.max(-1, Math.min(1, (event.clientX - bounds.left) / bounds.width * 2 - 1));
            pointerY = Math.max(-1, Math.min(1, (event.clientY - bounds.top) / bounds.height * 2 - 1));
            requestUpdate();
        });

        hero.addEventListener("pointerleave", () => {
            pointerX = 0;
            pointerY = 0;
            requestUpdate();
        });
    });
}
