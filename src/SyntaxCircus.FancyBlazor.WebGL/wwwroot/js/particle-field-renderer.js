export async function loadThree() {
    const modulePath = "../vendor/three/build/" + "three.module.js";
    return import(new URL(`${modulePath}?v=r184`, import.meta.url).href);
}

function finiteOr(value, fallback) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
}

function resolveParticleCount(options, defaults) {
    const quality = options.quality || defaults.quality;
    const cap = quality === "Low" ? 80 : quality === "Medium" ? 160 : quality === "High" ? 320 : 160;
    const density = Math.max(0, Math.min(1, finiteOr(options.density, 0.5)));
    return Math.max(12, Math.round(cap * density));
}

function buildGeometry(count, THREE) {
    const positions = new Float32Array(count * 3);
    const seeds = new Float32Array(count);
    const scales = new Float32Array(count);
    for (let i = 0; i < count; i++) {
        positions[i * 3] = Math.random() * 2 - 1;
        positions[i * 3 + 1] = Math.random() * 2 - 1;
        positions[i * 3 + 2] = 0;
        seeds[i] = Math.random();
        scales[i] = 0.5 + Math.random();
    }

    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute("aSeed", new THREE.BufferAttribute(seeds, 1));
    geometry.setAttribute("aScale", new THREE.BufferAttribute(scales, 1));
    return geometry;
}

export function createParticleFieldBackground(canvas, options, defaults, THREE) {
    const renderer = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: false, powerPreference: "low-power" });
    const scene = new THREE.Scene();
    const camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    let particleCount = resolveParticleCount(options, defaults);
    let geometry = buildGeometry(particleCount, THREE);
    const material = new THREE.ShaderMaterial({
        transparent: true,
        depthTest: false,
        uniforms: {
            time: { value: 0 },
            intensity: { value: finiteOr(options.intensity, 0.5) },
            drift: { value: finiteOr(options.drift, 0.5) },
            size: { value: finiteOr(options.size, 0.5) },
            interactiveFlag: { value: options.interactive ? 1 : 0 },
            pointer: { value: new THREE.Vector2(0.5, 0.5) },
            first: { value: new THREE.Color(options.palette?.[0] || "#5e82f6") },
            second: { value: new THREE.Color(options.palette?.[1] || "#a855f7") },
            accent: { value: new THREE.Color(options.palette?.[2] || "#22d3ee") },
        },
        vertexShader: "attribute float aSeed;attribute float aScale;uniform float time;uniform float drift;uniform float size;uniform float interactiveFlag;uniform vec2 pointer;varying float vSeed;void main(){vSeed=aSeed;vec2 pos=position.xy;float phase=aSeed*6.28318;vec2 driftVec=vec2(cos(phase),sin(phase));pos+=driftVec*drift*0.15*sin(time*(0.3+aSeed*0.4)+phase);vec2 pointerClip=pointer*2.0-1.0;vec2 toPointer=pointerClip-pos;float d=length(toPointer);float pull=smoothstep(0.4,0.0,d)*0.15*interactiveFlag;vec2 dir=d>0.0001?toPointer/d:vec2(0.0);pos+=dir*pull;pos=mod(pos+1.0,2.0)-1.0;gl_Position=vec4(pos,0.0,1.0);gl_PointSize=(2.0+size*14.0)*aScale;}",
        fragmentShader: "varying float vSeed;uniform float intensity;uniform vec3 first;uniform vec3 second;uniform vec3 accent;void main(){vec2 uv=gl_PointCoord-vec2(0.5);float d=length(uv)*2.0;float alpha=smoothstep(1.0,0.0,d);vec3 color=mix(first,second,vSeed);float glow=smoothstep(1.0,0.0,d*1.6);color=mix(color,accent,glow*0.3);gl_FragColor=vec4(color,alpha*intensity);}",
    });
    const points = new THREE.Points(geometry, material);
    scene.add(points);
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
            material.uniforms.drift.value = finiteOr(next.drift, 0.5);
            material.uniforms.size.value = finiteOr(next.size, 0.5);
            material.uniforms.interactiveFlag.value = next.interactive ? 1 : 0;
            material.uniforms.first.value.set(next.palette?.[0] || "#5e82f6");
            material.uniforms.second.value.set(next.palette?.[1] || "#a855f7");
            material.uniforms.accent.value.set(next.palette?.[2] || "#22d3ee");
            const nextCount = resolveParticleCount(next, defaults);
            if (nextCount !== particleCount) {
                const oldGeometry = geometry;
                geometry = buildGeometry(nextCount, THREE);
                points.geometry = geometry;
                oldGeometry.dispose();
                particleCount = nextCount;
            }
        },
        getPalette() {
            return [material.uniforms.first.value, material.uniforms.second.value, material.uniforms.accent.value]
                .map(color => `#${color.getHexString()}`);
        },
        getState() {
            return {
                time: material.uniforms.time.value,
                resizeCount,
                particleCount,
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
