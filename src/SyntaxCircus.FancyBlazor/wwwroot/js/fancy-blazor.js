import { createShaderTile } from './shader-gallery-renderer.js';

const instances = new Map();
let nextId = 1;
let visibilityListening = false;
let nacreSourcePromise;
let diagnosticsEnabled = false;

export async function createEffect(element, effect, options, defaults) {
    if (!element) throw new Error('FancyBlazor requires a rendered element.');

    const factory = factories[effect];
    if (!factory) throw new Error(`Unknown FancyBlazor effect: ${effect}`);

    diagnosticsEnabled ||= defaults?.enableDiagnostics === true;
    const instance = await factory(element, options ?? {}, defaults ?? {});
    const id = nextId++;
    instances.set(id, { effect, instance });
    ensureVisibilityListener();
    if (effect !== 'shader-background') setState(element, 'ready');
    updateDiagnostics();
    return id;
}

export function updateEffect(id, options) {
    instances.get(id)?.instance.update(options ?? {});
}

export function destroyEffect(id) {
    const entry = instances.get(id);
    if (!entry) return;
    entry.instance.destroy();
    instances.delete(id);
    releaseVisibilityListenerIfIdle();
    updateDiagnostics();
}

export function disposeRuntime() {
    for (const entry of instances.values()) entry.instance.destroy();
    instances.clear();
    releaseVisibilityListenerIfIdle();
    diagnosticsEnabled = false;
    updateDiagnostics();
}

// Unsupported test/diagnostic hook. It is intentionally absent from the .NET API.
export function getDiagnostics() {
    return {
        instanceCount: instances.size,
        effects: [...instances.values()].map(entry => entry.effect),
        animationFrameCount: [...instances.values()].filter(entry => entry.instance.hasActiveAnimationFrame?.()).length,
        documentVisible: document.visibilityState !== 'hidden',
    };
}

const factories = {
    'shader-background': createShaderBackground,
    reveal: createReveal,
    tilt: createTilt,
    spotlight: createSpotlight,
    magnetic: createMagnetic,
    parallax: createParallax,
    stagger: createStagger,
};

async function createShaderBackground(element, initialOptions, defaults) {
    const canvas = element.querySelector('.syntax-circus-fancy-shader-background__canvas');
    if (!(canvas instanceof HTMLCanvasElement)) throw new Error('ShaderBackground canvas not found.');

    const fragmentSource = await loadNacreSource();
    let options = initialOptions;
    let tile = null;
    let destroyed = false;
    let media = null;
    let mediaHandler = null;

    const reduced = () => motionReduced(defaults.motionPreference, media);

    const build = () => {
        if (destroyed || tile || reduced() || (defaults.pauseWhenHidden !== false && document.hidden)) {
            setState(element, reduced() ? 'reduced' : 'fallback');
            return;
        }
        if (globalThis.__syntaxCircusFancyBlazorDisableWebGl) throw new Error('WebGL disabled by diagnostics.');

        tile = createShaderTile(canvas, fragmentSource, {
            palette: resolvePalette(options.palette),
            uniforms: shaderUniforms(options),
            post: { grain: 0.05, aberration: 0.004, vignette: 0.3, saturation: 1.12 },
            maxDpr: qualityDpr(options.quality ?? defaults.quality),
            pauseWhenOffscreen: defaults.pauseWhenOffscreen !== false,
        });
        setState(element, 'active');
    };

    const teardown = () => {
        tile?.destroy();
        tile = null;
    };

    if (defaults.motionPreference === 'RespectSystem') {
        media = matchMedia('(prefers-reduced-motion: reduce)');
        mediaHandler = () => {
            teardown();
            if (!reduced()) safely(build, element);
            else setState(element, 'reduced');
        };
        media.addEventListener('change', mediaHandler);
    }

    safely(build, element);

    return {
        update(next) {
            const priorQuality = options.quality;
            options = next;
            if (!tile) {
                safely(build, element);
                return;
            }
            if (priorQuality !== options.quality) {
                teardown();
                safely(build, element);
                return;
            }
            tile.setPalette(resolvePalette(options.palette));
            const uniforms = shaderUniforms(options);
            for (const [name, value] of Object.entries(uniforms)) tile.setUniform(name, value);
        },
        setDocumentVisible(visible) {
            if (defaults.pauseWhenHidden === false) return;
            if (visible) safely(build, element);
            else {
                teardown();
                setState(element, 'paused');
            }
        },
        hasActiveAnimationFrame() { return tile?.isRunning?.() ?? false; },
        destroy() {
            destroyed = true;
            teardown();
            if (media && mediaHandler) media.removeEventListener('change', mediaHandler);
            setState(element, 'disposed');
        },
    };
}

function createReveal(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let observeFrame = null;
    let media = null;
    let mediaHandler = null;
    let destroyed = false;

    const configure = () => {
        observer?.disconnect();
        observer = null;
        element.dataset.fancyReady = 'true';

        if (motionReduced(defaults.motionPreference, media)) {
            element.dataset.fancyVisible = 'true';
            return;
        }

        element.dataset.fancyVisible = 'false';
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                element.dataset.fancyVisible = entry.isIntersecting ? 'true' : 'false';
                if (entry.isIntersecting && options.once !== false) {
                    observer?.disconnect();
                    observer = null;
                }
            }
        }, { threshold: 0.1 });
        observeFrame = requestAnimationFrame(() => {
            observeFrame = null;
            observer?.observe(element);
        });
    };

    if (defaults.motionPreference === 'RespectSystem') {
        media = matchMedia('(prefers-reduced-motion: reduce)');
        mediaHandler = () => { if (!destroyed) configure(); };
        media.addEventListener('change', mediaHandler);
    }

    configure();
    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return false; },
        destroy() {
            destroyed = true;
            if (observeFrame !== null) cancelAnimationFrame(observeFrame);
            observer?.disconnect();
            if (media && mediaHandler) media.removeEventListener('change', mediaHandler);
            delete element.dataset.fancyReady;
            delete element.dataset.fancyVisible;
        },
    };
}

function createTilt(element, initialOptions, defaults) {
    let options = initialOptions;
    let frame = null;
    let media = defaults.motionPreference === 'RespectSystem'
        ? matchMedia('(prefers-reduced-motion: reduce)')
        : null;
    let active = false;

    const reset = () => {
        if (frame !== null) cancelAnimationFrame(frame);
        frame = null;
        element.style.removeProperty('--sc-fancy-tilt-x');
        element.style.removeProperty('--sc-fancy-tilt-y');
        element.style.removeProperty('--sc-fancy-tilt-scale');
        element.style.removeProperty('--sc-fancy-glare-x');
        element.style.removeProperty('--sc-fancy-glare-y');
        delete element.dataset.fancyEngaged;
    };

    const onMove = event => {
        if (motionReduced(defaults.motionPreference, media)) return;
        if (frame !== null) cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            const rect = element.getBoundingClientRect();
            const x = clamp((event.clientX - rect.left) / Math.max(rect.width, 1), 0, 1);
            const y = clamp((event.clientY - rect.top) / Math.max(rect.height, 1), 0, 1);
            const maxAngle = clamp(options.maxAngle, 0, 45);
            element.style.setProperty('--sc-fancy-tilt-x', `${(0.5 - y) * maxAngle * 2}deg`);
            element.style.setProperty('--sc-fancy-tilt-y', `${(x - 0.5) * maxAngle * 2}deg`);
            element.style.setProperty('--sc-fancy-tilt-scale', clamp(options.scale, 0.8, 1.25));
            element.style.setProperty('--sc-fancy-glare-x', `${x * 100}%`);
            element.style.setProperty('--sc-fancy-glare-y', `${y * 100}%`);
            element.dataset.fancyEngaged = 'true';
            frame = null;
        });
    };

    const enable = () => {
        if (active || motionReduced(defaults.motionPreference, media)) return;
        active = true;
        element.addEventListener('pointermove', onMove, { passive: true });
        element.addEventListener('pointerleave', reset, { passive: true });
    };
    const disable = () => {
        if (!active) return;
        active = false;
        element.removeEventListener('pointermove', onMove);
        element.removeEventListener('pointerleave', reset);
        reset();
    };
    const mediaHandler = () => motionReduced(defaults.motionPreference, media) ? disable() : enable();
    media?.addEventListener('change', mediaHandler);
    enable();

    return {
        update(next) { options = next; },
        setDocumentVisible(visible) { if (visible) enable(); else disable(); },
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() { disable(); media?.removeEventListener('change', mediaHandler); },
    };
}

function createSpotlight(element, initialOptions, defaults) {
    let frame = null;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const reset = () => { element.style.removeProperty('--sc-fancy-spotlight-x'); element.style.removeProperty('--sc-fancy-spotlight-y'); };
    const move = event => {
        if (motionReduced(defaults.motionPreference, media)) return;
        if (frame !== null) cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => { const rect = element.getBoundingClientRect(); element.style.setProperty('--sc-fancy-spotlight-x', `${clamp((event.clientX - rect.left) / Math.max(rect.width, 1), 0, 1) * 100}%`); element.style.setProperty('--sc-fancy-spotlight-y', `${clamp((event.clientY - rect.top) / Math.max(rect.height, 1), 0, 1) * 100}%`); frame = null; });
    };
    element.addEventListener('pointermove', move, { passive: true }); element.addEventListener('pointerleave', reset, { passive: true });
    return { update() {}, setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; }, destroy() { if (frame !== null) cancelAnimationFrame(frame); element.removeEventListener('pointermove', move); element.removeEventListener('pointerleave', reset); reset(); } };
}

function createMagnetic(element, initialOptions, defaults) {
    let options = initialOptions; let frame = null; let active = true;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const reset = () => { if (frame !== null) cancelAnimationFrame(frame); frame = null; element.style.removeProperty('--sc-fancy-magnetic-x'); element.style.removeProperty('--sc-fancy-magnetic-y'); delete element.dataset.fancyEngaged; };
    const move = event => { if (!active || motionReduced(defaults.motionPreference, media)) return; if (frame !== null) cancelAnimationFrame(frame); frame = requestAnimationFrame(() => { const rect = element.getBoundingClientRect(); const strength = clamp(options.strength, 0, 1); element.style.setProperty('--sc-fancy-magnetic-x', `${((event.clientX - (rect.left + rect.width / 2)) / Math.max(rect.width, 1)) * 30 * strength}px`); element.style.setProperty('--sc-fancy-magnetic-y', `${((event.clientY - (rect.top + rect.height / 2)) / Math.max(rect.height, 1)) * 30 * strength}px`); element.dataset.fancyEngaged = 'true'; frame = null; }); };
    element.addEventListener('pointermove', move, { passive: true }); element.addEventListener('pointerleave', reset, { passive: true });
    return { update(next) { options = next; }, setDocumentVisible(visible) { active = visible; if (!visible) reset(); }, hasActiveAnimationFrame() { return frame !== null; }, destroy() { element.removeEventListener('pointermove', move); element.removeEventListener('pointerleave', reset); reset(); } };
}

function createParallax(element, initialOptions, defaults) {
    let frame = null; let active = true;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const update = () => { if (!active || motionReduced(defaults.motionPreference, media)) return; if (frame !== null) return; frame = requestAnimationFrame(() => { const rect = element.getBoundingClientRect(); const distance = parseFloat(getComputedStyle(element).getPropertyValue('--sc-fancy-parallax-distance')) || 0; const offset = clamp((window.innerHeight / 2 - (rect.top + rect.height / 2)) / Math.max(window.innerHeight, 1), -1, 1) * distance; element.style.setProperty('--sc-fancy-parallax-y', `${offset}px`); element.dataset.fancyParallaxOffset = `${Math.round(offset)}`; frame = null; }); };
    addEventListener('scroll', update, { passive: true }); addEventListener('resize', update, { passive: true }); update();
    return { update, setDocumentVisible(visible) { active = visible; if (visible) update(); else { if (frame !== null) cancelAnimationFrame(frame); frame = null; } }, hasActiveAnimationFrame() { return frame !== null; }, destroy() { removeEventListener('scroll', update); removeEventListener('resize', update); if (frame !== null) cancelAnimationFrame(frame); element.style.removeProperty('--sc-fancy-parallax-y'); delete element.dataset.fancyParallaxOffset; } };
}

function createStagger(element, initialOptions, defaults) {
    let options = initialOptions; let observer = null; let frame = null; let timer = null;
    const configure = (replay = false) => { observer?.disconnect(); if (timer !== null) clearTimeout(timer); [...element.children].forEach((child, index) => child.style.setProperty('--sc-fancy-index', index)); element.dataset.fancyReady = 'true'; if (motionReduced(defaults.motionPreference)) { element.dataset.fancyVisible = 'true'; return; } element.dataset.fancyVisible = 'false'; observer = new IntersectionObserver(entries => entries.forEach(entry => { element.dataset.fancyVisible = entry.isIntersecting ? 'true' : 'false'; if (entry.isIntersecting && options.once !== false) observer?.disconnect(); }), { threshold: .1 }); const observe = () => { frame = requestAnimationFrame(() => { frame = requestAnimationFrame(() => { frame = null; observer?.observe(element); }); }); }; if (replay) timer = setTimeout(() => { timer = null; observe(); }, 300); else observe(); };
    configure(); return { update(next) { const replay = next.replayToken !== options.replayToken; options = next; configure(replay); }, setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; }, destroy() { if (frame !== null) cancelAnimationFrame(frame); if (timer !== null) clearTimeout(timer); observer?.disconnect(); [...element.children].forEach(child => child.style.removeProperty('--sc-fancy-index')); delete element.dataset.fancyReady; delete element.dataset.fancyVisible; } };
}

function ensureVisibilityListener() {
    if (visibilityListening) return;
    document.addEventListener('visibilitychange', onVisibilityChange);
    visibilityListening = true;
}

function releaseVisibilityListenerIfIdle() {
    if (!visibilityListening || instances.size !== 0) return;
    document.removeEventListener('visibilitychange', onVisibilityChange);
    visibilityListening = false;
}

function onVisibilityChange() {
    const visible = !document.hidden;
    for (const entry of instances.values()) entry.instance.setDocumentVisible?.(visible);
    updateDiagnostics();
}

function loadNacreSource() {
    nacreSourcePromise ??= fetch(new URL('../vendor/shader-gallery/nacre.frag', import.meta.url))
        .then(response => {
            if (!response.ok) throw new Error(`Nacre shader returned ${response.status}.`);
            return response.text();
        });
    return nacreSourcePromise;
}

function shaderUniforms(options) {
    const intensity = clamp(options.intensity, 0, 1);
    return {
        u_flow: clamp(options.speed, 0, 3) * 0.45,
        u_iridescence: 0.2 + intensity,
        u_bands: 0.6,
        u_size: 1,
        u_mouseInfluence: options.interactive ? 0.35 : 0,
    };
}

function qualityDpr(quality) {
    switch (quality) {
        case 'Low': return 1;
        case 'High': return 2;
        case 'Medium': return 1.5;
        default: return 1.5;
    }
}

function motionReduced(preference, media) {
    if (preference === 'AlwaysReduce') return true;
    if (preference === 'IgnoreSystem') return false;
    return media?.matches ?? matchMedia('(prefers-reduced-motion: reduce)').matches;
}

function resolvePalette(values) {
    const fallback = ['#5e82f6', '#a855f7', '#22d3ee', '#08111f'];
    return fallback.map((color, index) => parseCssColor(values?.[index] ?? color, color));
}

function parseCssColor(value, fallback) {
    const probe = document.createElement('span');
    probe.style.color = value;
    probe.style.display = 'none';
    document.body.appendChild(probe);
    let computed = getComputedStyle(probe).color;
    probe.remove();
    if (!computed) computed = fallback;
    const numbers = computed.match(/[\d.]+/g)?.slice(0, 3).map(Number);
    if (!numbers || numbers.length < 3) return parseCssColor(fallback, '#000000');
    return numbers.map(number => number / 255);
}

function safely(action, element) {
    try { action(); }
    catch (error) {
        console.warn('FancyBlazor effect fell back to CSS.', error);
        setState(element, 'fallback');
    }
}

function setState(element, state) {
    element.dataset.fancyState = state;
}

function clamp(value, minimum, maximum) {
    const number = Number(value);
    return Number.isFinite(number) ? Math.min(maximum, Math.max(minimum, number)) : minimum;
}

function updateDiagnostics() {
    if (!diagnosticsEnabled) {
        delete globalThis.__syntaxCircusFancyBlazor;
        return;
    }
    globalThis.__syntaxCircusFancyBlazor = {
        ...getDiagnostics(),
        getDiagnostics,
    };
}
