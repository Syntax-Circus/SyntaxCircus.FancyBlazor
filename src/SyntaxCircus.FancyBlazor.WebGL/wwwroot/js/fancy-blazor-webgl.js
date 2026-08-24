const instances = new Map();
const waiting = [];
const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");
let nextHandle = 1;
let activeContexts = 0;
let threeLoaded = false;
let lastFailure = null;
let disposed = false;
let rendererObjectsCreated = 0;
let rendererObjectsDestroyed = 0;

function isReduced(defaults) {
    return defaults.motionPreference === "AlwaysReduce" ||
        (defaults.motionPreference !== "IgnoreSystem" && reducedMotion.matches);
}

function setState(instance, state) {
    instance.element.dataset.webglState = state;
}

function isEligible(instance) {
    return !disposed && !instance.destroyed && !instance.reduced && !instance.contextLost &&
        instance.visible && !document.hidden;
}

function removeWaiting(instance) {
    const index = waiting.indexOf(instance);
    if (index >= 0) {
        waiting.splice(index, 1);
    }
    instance.waiting = false;
}

function enqueue(instance) {
    if (!isEligible(instance) || instance.active || instance.waiting) {
        return;
    }

    instance.waiting = true;
    waiting.push(instance);
    setState(instance, "waiting");
    pump();
}

function destroyRenderer(instance, renderer = instance.renderer) {
    if (!renderer) {
        return;
    }

    instance.releasing = true;
    try {
        renderer.destroy();
    } finally {
        rendererObjectsDestroyed++;
        if (instance.renderer === renderer) {
            instance.renderer = null;
        }
        instance.releasing = false;
    }
}

async function pump() {
    while (!disposed && activeContexts < contextCap()) {
        const instance = waiting.shift();
        if (!instance) {
            return;
        }

        instance.waiting = false;
        if (!isEligible(instance)) {
            continue;
        }

        activeContexts++;
        instance.active = true;
        setState(instance, "loading");
        try {
            const constructionGate = globalThis.__syntaxCircusFancyBlazorWebGlConstructionGate;
            if (constructionGate && typeof constructionGate.then === "function") {
                await constructionGate;
            }

            if (globalThis.__syntaxCircusFancyBlazorWebGlForceFailure) {
                throw new Error("The test failure switch is enabled.");
            }

            const rendererModule = await import(new URL("./holographic-surface-renderer.js", import.meta.url).href);
            if (!isEligible(instance) || !instance.active) {
                release(instance, false);
                continue;
            }

            const renderer = await rendererModule.createHolographicSurface(instance.canvas, instance.options, instance.defaults);
            rendererObjectsCreated++;
            if (!isEligible(instance) || !instance.active || instances.get(instance.handle) !== instance) {
                destroyRenderer(instance, renderer);
                continue;
            }

            instance.renderer = renderer;
            threeLoaded = true;
            instance.renderer.start();
            setState(instance, "active");
        } catch (error) {
            lastFailure = String(error?.message || error);
            release(instance, false);
            setState(instance, "fallback");
        }
    }
}

function contextCap() {
    for (const instance of instances.values()) {
        return Math.max(1, Math.min(8, Number(instance.defaults.maxActiveContexts) || 4));
    }
    return 4;
}

function release(instance, requeue) {
    removeWaiting(instance);
    destroyRenderer(instance);
    if (instance.active) {
        instance.active = false;
        activeContexts = Math.max(0, activeContexts - 1);
    }
    if (!instance.destroyed && !instance.reduced && !instance.contextLost) {
        setState(instance, requeue ? "waiting" : "fallback");
    }
    if (requeue) {
        enqueue(instance);
    }
    void pump();
}

function recheck(instance) {
    if (isEligible(instance)) {
        enqueue(instance);
    } else if (instance.active) {
        release(instance, false);
    }
}

function getDiagnostics() {
    const detail = [...instances.values()].map(instance => ({
        handle: instance.handle,
        state: instance.element.dataset.webglState,
        intensity: instance.options.intensity,
        palette: instance.renderer?.getPalette() ?? instance.options.palette,
        active: instance.active,
        waiting: instance.waiting,
    }));
    return {
        instanceCount: instances.size,
        activeContexts,
        waitingContexts: waiting.length,
        animationFrameCount: [...instances.values()].filter(instance => instance.renderer?.hasFrame()).length,
        rendererObjectsCreated,
        rendererObjectsDestroyed,
        liveRendererCount: rendererObjectsCreated - rendererObjectsDestroyed,
        threeLoaded,
        lastFailure,
        instances: detail,
    };
}

function onVisibilityChange() {
    for (const instance of instances.values()) {
        if (document.hidden) {
            release(instance, false);
        } else {
            recheck(instance);
        }
    }
}

document.addEventListener("visibilitychange", onVisibilityChange);

export function createEffect(element, effect, options, defaults) {
    if (disposed || effect !== "holographic-surface") {
        return null;
    }

    const canvas = element.querySelector(".syntax-circus-fancy-holographic-surface__canvas");
    if (!(canvas instanceof HTMLCanvasElement)) {
        return null;
    }

    const handle = nextHandle++;
    const instance = {
        handle,
        element,
        canvas,
        options: { ...options },
        defaults: { ...defaults },
        active: false,
        waiting: false,
        destroyed: false,
        visible: false,
        reduced: isReduced(defaults),
        contextLost: false,
        renderer: null,
        releasing: false,
        observer: null,
        pointerMove: null,
        contextLostHandler: null,
        contextRestoredHandler: null,
    };

    instances.set(handle, instance);
    if (instance.reduced) {
        setState(instance, "reduced");
        return handle;
    }

    instance.observer = new IntersectionObserver(entries => {
        instance.visible = entries.some(entry => entry.isIntersecting);
        recheck(instance);
    }, { threshold: 0.01 });
    instance.observer.observe(element);

    if (options.interactive) {
        instance.pointerMove = event => {
            if (!matchMedia("(pointer: fine)").matches ||
                (event.pointerType && event.pointerType !== "mouse" && event.pointerType !== "pen")) {
                return;
            }
            const bounds = element.getBoundingClientRect();
            const x = bounds.width ? (event.clientX - bounds.left) / bounds.width : 0.5;
            const y = bounds.height ? (event.clientY - bounds.top) / bounds.height : 0.5;
            instance.renderer?.setPointer(x, y);
            element.dataset.webglPointer = "true";
        };
        element.addEventListener("pointermove", instance.pointerMove, { passive: true });
    }

    instance.contextLostHandler = event => {
        event.preventDefault();
        if (instance.releasing) {
            return;
        }
        instance.contextLost = true;
        release(instance, false);
        setState(instance, "fallback");
    };
    instance.contextRestoredHandler = () => {
        instance.contextLost = false;
        recheck(instance);
    };
    canvas.addEventListener("webglcontextlost", instance.contextLostHandler);
    canvas.addEventListener("webglcontextrestored", instance.contextRestoredHandler);
    return handle;
}

export function updateEffect(handle, options) {
    const instance = instances.get(handle);
    if (!instance || instance.destroyed) {
        return false;
    }

    instance.options = { ...instance.options, ...options };
    instance.renderer?.update(instance.options);
    return true;
}

export function destroyEffect(handle) {
    const instance = instances.get(handle);
    if (!instance) {
        return;
    }

    instance.destroyed = true;
    instance.observer?.disconnect();
    if (instance.pointerMove) {
        instance.element.removeEventListener("pointermove", instance.pointerMove);
    }
    instance.canvas.removeEventListener("webglcontextlost", instance.contextLostHandler);
    instance.canvas.removeEventListener("webglcontextrestored", instance.contextRestoredHandler);
    release(instance, false);
    instances.delete(handle);
}

export function disposeRuntime() {
    disposed = true;
    for (const handle of [...instances.keys()]) {
        destroyEffect(handle);
    }
    waiting.length = 0;
    document.removeEventListener("visibilitychange", onVisibilityChange);
}

export { getDiagnostics };

globalThis.__syntaxCircusFancyBlazorWebGl = { getDiagnostics };
