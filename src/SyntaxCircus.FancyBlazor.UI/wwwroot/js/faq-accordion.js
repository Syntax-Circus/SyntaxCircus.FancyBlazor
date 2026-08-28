const instances = new WeakMap();

function prefersReducedMotion() {
    return typeof window.matchMedia === "function" && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

function setExpanded(trigger, panel, expanded, animated) {
    trigger.setAttribute("aria-expanded", expanded ? "true" : "false");

    if (!animated || prefersReducedMotion()) {
        panel.style.height = "";
        panel.hidden = !expanded;
        return;
    }

    if (expanded) {
        panel.hidden = false;
        const target = panel.scrollHeight;
        panel.style.height = "0px";
        void panel.offsetHeight;
        panel.style.height = `${target}px`;
        panel.addEventListener("transitionend", () => {
            panel.style.height = "";
        }, { once: true });
    } else {
        const current = panel.scrollHeight;
        panel.style.height = `${current}px`;
        void panel.offsetHeight;
        panel.style.height = "0px";
        panel.addEventListener("transitionend", () => {
            panel.hidden = true;
            panel.style.height = "";
        }, { once: true });
    }
}

export function create(root, options) {
    if (!root || instances.has(root)) {
        return;
    }

    const singleOpen = !options || options.singleOpen !== false;
    const animated = Boolean(options && options.animated);
    const triggers = Array.from(root.querySelectorAll("[data-faq-trigger]"));

    function onClick(event) {
        const trigger = event.currentTarget;
        const panel = document.getElementById(trigger.getAttribute("aria-controls"));
        if (!panel) {
            return;
        }

        const expanded = trigger.getAttribute("aria-expanded") === "true";
        if (singleOpen && !expanded) {
            for (const other of triggers) {
                if (other === trigger || other.getAttribute("aria-expanded") !== "true") {
                    continue;
                }
                const otherPanel = document.getElementById(other.getAttribute("aria-controls"));
                if (otherPanel) {
                    setExpanded(other, otherPanel, false, animated);
                }
            }
        }

        setExpanded(trigger, panel, !expanded, animated);
    }

    for (const trigger of triggers) {
        trigger.addEventListener("click", onClick);
    }

    instances.set(root, { triggers, onClick });
}

export function destroy(root) {
    const instance = root && instances.get(root);
    if (!instance) {
        return;
    }

    for (const trigger of instance.triggers) {
        trigger.removeEventListener("click", instance.onClick);
    }

    instances.delete(root);
}
