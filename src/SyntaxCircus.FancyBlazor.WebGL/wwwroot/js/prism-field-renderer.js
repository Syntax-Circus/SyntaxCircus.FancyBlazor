export async function loadThree() {
    const modulePath = "../vendor/three/build/" + "three.module.js";
    return import(new URL(`${modulePath}?v=r184`, import.meta.url).href);
}

function finiteOr(value, fallback) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
}

export function createPrismFieldBackground(canvas, options, defaults, THREE) {
    const renderer = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: false, powerPreference: "low-power" });
    const scene = new THREE.Scene();
    const camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    const material = new THREE.ShaderMaterial({
        transparent: true,
        uniforms: {
            time: { value: 0 },
            intensity: { value: finiteOr(options.intensity, 0.5) },
            facets: { value: finiteOr(options.facets, 0.5) },
            dispersion: { value: finiteOr(options.dispersion, 0.5) },
            sheen: { value: finiteOr(options.sheen, 0.5) },
            pointer: { value: new THREE.Vector2(0.5, 0.5) },
            first: { value: new THREE.Color(options.palette?.[0] || "#5e82f6") },
            second: { value: new THREE.Color(options.palette?.[1] || "#a855f7") },
            accent: { value: new THREE.Color(options.palette?.[2] || "#22d3ee") },
        },
        vertexShader: "varying vec2 vUv;void main(){vUv=uv;gl_Position=vec4(position,1.0);}",
        fragmentShader: "varying vec2 vUv;uniform float time;uniform float intensity;uniform float facets;uniform float dispersion;uniform float sheen;uniform vec2 pointer;uniform vec3 first;uniform vec3 second;uniform vec3 accent;float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453123);}void main(){vec2 uv=vUv;float density=3.0+facets*9.0;float ang=0.35;vec2 rot=vec2(uv.x*cos(ang)-uv.y*sin(ang),uv.x*sin(ang)+uv.y*cos(ang));vec2 cell=floor(rot*density);float h=hash(cell);vec3 color=mix(first,second,h);float off=dispersion*0.02;vec2 rotR=vec2((uv.x+off)*cos(ang)-uv.y*sin(ang),(uv.x+off)*sin(ang)+uv.y*cos(ang));vec2 rotB=vec2((uv.x-off)*cos(ang)-uv.y*sin(ang),(uv.x-off)*sin(ang)+uv.y*cos(ang));float hr=hash(floor(rotR*density));float hb=hash(floor(rotB*density));color.r=mix(color.r,mix(first.r,second.r,hr),0.5);color.b=mix(color.b,mix(first.b,second.b,hb),0.5);vec2 pointerRel=pointer-vec2(0.5);float lightAngle=atan(pointerRel.y,pointerRel.x)+time*0.2;vec2 lightDir=vec2(cos(lightAngle),sin(lightAngle));vec2 cellCenter=(cell+0.5)/density;vec2 toCell=cellCenter-vec2(0.5);float toCellLen=length(toCell);vec2 toCellDir=toCellLen>0.0001?toCell/toCellLen:vec2(0.0);float facing=max(0.0,dot(toCellDir,lightDir));float glint=pow(facing,6.0)*sheen;color=mix(color,accent,glint);gl_FragColor=vec4(color,intensity*0.75+0.25);}",
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
            material.uniforms.facets.value = finiteOr(next.facets, 0.5);
            material.uniforms.dispersion.value = finiteOr(next.dispersion, 0.5);
            material.uniforms.sheen.value = finiteOr(next.sheen, 0.5);
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
                facets: material.uniforms.facets.value,
                dispersion: material.uniforms.dispersion.value,
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
