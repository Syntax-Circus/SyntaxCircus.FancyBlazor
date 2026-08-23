export async function createHolographicSurface(canvas, options, defaults) {
    const modulePath = "../vendor/three/build/" + "three.module.js";
    const THREE = await import(new URL(`${modulePath}?v=r184`, import.meta.url).href);
    const renderer = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: false, powerPreference: "low-power" });
    const scene = new THREE.Scene();
    const camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    const material = new THREE.ShaderMaterial({
        transparent: true,
        uniforms: {
            time: { value: 0 },
            intensity: { value: Number(options.intensity) || 0.5 },
            depth: { value: Number(options.depth) || 0.5 },
            sheen: { value: Number(options.sheen) || 0.5 },
            pointer: { value: new THREE.Vector2(0.5, 0.5) },
            first: { value: new THREE.Color(options.palette?.[0] || "#7c3aed") },
            second: { value: new THREE.Color(options.palette?.[1] || "#2563eb") },
            accent: { value: new THREE.Color(options.palette?.[2] || "#22d3ee") },
        },
        vertexShader: "void main(){gl_Position=vec4(position,1.0);}",
        fragmentShader: "uniform float time;uniform float intensity;uniform float depth;uniform float sheen;uniform vec2 pointer;uniform vec3 first;uniform vec3 second;uniform vec3 accent;void main(){vec2 uv=gl_FragCoord.xy/vec2(max(1.0,gl_FragCoord.w));float wave=sin((uv.x+uv.y+time*.08)*12.0)*.5+.5;float glint=pow(max(0.0,1.0-distance(uv,pointer)),4.0)*sheen;vec3 color=mix(first,second,wave);color=mix(color,accent,glint+depth*.12);gl_FragColor=vec4(color,intensity*.65);}",
    });
    const geometry = new THREE.PlaneGeometry(2, 2);
    scene.add(new THREE.Mesh(geometry, material));
    let frame = 0;
    let startedAt = performance.now();

    function size() {
        const bounds = canvas.getBoundingClientRect();
        const quality = options.quality || defaults.quality;
        const ceiling = quality === "Low" ? 1 : quality === "Medium" ? 1.5 : quality === "High" ? 2 : 1.5;
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, ceiling));
        renderer.setSize(Math.max(1, bounds.width), Math.max(1, bounds.height), false);
    }

    function draw(now) {
        frame = requestAnimationFrame(draw);
        material.uniforms.time.value = (now - startedAt) / 1000 * (Number(options.speed) || 1);
        size();
        renderer.render(scene, camera);
    }

    return {
        start() { if (!frame) { frame = requestAnimationFrame(draw); } },
        hasFrame() { return frame !== 0; },
        setPointer(x, y) { material.uniforms.pointer.value.set(x, 1 - y); },
        update(next) {
            options = next;
            material.uniforms.intensity.value = Number(next.intensity) || 0.5;
            material.uniforms.depth.value = Number(next.depth) || 0.5;
            material.uniforms.sheen.value = Number(next.sheen) || 0.5;
        },
        destroy() {
            if (frame) { cancelAnimationFrame(frame); frame = 0; }
            geometry.dispose();
            material.dispose();
            renderer.dispose();
            renderer.forceContextLoss();
        },
    };
}
