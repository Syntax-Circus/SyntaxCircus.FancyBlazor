const instances = new WeakMap();

function setExpanded(trigger, panel, expanded) {
    trigger.setAttribute("aria-expanded", expanded ? "true" : "false");
    panel.hidden = !expanded;
}

export function create(root, options) {
    if (!root || instances.has(root)) {
        return;
    }

    const singleOpen = !options || options.singleOpen !== false;
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
                if (other === trigger) {
                    continue;
                }
                const otherPanel = document.getElementById(other.getAttribute("aria-controls"));
                if (otherPanel) {
                    setExpanded(other, otherPanel, false);
                }
            }
        }

        setExpanded(trigger, panel, !expanded);
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
