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
    'text-reveal': createTextReveal,
    ripple: createRipple,
    'cursor-trail': createCursorTrail,
    'constellation-background': (element, options, defaults) => createCanvasAtmosphere(element, options, defaults, 'constellation'),
    'arc-flow-background': (element, options, defaults) => createCanvasAtmosphere(element, options, defaults, 'arc-flow'),
    'flicker-grid': (element, options, defaults) => createCanvasAtmosphere(element, options, defaults, 'flicker-grid'),
    'meteor-background': (element, options, defaults) => createCanvasAtmosphere(element, options, defaults, 'meteor'),
    'light-rays-background': (element, options, defaults) => createCanvasAtmosphere(element, options, defaults, 'light-rays'),
    'type-flow': createTextReveal,
    'scramble-text': createScrambleText,
    marquee: createMarquee,
    'number-ticker': createNumberTicker,
    'scroll-scene': createScrollProgressEffect,
    'scroll-indicator': createScrollProgressEffect,
    'scroll-backdrop': createScrollProgressEffect,
    'press-scale': createPressScale,
    'word-rotate': createWordRotate,
    'morph-text': createMorphText,
    typewriter: createTypewriter,
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

function createScrollProgressEffect(element, initialOptions, defaults) {
    let options = initialOptions;
    let frame = null;
    let intersecting = false;
    let documentVisible = !document.hidden;
    let destroyed = false;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const reduced = () => motionReduced(defaults.motionPreference, media);
    const setStatic = () => {
        element.style.removeProperty('--sc-fancy-scroll-progress');
        element.style.removeProperty('--sc-fancy-scroll-distance');
        delete element.dataset.fancyReady;
        delete element.dataset.fancyScrollProgress;
    };
    const update = () => {
        if (destroyed || !intersecting || !documentVisible || reduced() || frame !== null) return;
        frame = requestAnimationFrame(() => {
            frame = null;
            if (destroyed || !intersecting || !documentVisible || reduced()) return;
            const rect = element.getBoundingClientRect();
            const progress = clamp((innerHeight - rect.top) / Math.max(innerHeight + rect.height, 1), 0, 1);
            const distance = Math.abs(progress - .5) * 2;
            element.style.setProperty('--sc-fancy-scroll-progress', `${progress}`);
            element.style.setProperty('--sc-fancy-scroll-distance', `${distance}`);
            element.dataset.fancyReady = 'true';
            element.dataset.fancyScrollProgress = `${Math.round(progress * 100)}`;
        });
    };
    const observer = new IntersectionObserver(entries => {
        intersecting = entries.some(entry => entry.isIntersecting);
        if (intersecting) update();
        else if (frame !== null) { cancelAnimationFrame(frame); frame = null; }
    }, { threshold: 0 });
    const mediaHandler = () => { if (reduced()) setStatic(); else update(); };
    observer.observe(element);
    addEventListener('scroll', update, { passive: true });
    addEventListener('resize', update, { passive: true });
    media?.addEventListener('change', mediaHandler);
    update();
    return {
        update(next) { options = next; update(); },
        setDocumentVisible(visible) {
            documentVisible = visible;
            if (!visible) { if (frame !== null) cancelAnimationFrame(frame); frame = null; }
            else update();
        },
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            observer.disconnect();
            removeEventListener('scroll', update);
            removeEventListener('resize', update);
            media?.removeEventListener('change', mediaHandler);
            if (frame !== null) cancelAnimationFrame(frame);
            setStatic();
        },
    };
}

function createPressScale(element, initialOptions, defaults) {
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const setPressed = pressed => {
        if (motionReduced(defaults.motionPreference, media)) return;
        if (pressed) element.dataset.fancyPressed = 'true';
        else delete element.dataset.fancyPressed;
    };
    const pointerDown = () => setPressed(true);
    const pointerUp = () => setPressed(false);
    const keyDown = event => { if (event.key === ' ' || event.key === 'Enter') setPressed(true); };
    const keyUp = event => { if (event.key === ' ' || event.key === 'Enter') setPressed(false); };
    const mediaHandler = () => { if (motionReduced(defaults.motionPreference, media)) setPressed(false); };
    element.addEventListener('pointerdown', pointerDown, { passive: true });
    element.addEventListener('pointerup', pointerUp, { passive: true });
    element.addEventListener('pointercancel', pointerUp, { passive: true });
    element.addEventListener('pointerleave', pointerUp, { passive: true });
    element.addEventListener('keydown', keyDown);
    element.addEventListener('keyup', keyUp);
    media?.addEventListener('change', mediaHandler);
    return {
        update() {}, setDocumentVisible() {}, hasActiveAnimationFrame() { return false; },
        destroy() {
            element.removeEventListener('pointerdown', pointerDown);
            element.removeEventListener('pointerup', pointerUp);
            element.removeEventListener('pointercancel', pointerUp);
            element.removeEventListener('pointerleave', pointerUp);
            element.removeEventListener('keydown', keyDown);
            element.removeEventListener('keyup', keyUp);
            media?.removeEventListener('change', mediaHandler);
            delete element.dataset.fancyPressed;
        },
    };
}

function createStagger(element, initialOptions, defaults) {
    let options = initialOptions; let observer = null; let frame = null; let timer = null;
    const configure = (replay = false) => { observer?.disconnect(); if (timer !== null) clearTimeout(timer); [...element.children].forEach((child, index) => child.style.setProperty('--sc-fancy-index', index)); element.dataset.fancyReady = 'true'; if (motionReduced(defaults.motionPreference)) { element.dataset.fancyVisible = 'true'; return; } element.dataset.fancyVisible = 'false'; observer = new IntersectionObserver(entries => entries.forEach(entry => { element.dataset.fancyVisible = entry.isIntersecting ? 'true' : 'false'; if (entry.isIntersecting && options.once !== false) observer?.disconnect(); }), { threshold: .1 }); const observe = () => { frame = requestAnimationFrame(() => { frame = requestAnimationFrame(() => { frame = null; observer?.observe(element); }); }); }; if (replay) timer = setTimeout(() => { timer = null; observe(); }, 300); else observe(); };
    configure(); return { update(next) { const replay = next.replayToken !== options.replayToken; options = next; configure(replay); }, setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; }, destroy() { if (frame !== null) cancelAnimationFrame(frame); if (timer !== null) clearTimeout(timer); observer?.disconnect(); [...element.children].forEach(child => child.style.removeProperty('--sc-fancy-index')); delete element.dataset.fancyReady; delete element.dataset.fancyVisible; } };
}

function createTextReveal(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;

    const tokens = () => {
        element.replaceChildren();
        element.setAttribute('aria-label', options.text ?? '');
        const values = options.unit === 'Character' ? Array.from(options.text ?? '') : (options.text ?? '').split(/(\s+)/);
        let index = 0;
        for (const value of values) {
            if (!value) continue;
            if (/^\s+$/.test(value)) { element.append(document.createTextNode(value)); continue; }
            const token = document.createElement('span');
            token.className = 'syntax-circus-fancy-text-reveal__token';
            token.setAttribute('aria-hidden', 'true');
            token.style.setProperty('--sc-fancy-index', index++);
            token.textContent = value;
            element.append(token);
        }
    };
    const configure = (replay = false) => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        tokens();
        element.dataset.fancyReady = 'true';
        if (motionReduced(defaults.motionPreference, media)) { element.dataset.fancyVisible = 'true'; return; }
        element.dataset.fancyVisible = 'false';
        observer = new IntersectionObserver(entries => entries.forEach(entry => {
            element.dataset.fancyVisible = entry.isIntersecting ? 'true' : 'false';
            if (entry.isIntersecting && options.once !== false) { observer?.disconnect(); observer = null; }
        }), { threshold: .1 });
        const observe = () => { frame = requestAnimationFrame(() => { frame = requestAnimationFrame(() => { frame = null; observer?.observe(element); }); }); };
        if (replay) timer = setTimeout(() => { timer = null; observe(); }, 80); else observe();
    };
    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();
    return {
        update(next) { const replay = next.replayToken !== options.replayToken; options = next; configure(replay); },
        setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; },
        destroy() { destroyed = true; if (frame !== null) cancelAnimationFrame(frame); if (timer !== null) clearTimeout(timer); observer?.disconnect(); media?.removeEventListener('change', mediaHandler); element.textContent = options.text ?? ''; element.removeAttribute('aria-label'); delete element.dataset.fancyReady; delete element.dataset.fancyVisible; },
    };
}

const SCRAMBLE_GLYPHS = '!@#$%^&*_+-=[]{}|;:,.<>?/0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';

function createScrambleText(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let tokens = [];
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;

    const buildTokens = () => {
        element.replaceChildren();
        element.setAttribute('aria-label', options.text ?? '');
        tokens = [];
        let index = 0;
        for (const character of Array.from(options.text ?? '')) {
            if (/^\s$/.test(character)) { element.append(document.createTextNode(character)); continue; }
            const token = document.createElement('span');
            token.className = 'syntax-circus-fancy-scramble-text__token';
            token.setAttribute('aria-hidden', 'true');
            token.textContent = character;
            element.append(token);
            tokens.push({ el: token, final: character, index: index++ });
        }
    };
    const settle = () => tokens.forEach(token => { token.el.textContent = token.final; });
    const animate = () => {
        if (frame !== null) cancelAnimationFrame(frame);
        const duration = Math.max(1, Number(options.duration) || 1);
        const stagger = Math.max(0, Number(options.stagger) || 0);
        const start = performance.now();
        const step = now => {
            if (destroyed) return;
            let running = false;
            for (const token of tokens) {
                const elapsed = now - (start + token.index * stagger);
                if (elapsed < 0) { running = true; continue; }
                if (elapsed >= duration) { token.el.textContent = token.final; continue; }
                running = true;
                token.el.textContent = SCRAMBLE_GLYPHS[Math.floor(Math.random() * SCRAMBLE_GLYPHS.length)];
            }
            frame = running ? requestAnimationFrame(step) : null;
        };
        frame = requestAnimationFrame(step);
    };
    const configure = (replay = false) => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        buildTokens();
        element.dataset.fancyReady = 'true';
        if (motionReduced(defaults.motionPreference, media)) { settle(); return; }
        const start = () => {
            observer = new IntersectionObserver(entries => entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                animate();
                if (options.once !== false) { observer?.disconnect(); observer = null; }
            }), { threshold: .1 });
            observer.observe(element);
        };
        if (replay) timer = setTimeout(() => { timer = null; start(); }, 80); else start();
    };
    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();
    return {
        update(next) { const replay = next.replayToken !== options.replayToken; options = next; configure(replay); },
        setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; },
        destroy() { destroyed = true; if (frame !== null) cancelAnimationFrame(frame); if (timer !== null) clearTimeout(timer); observer?.disconnect(); media?.removeEventListener('change', mediaHandler); element.textContent = options.text ?? ''; element.removeAttribute('aria-label'); delete element.dataset.fancyReady; },
    };
}

function createMarquee(element, initialOptions, defaults) {
    let options = initialOptions;
    let intersecting = false;
    let documentVisible = !document.hidden;
    let hovering = false;
    let destroyed = false;
    const track = element.querySelector('.syntax-circus-fancy-marquee__track');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const reduced = () => motionReduced(defaults.motionPreference, media);
    const apply = () => {
        if (!track) return;
        const running = !destroyed && intersecting && documentVisible && !reduced() && !(options.pauseOnHover && hovering);
        track.style.animationPlayState = running ? 'running' : 'paused';
    };
    const observer = new IntersectionObserver(entries => { intersecting = entries.some(entry => entry.isIntersecting); apply(); }, { threshold: 0 });
    const pointerEnter = () => { hovering = true; apply(); };
    const pointerLeave = () => { hovering = false; apply(); };
    const mediaHandler = () => apply();
    observer.observe(element);
    element.addEventListener('pointerenter', pointerEnter);
    element.addEventListener('pointerleave', pointerLeave);
    media?.addEventListener('change', mediaHandler);
    apply();
    return {
        update(next) { options = next; apply(); },
        setDocumentVisible(visible) { documentVisible = visible; apply(); },
        hasActiveAnimationFrame() { return track?.style.animationPlayState === 'running'; },
        destroy() { destroyed = true; observer.disconnect(); element.removeEventListener('pointerenter', pointerEnter); element.removeEventListener('pointerleave', pointerLeave); media?.removeEventListener('change', mediaHandler); if (track) track.style.animationPlayState = 'paused'; },
    };
}

function createWordRotate(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let lastSwapAt = 0;
    const display = element.querySelector('.syntax-circus-fancy-word-rotate__display');
    const srOnly = element.querySelector('.syntax-circus-fancy-word-rotate__sr-only');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const words = () => Array.isArray(options.words) ? options.words : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);
    const announce = (text) => { if (srOnly) srOnly.textContent = text; };

    const settle = () => {
        const list = words();
        if (list.length === 0 || !display) return;
        display.textContent = list[index % list.length] ?? '';
        display.dataset.fancyState = 'idle';
        announce(display.textContent);
    };

    const applyTransition = (next) => {
        if (!display) return;
        const transition = String(options.transition || 'fade');
        display.dataset.fancyTransition = transition;
        display.dataset.fancyState = 'out';
        if (timer !== null) clearTimeout(timer);
        const finish = () => {
            if (timer !== null) { clearTimeout(timer); timer = null; }
            display.textContent = next;
            display.dataset.fancyState = 'in';
            announce(next);
            display.removeEventListener('transitionend', finish);
        };
        display.addEventListener('transitionend', finish, { once: true });
        timer = setTimeout(finish, 400);
    };

    const tick = (now) => {
        if (destroyed) return;
        const list = words();
        if (list.length < 2) { frame = null; return; }
        const interval = Math.max(1, Number(options.interval) || 1);
        if (lastSwapAt === 0) lastSwapAt = now;
        if (now - lastSwapAt >= interval) {
            lastSwapAt = now;
            index = (index + 1) % list.length;
            applyTransition(list[index]);
        }
        frame = requestAnimationFrame(tick);
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) { clearTimeout(timer); timer = null; }
        if (!display) return;
        const list = words();
        if (list.length === 0) return;
        if (reduced() || list.length < 2) { settle(); return; }
        index = Math.min(index, list.length - 1);
        const initial = list[index];
        display.textContent = initial;
        display.dataset.fancyTransition = String(options.transition || 'fade');
        display.dataset.fancyState = 'in';
        announce(initial);
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { lastSwapAt = 0; frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            if (timer !== null) clearTimeout(timer);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            const list = words();
            const first = list.length > 0 ? (list[0] ?? '') : '';
            if (display) { display.textContent = first; delete display.dataset.fancyState; delete display.dataset.fancyTransition; }
            announce(first);
        },
    };
}

function createMorphText(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let phase = 'hold';
    let phaseStart = 0;
    const front = element.querySelector('[data-fancy-layer="front"]');
    const back = element.querySelector('[data-fancy-layer="back"]');
    const srOnly = element.querySelector('.syntax-circus-fancy-morph-text__sr-only');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const words = () => Array.isArray(options.words) ? options.words : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);
    const announce = (text) => { if (srOnly) srOnly.textContent = text; };

    const settle = () => {
        const list = words();
        if (list.length === 0 || !front || !back) return;
        front.textContent = list[index % list.length] ?? '';
        front.dataset.fancyState = 'in';
        back.textContent = '';
        back.dataset.fancyState = 'idle';
        announce(front.textContent);
    };

    const tick = (now) => {
        if (destroyed) return;
        const list = words();
        if (list.length < 2) { frame = null; return; }
        const duration = Math.max(1, Number(options.duration) || 1);
        const hold = Math.max(0, Number(options.hold) || 0);
        if (phase === 'hold' && now - phaseStart >= hold) {
            const nextIndex = (index + 1) % list.length;
            const next = list[nextIndex];
            back.textContent = next;
            back.dataset.fancyState = 'in';
            front.dataset.fancyState = 'out';
            if (timer !== null) clearTimeout(timer);
            const onEnd = () => {
                if (timer !== null) { clearTimeout(timer); timer = null; }
                front.textContent = next;
                front.dataset.fancyState = 'in';
                back.dataset.fancyState = 'idle';
                announce(next);
                index = nextIndex;
                phase = 'hold';
                phaseStart = performance.now();
                front.removeEventListener('transitionend', onEnd);
            };
            front.addEventListener('transitionend', onEnd, { once: true });
            timer = setTimeout(onEnd, 400);
            phase = 'morph';
            phaseStart = now;
        } else if (phase === 'morph' && now - phaseStart >= duration) {
            phase = 'hold';
            phaseStart = now;
        }
        frame = requestAnimationFrame(tick);
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) { clearTimeout(timer); timer = null; }
        if (!front || !back) return;
        const list = words();
        if (list.length === 0) return;
        if (reduced() || list.length < 2) { settle(); return; }
        index = Math.min(index, list.length - 1);
        const initial = list[index];
        front.textContent = initial; front.dataset.fancyState = 'in';
        back.textContent = initial; back.dataset.fancyState = 'idle';
        announce(initial);
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { phase = 'hold'; phaseStart = performance.now(); frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            if (timer !== null) clearTimeout(timer);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            const list = words();
            const first = list.length > 0 ? (list[0] ?? '') : '';
            if (front) { front.textContent = first; front.dataset.fancyState = 'in'; }
            if (back) { back.textContent = ''; back.dataset.fancyState = 'idle'; }
            announce(first);
        },
    };
}

function createTypewriter(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let phase = 'typing';
    let phaseStart = 0;
    let charIndex = 0;
    const textEl = element.querySelector('.syntax-circus-fancy-typewriter__text');
    const srOnly = element.querySelector('.syntax-circus-fancy-typewriter__sr-only');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const lines = () => Array.isArray(options.text) ? options.text : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);

    const syncAccessible = () => {
        const list = lines();
        if (list.length === 0 || !srOnly) return;
        srOnly.textContent = list[index % list.length] ?? '';
    };

    const settle = () => {
        const list = lines();
        if (list.length === 0 || !textEl) return;
        textEl.textContent = list[index % list.length] ?? '';
        syncAccessible();
    };

    const advanceLine = (list) => {
        if (index + 1 >= list.length) {
            if (options.loop === false) {
                const finalText = list[index % list.length] ?? '';
                textEl.textContent = finalText;
                syncAccessible();
                return false;
            }
            index = 0;
        } else {
            index++;
        }
        syncAccessible();
        phase = 'typing';
        charIndex = 0;
        phaseStart = performance.now();
        return true;
    };

    const tick = (now) => {
        if (destroyed || !textEl) return;
        const list = lines();
        if (list.length === 0) { frame = null; return; }
        const speed = Math.max(1, Number(options.speed) || 1);
        const deleteSpeed = options.deleteSpeed == null ? speed : Math.max(1, Number(options.deleteSpeed) || speed);
        const holdAfter = Math.max(0, Number(options.holdAfter) || 0);
        const current = list[index % list.length] ?? '';
        const chars = Array.from(current);
        if (phase === 'typing') {
            if (charIndex < chars.length) {
                if (now - phaseStart >= speed) {
                    charIndex++;
                    textEl.textContent = chars.slice(0, charIndex).join('');
                    phaseStart = now;
                }
            } else {
                phase = 'holdAfter';
                phaseStart = now;
            }
        } else if (phase === 'holdAfter') {
            if (now - phaseStart >= holdAfter) {
                if (charIndex > 0 && options.deleteSpeed !== null) { phase = 'deleting'; phaseStart = now; }
                else if (!advanceLine(list)) { frame = null; return; }
            }
        } else if (phase === 'deleting') {
            if (charIndex > 0) {
                if (now - phaseStart >= deleteSpeed) {
                    charIndex--;
                    textEl.textContent = chars.slice(0, charIndex).join('');
                    phaseStart = now;
                }
            } else if (!advanceLine(list)) { frame = null; return; }
        }
        frame = requestAnimationFrame(tick);
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (!textEl) return;
        const list = lines();
        if (list.length === 0) return;
        if (reduced()) { settle(); return; }
        index = Math.min(index, list.length - 1);
        textEl.textContent = '';
        charIndex = 0;
        phase = 'typing';
        phaseStart = performance.now();
        syncAccessible();
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { phaseStart = performance.now(); frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            const list = lines();
            const first = list.length > 0 ? (list[0] ?? '') : '';
            if (textEl) textEl.textContent = first;
            if (srOnly) srOnly.textContent = first;
        },
    };
}

function createNumberTicker(element, initialOptions, defaults) {
    let options = initialOptions;
    let frame = null;
    let current = 0;
    let destroyed = false;
    let observer = null;
    let timer = null;
    const display = element.querySelector('.syntax-circus-fancy-number-ticker__display');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const format = value => {
        const decimals = Math.max(0, Number(options.decimals) || 0);
        return new Intl.NumberFormat(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }).format(value);
    };
    const settle = () => { if (display) display.textContent = options.formatted ?? format(Number(options.value) || 0); current = Number(options.value) || 0; };
    const animate = () => {
        if (frame !== null) cancelAnimationFrame(frame);
        const target = Number(options.value) || 0;
        const start = current;
        const duration = Math.max(1, Number(options.duration) || 1);
        const startTime = performance.now();
        const step = now => {
            if (destroyed || !display) return;
            const progress = Math.min(1, (now - startTime) / duration);
            const eased = 1 - Math.pow(1 - progress, 3);
            current = start + (target - start) * eased;
            display.textContent = format(current);
            if (progress < 1) frame = requestAnimationFrame(step);
            else { display.textContent = options.formatted ?? format(target); current = target; frame = null; }
        };
        frame = requestAnimationFrame(step);
    };
    const configure = (replay = false) => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        if (motionReduced(defaults.motionPreference, media)) { settle(); return; }
        const start = () => {
            observer = new IntersectionObserver(entries => entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                animate();
                if (options.once !== false) { observer?.disconnect(); observer = null; }
            }), { threshold: .3 });
            observer.observe(element);
        };
        if (replay) timer = setTimeout(() => { timer = null; current = 0; start(); }, 80); else start();
    };
    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();
    return {
        update(next) { const replay = next.replayToken !== options.replayToken; options = next; configure(replay); },
        setDocumentVisible() {}, hasActiveAnimationFrame() { return frame !== null; },
        destroy() { destroyed = true; if (frame !== null) cancelAnimationFrame(frame); if (timer !== null) clearTimeout(timer); observer?.disconnect(); media?.removeEventListener('change', mediaHandler); if (display) display.textContent = options.formatted ?? ''; },
    };
}

function createRipple(element, initialOptions, defaults) {
    let options = initialOptions;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const layer = element.querySelector('.syntax-circus-fancy-ripple__layer');
    const ripple = event => {
        if (motionReduced(defaults.motionPreference, media) || !layer) return;
        const rect = element.getBoundingClientRect();
        const diameter = Math.hypot(rect.width, rect.height) * 2;
        const wave = document.createElement('span');
        wave.className = 'syntax-circus-fancy-ripple__wave'; wave.setAttribute('aria-hidden', 'true');
        wave.style.width = `${diameter}px`; wave.style.height = `${diameter}px`;
        wave.style.left = `${event.clientX - rect.left}px`; wave.style.top = `${event.clientY - rect.top}px`;
        layer.append(wave);
        window.setTimeout(() => wave.remove(), Math.max(0, Number(options.duration) || 0));
    };
    element.addEventListener('pointerdown', ripple, { passive: true });
    return { update(next) { options = next; }, setDocumentVisible() {}, hasActiveAnimationFrame() { return false; }, destroy() { element.removeEventListener('pointerdown', ripple); layer?.replaceChildren(); } };
}

function createCursorTrail(element, initialOptions, defaults) {
    let options = initialOptions;
    let particles = [];
    let frame = null;
    let active = true;
    const canvas = element.querySelector('.syntax-circus-fancy-cursor-trail__canvas');
    const context = canvas?.getContext('2d');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const resize = () => {
        if (!canvas) return;
        const rect = element.getBoundingClientRect(); const dpr = Math.min(devicePixelRatio || 1, 2);
        canvas.width = Math.max(1, Math.round(rect.width * dpr)); canvas.height = Math.max(1, Math.round(rect.height * dpr));
        canvas.style.width = `${rect.width}px`; canvas.style.height = `${rect.height}px`; context?.setTransform(dpr, 0, 0, dpr, 0, 0);
    };
    const clear = () => { if (!canvas || !context) return; const rect = element.getBoundingClientRect(); context.clearRect(0, 0, rect.width, rect.height); };
    const reset = () => { if (frame !== null) cancelAnimationFrame(frame); frame = null; particles = []; clear(); };
    const draw = now => {
        frame = null; clear();
        const duration = Math.max(1, Number(options.duration) || 1); const rect = element.getBoundingClientRect();
        const color = options.color === 'currentColor'
            ? getComputedStyle(element).color
            : (options.color || getComputedStyle(element).getPropertyValue('--sc-fancy-cursor-trail-color') || getComputedStyle(element).color);
        particles = particles.filter(particle => now - particle.created < duration);
        for (const particle of particles) { const age = (now - particle.created) / duration; context.fillStyle = color; context.globalAlpha = (1 - age) * .75; context.beginPath(); context.arc(particle.x, particle.y, Math.max(1, Number(options.size) * (1 - age) / 2), 0, Math.PI * 2); context.fill(); }
        context.globalAlpha = 1;
        if (particles.length) frame = requestAnimationFrame(draw);
    };
    const move = event => {
        if (!active || motionReduced(defaults.motionPreference, media) || !canvas || !context) return;
        const rect = element.getBoundingClientRect(); particles.push({ x: event.clientX - rect.left, y: event.clientY - rect.top, created: performance.now() });
        const cap = clamp(options.particleCount, 1, 48); if (particles.length > cap) particles.splice(0, particles.length - cap);
        if (frame === null) frame = requestAnimationFrame(draw);
    };
    const mediaHandler = () => { if (motionReduced(defaults.motionPreference, media)) reset(); };
    const resizeObserver = typeof ResizeObserver === 'undefined' ? null : new ResizeObserver(resize);
    resize(); resizeObserver?.observe(element); element.addEventListener('pointermove', move, { passive: true }); media?.addEventListener('change', mediaHandler);
    return { update(next) { options = next; resize(); }, setDocumentVisible(visible) { active = visible; if (!visible) reset(); }, hasActiveAnimationFrame() { return frame !== null; }, destroy() { element.removeEventListener('pointermove', move); media?.removeEventListener('change', mediaHandler); resizeObserver?.disconnect(); reset(); } };
}

function createCanvasAtmosphere(element, initialOptions, defaults, kind) {
    let options = initialOptions;
    let frame = null;
    let intersecting = false;
    let documentVisible = !document.hidden;
    let destroyed = false;
    let particles = [];
    let colors = resolvePalette(initialOptions.palette);
    const canvas = element.querySelector('canvas');
    const context = canvas?.getContext('2d');
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const reduced = () => motionReduced(defaults.motionPreference, media);
    const resize = () => {
        if (!canvas || !context) return;
        const rect = element.getBoundingClientRect();
        const dpr = Math.min(devicePixelRatio || 1, qualityDpr(defaults.quality));
        canvas.width = Math.max(1, Math.round(rect.width * dpr));
        canvas.height = Math.max(1, Math.round(rect.height * dpr));
        canvas.style.width = `${rect.width}px`;
        canvas.style.height = `${rect.height}px`;
        context.setTransform(dpr, 0, 0, dpr, 0, 0);
        particles = createAtmosphereParticles(rect, Math.max(1, Number(options.density) || 1), kind);
    };
    const clear = () => {
        if (!canvas || !context) return;
        const rect = element.getBoundingClientRect();
        context.clearRect(0, 0, rect.width, rect.height);
    };
    const stop = () => { if (frame !== null) cancelAnimationFrame(frame); frame = null; clear(); };
    const draw = now => {
        frame = null;
        if (destroyed || !intersecting || !documentVisible || reduced() || !context) return;
        const rect = element.getBoundingClientRect();
        clear();
        const speed = clamp(options.speed, 0, 3) * .25;
        if (kind === 'constellation') drawConstellation(context, particles, rect, now, speed, colors, clamp(options.lineOpacity, 0, 1));
        else if (kind === 'arc-flow') drawArcFlow(context, particles, rect, now, speed, colors, clamp(options.intensity, 0, 1));
        else if (kind === 'flicker-grid') drawFlickerGrid(context, particles, rect, now, speed, colors, clamp(options.intensity, 0, 1));
        else if (kind === 'meteor') drawMeteors(context, particles, rect, now, speed, colors, clamp(options.intensity, 0, 1));
        else drawLightRays(context, particles, rect, now, speed, colors, clamp(options.intensity, 0, 1));
        frame = requestAnimationFrame(draw);
    };
    const start = () => {
        if (destroyed || frame !== null || !intersecting || !documentVisible || reduced() || !context) return;
        frame = requestAnimationFrame(draw);
    };
    const observer = new IntersectionObserver(entries => {
        intersecting = entries.some(entry => entry.isIntersecting);
        if (intersecting) start(); else stop();
    }, { threshold: 0 });
    const mediaHandler = () => { if (reduced()) stop(); else start(); };
    const resizeObserver = typeof ResizeObserver === 'undefined' ? null : new ResizeObserver(() => { resize(); start(); });
    resize();
    observer.observe(element);
    resizeObserver?.observe(element);
    media?.addEventListener('change', mediaHandler);
    return {
        update(next) { options = next; colors = resolvePalette(options.palette); resize(); start(); },
        setDocumentVisible(visible) { documentVisible = visible; if (visible) start(); else stop(); },
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() { destroyed = true; observer.disconnect(); resizeObserver?.disconnect(); media?.removeEventListener('change', mediaHandler); stop(); },
    };
}

function createAtmosphereParticles(rect, count, kind) {
    return Array.from({ length: count }, (_, index) => ({
        x: Math.random() * Math.max(rect.width, 1), y: Math.random() * Math.max(rect.height, 1),
        radius: kind === 'constellation' ? 1 + Math.random() * 1.5 : 24 + Math.random() * 80,
        phase: Math.random() * Math.PI * 2, drift: .3 + Math.random() * .7, color: index % 3,
    }));
}

function drawConstellation(context, particles, rect, now, speed, colors, opacity) {
    const time = now * .001 * speed;
    const points = particles.map(particle => ({ ...particle, x: (particle.x + Math.sin(time * particle.drift + particle.phase) * 24 + rect.width) % Math.max(rect.width, 1), y: (particle.y + Math.cos(time * particle.drift + particle.phase) * 18 + rect.height) % Math.max(rect.height, 1) }));
    const linkDistance = Math.min(180, Math.max(72, Math.min(rect.width, rect.height) * .32));
    context.lineWidth = 1;
    for (let index = 0; index < points.length; index++) {
        const point = points[index];
        context.fillStyle = `rgba(${colors[point.color].map(value => Math.round(value * 255)).join(',')},${.25 + opacity * .55})`;
        context.beginPath(); context.arc(point.x, point.y, point.radius, 0, Math.PI * 2); context.fill();
        for (let other = index + 1; other < points.length; other++) {
            const target = points[other]; const distance = Math.hypot(point.x - target.x, point.y - target.y);
            if (distance > linkDistance) continue;
            context.strokeStyle = `rgba(${colors[point.color].map(value => Math.round(value * 255)).join(',')},${(1 - distance / linkDistance) * opacity * .45})`;
            context.beginPath(); context.moveTo(point.x, point.y); context.lineTo(target.x, target.y); context.stroke();
        }
    }
}

function drawArcFlow(context, particles, rect, now, speed, colors, intensity) {
    const time = now * .001 * speed;
    context.lineWidth = 1.25;
    for (const arc of particles) {
        const x = (arc.x + time * 42 * arc.drift) % Math.max(rect.width + arc.radius * 2, 1) - arc.radius;
        const y = arc.y + Math.sin(time * arc.drift + arc.phase) * 28;
        const rgb = colors[arc.color].map(value => Math.round(value * 255)).join(',');
        context.strokeStyle = `rgba(${rgb},${.1 + intensity * .38})`;
        context.beginPath(); context.arc(x, y, arc.radius, Math.PI * .15, Math.PI * 1.15); context.stroke();
    }
}

function drawFlickerGrid(context, particles, rect, now, speed, colors, intensity) {
    const time = now * .001 * speed;
    const columns = Math.max(1, Math.ceil(Math.sqrt(particles.length)));
    const rows = Math.max(1, Math.ceil(particles.length / columns));
    const cellWidth = rect.width / columns;
    const cellHeight = rect.height / rows;
    const inset = Math.min(cellWidth, cellHeight) * .18;
    particles.forEach((cell, index) => {
        const column = index % columns;
        const row = Math.floor(index / columns);
        const flicker = .5 + .5 * Math.sin(time * (1.5 + cell.drift) + cell.phase);
        const rgb = colors[cell.color].map(value => Math.round(value * 255)).join(',');
        context.fillStyle = `rgba(${rgb},${flicker * intensity * .6})`;
        context.fillRect(column * cellWidth + inset, row * cellHeight + inset, Math.max(0, cellWidth - inset * 2), Math.max(0, cellHeight - inset * 2));
    });
}

function drawMeteors(context, particles, rect, now, speed, colors, intensity) {
    const time = now * .001 * speed;
    const diagonal = rect.width + rect.height;
    context.lineCap = 'round';
    for (const meteor of particles) {
        const travel = (time * 220 * (.5 + meteor.drift) + meteor.phase * 80) % (diagonal + 200) - 100;
        const x = meteor.x + travel;
        const y = meteor.y + travel;
        const length = 40 + meteor.radius;
        const rgb = colors[meteor.color].map(value => Math.round(value * 255)).join(',');
        const gradient = context.createLinearGradient(x, y, x - length, y - length);
        gradient.addColorStop(0, `rgba(${rgb},${.15 + intensity * .55})`);
        gradient.addColorStop(1, `rgba(${rgb},0)`);
        context.strokeStyle = gradient;
        context.lineWidth = 1.5;
        context.beginPath(); context.moveTo(x, y); context.lineTo(x - length, y - length); context.stroke();
    }
}

function drawLightRays(context, particles, rect, now, speed, colors, intensity) {
    const time = now * .001 * speed;
    const originX = rect.width * .5;
    const originY = -rect.height * .15;
    const maxRadius = Math.hypot(rect.width, rect.height) * 1.1;
    const count = particles.length;
    const halfWidth = (Math.PI / count) * .8;
    particles.forEach((ray, index) => {
        const angle = (index / count) * Math.PI * 2 + time * .12 * (.4 + ray.drift * .2) + ray.phase;
        const rgb = colors[ray.color].map(value => Math.round(value * 255)).join(',');
        const gradient = context.createRadialGradient(originX, originY, 0, originX, originY, maxRadius);
        gradient.addColorStop(0, `rgba(${rgb},${.05 + intensity * .22})`);
        gradient.addColorStop(1, `rgba(${rgb},0)`);
        context.fillStyle = gradient;
        context.beginPath();
        context.moveTo(originX, originY);
        context.arc(originX, originY, maxRadius, angle - halfWidth, angle + halfWidth);
        context.closePath();
        context.fill();
    });
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
