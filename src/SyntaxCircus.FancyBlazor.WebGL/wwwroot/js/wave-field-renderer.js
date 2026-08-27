export async function loadThree() {
    const modulePath = "../vendor/three/build/" + "three.module.js";
    return import(new URL(`${modulePath}?v=r184`, import.meta.url).href);
}

function finiteOr(value, fallback) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
}

export function createWaveFieldBackground(canvas, options, defaults, THREE) {
    const renderer = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: false, powerPreference: "low-power" });
    const scene = new THREE.Scene();
    const camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    const material = new THREE.ShaderMaterial({
        transparent: true,
        uniforms: {
            time: { value: 0 },
            intensity: { value: finiteOr(options.intensity, 0.5) },
            amplitude: { value: finiteOr(options.amplitude, 0.5) },
            frequency: { value: finiteOr(options.frequency, 0.5) },
            foam: { value: finiteOr(options.foam, 0.5) },
            pointer: { value: new THREE.Vector2(0.5, 0.5) },
            first: { value: new THREE.Color(options.palette?.[0] || "#5e82f6") },
            second: { value: new THREE.Color(options.palette?.[1] || "#a855f7") },
            accent: { value: new THREE.Color(options.palette?.[2] || "#22d3ee") },
        },
        vertexShader: "varying vec2 vUv;void main(){vUv=uv;gl_Position=vec4(position,1.0);}",
        fragmentShader: "varying vec2 vUv;uniform float time;uniform float intensity;uniform float amplitude;uniform float frequency;uniform float foam;uniform vec2 pointer;uniform vec3 first;uniform vec3 second;uniform vec3 accent;void main(){vec2 uv=vUv;float freq=2.0+frequency*10.0;float ripple=pow(max(0.0,1.0-distance(uv,pointer)*3.0),3.0)*amplitude;float wave=sin(uv.y*freq+time);wave+=sin(uv.y*freq*1.7-time*1.3+uv.x*2.0)*0.6;wave+=sin(uv.y*freq*2.3+time*0.7-uv.x*3.0)*0.4;wave=wave*amplitude+ripple;float phase=wave*0.5+0.5;vec3 color=mix(first,second,smoothstep(0.0,1.0,phase));float crest=pow(max(0.0,sin(phase*3.14159265)),8.0)*foam;color=mix(color,accent,crest);gl_FragColor=vec4(color,intensity*0.7+crest*0.2);}",
    });
    const geometry = new THREE.PlaneGeometry(2, 2);
    scene.add(new THREE.Mesh(geometry, material));
    let frame = 0;
    let startedAt = performance.now();
    let destroyed = false;
    let restoreContext = null;
    let resizeCount = 0;
    let lastWidth = 0;
    let lastHeight = 0;
    let lastPixelRatio = 0;

    function size() {
        const bounds = canvas.getBoundingClientRect();
        const quality = options.quality || defaults.quality;
        const ceiling = quality === "Low" ? 1 : quality === "Medium" ? 1.5 : quality === "High" ? 2 : 1.5;
        const pixelRatio = Math.min(window.devicePixelRatio || 1, ceiling);
        const width = Math.max(1, bounds.width);
        const height = Math.max(1, bounds.height);
        if (width === lastWidth && height === lastHeight && pixelRatio === lastPixelRatio) {
            return;
        }

        lastWidth = width;
        lastHeight = height;
        lastPixelRatio = pixelRatio;
        renderer.setPixelRatio(pixelRatio);
        renderer.setSize(width, height, false);
        resizeCount++;
    }

    function draw(now) {
        frame = requestAnimationFrame(draw);
        material.uniforms.time.value = (now - startedAt) / 1000 * finiteOr(options.speed, 1);
        size();
        renderer.render(scene, camera);
    }

    return {
        start() { if (!frame) { frame = requestAnimationFrame(draw); } },
        hasFrame() { return frame !== 0; },
        setPointer(x, y) { material.uniforms.pointer.value.set(x, 1 - y); },
        update(next) {
            options = next;
            material.uniforms.intensity.value = finiteOr(next.intensity, 0.5);
            material.uniforms.amplitude.value = finiteOr(next.amplitude, 0.5);
            material.uniforms.frequency.value = finiteOr(next.frequency, 0.5);
            material.uniforms.foam.value = finiteOr(next.foam, 0.5);
            material.uniforms.first.value.set(next.palette?.[0] || "#5e82f6");
            material.uniforms.second.value.set(next.palette?.[1] || "#a855f7");
            material.uniforms.accent.value.set(next.palette?.[2] || "#22d3ee");
        },
        getPalette() {
            return [material.uniforms.first.value, material.uniforms.second.value, material.uniforms.accent.value]
                .map(color => `#${color.getHexString()}`);
        },
        getState() {
            return {
                time: material.uniforms.time.value,
                resizeCount,
                amplitude: material.uniforms.amplitude.value,
                frequency: material.uniforms.frequency.value,
            };
        },
        destroy() {
            if (destroyed) {
                return restoreContext;
            }

            destroyed = true;
            if (frame) { cancelAnimationFrame(frame); frame = 0; }
            const context = renderer.getContext();
            const extension = context.getExtension("WEBGL_lose_context");
            const contextWasLost = context.isContextLost();
            const contextLost = !extension || contextWasLost
                ? Promise.resolve()
                : new Promise(resolve => canvas.addEventListener("webglcontextlost", resolve, { once: true }));
            let restoreRequest = null;
            restoreContext = () => {
                restoreRequest ??= (async () => {
                    await contextLost;
                    if (!extension || !context.isContextLost()) {
                        return;
                    }

                    const restoration = new Promise(resolve => canvas.addEventListener("webglcontextrestored", resolve, { once: true }));
                    extension.restoreContext();
                    await restoration;
                })();
                return restoreRequest;
            };
            geometry.dispose();
            material.dispose();
            renderer.dispose();
            renderer.forceContextLoss();
            return restoreContext;
        },
    };
}
