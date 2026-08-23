// renderer.js — framework-free WebGL runtime for a single fragment-shader tile.
//
// Design goals (see README): every shader gets the same uniform contract, and a
// page full of tiles must not melt the GPU — so each renderer only runs its RAF
// loop while its canvas is on screen (IntersectionObserver), shares no global
// state, and caps devicePixelRatio.
//
//   const tile = createShaderTile(canvas, fragSource, {
//     palette: [[r,g,b], ...],
//     uniforms: { u_speed: 1.0 },  // optional per-shader float params
//     post: { grain: 0.05, vignette: 0.3 },  // optional post-FX overrides
//   })
//   tile.setPalette(nextPalette)        // re-theme live
//   tile.setUniform('u_speed', 1.4)     // tweak a param live
//   tile.setPost('vignette', 0.4)       // tweak a post-FX control live
//   tile.destroy()                      // tear down GL + observers
//
// Palettes are arrays of up to 4 [r,g,b] triples in 0..1. Custom uniforms are
// floats only; names must match a `uniform float` in the fragment source
// (unknown names are ignored so metas and shaders can evolve independently).
//
// Rendering is TWO-PASS (P0-D, 2026-06-11): the shader draws into an offscreen
// framebuffer texture, then a shared "post" program samples it and applies film
// grain, chromatic aberration, vignette and tone (exposure/contrast/saturation)
// in one cheap pass before it reaches the canvas. A single shared post-pass
// lifts the whole gallery toward a richer, less-flat look at once; per-shader
// `post` overrides ride in meta.json. The poster pipeline (scripts/render-png.mjs)
// mirrors this exactly so static posters match the live tile.

const VERTEX_SRC = `
attribute vec2 a_pos;
void main() { gl_Position = vec4(a_pos, 0.0, 1.0); }
`;

// Gentle global default — applied to EVERY shader unless its meta overrides a
// key (decision RESOLVED 2026-06-11 by Erin: gentle global default over opt-in).
// A shader opts out of any effect by setting it to its identity value
// (grain/aberration/vignette → 0, exposure/contrast/saturation → 1).
export const POST_DEFAULTS = {
  grain: 0.05,       // animated film grain intensity (0 = none)
  grainSize: 1.5,    // grain cell size in device px (1 = per-pixel, larger = chunkier boil)
  grainSpeed: 6.0,   // grain reseed rate in fields/sec (0 = frozen/static, higher = faster boil)
  aberration: 0.0015, // radial chromatic aberration, UV-space offset at the corner
  vignette: 0.25,    // edge darkening amount (0 = none)
  exposure: 1.0,     // linear gain (1 = unchanged)
  contrast: 1.0,     // contrast around mid-grey (1 = unchanged)
  saturation: 1.0,   // 0 = greyscale, 1 = unchanged, >1 = punchier
  bloom: 0.0,        // single-pass highlight glow intensity (0 = off)
  bloomRadius: 3.0,  // bloom spread in device px (only active when bloom > 0)
  dither: 0.0,       // banding-removal dither, in 8-bit LSBs (0 = off, ~1 = exact)
  // gradient-mesh theme backdrop — a soft palette-coloured field that fills the
  // dead-black BEHIND the shader content (luminance-keyed: only near-black
  // regions take the fill, lit features stay on top). The catalog-wide "empty/
  // dark background" fix; opt in per shader via meta.post.backdrop.
  backdrop: 0.0,        // backdrop fill strength (0 = off / byte-identical)
  backdropScale: 1.0,   // pole spread (larger = broader, softer colour zones)
  // "look" primitives — all default-off (identity); preset bundles dial them in
  scanline: 0.0,     // CRT/VHS scanline darkening of alternate rows (0 = off)
  curve: 0.0,        // CRT barrel curvature of the sampling UV (0 = flat)
  crtMask: 0.0,      // CRT RGB phosphor column mask strength (0 = off)
  smear: 0.0,        // VHS horizontal chroma bleed / tape smear (0 = off)
  wobble: 0.0,       // VHS per-row tape wobble (animated horizontal jitter, 0 = off)
  glitch: 0.0,       // digital glitch: banded horizontal tears + channel split (0 = off)
  pixelate: 0.0,     // lo-fi mosaic: sampling-UV block size in px (0 = off)
  posterize: 0.0,    // flat colour levels per channel (0 = off; >=2 quantises)
  halftone: 0.0,     // print dot-screen cell size in px (0 = off)
  lut: 0.0,          // colour map: 0 off · 1 thermal · 2 night-vision · 3 sepia · 4 hologram · 5 duotone
  edge: 0.0,         // comic edge-ink strength (0 = off)
};

// Named post-FX "looks". Each bundle layers over POST_DEFAULTS and only lists
// its deviations; `none` is the shader's own post. A shader can ship in a look
// with meta.post = { "preset": "vhs", ...explicit overrides }. The detail page
// renders these as one-click cards. Keep this the single source of truth.
export const POST_PRESETS = [
  { id: 'none', label: 'None', desc: 'This shader’s own post defaults', post: null },
  { id: 'dreamwave', label: 'Dreamwave', desc: 'Soft neon bloom haze',
    post: { bloom: 1.3, bloomRadius: 6, saturation: 1.3, aberration: 0.0025, grain: 0.04, grainSpeed: 5, vignette: 0.32 } },
  { id: 'crt', label: 'CRT', desc: 'Curved phosphor tube',
    post: { curve: 0.5, scanline: 0.5, crtMask: 0.5, vignette: 0.5, bloom: 0.7, bloomRadius: 4, grain: 0.05, grainSpeed: 10, saturation: 1.1 } },
  { id: 'vhs', label: 'VHS', desc: 'Worn tape, tracking wobble',
    post: { smear: 0.55, wobble: 0.4, scanline: 0.3, grain: 0.13, grainSize: 2, grainSpeed: 16, saturation: 0.8, contrast: 1.12, aberration: 0.003 } },
  { id: 'glitch', label: 'Glitch', desc: 'Digital tearing + channel split',
    post: { glitch: 0.6, aberration: 0.002, grain: 0.07, grainSpeed: 20, contrast: 1.1, saturation: 1.1 } },
  { id: 'thermal', label: 'Thermal', desc: 'Infrared false-colour',
    post: { lut: 1, bloom: 0.4, bloomRadius: 4, grain: 0.03, grainSpeed: 8 } },
  { id: 'nightvision', label: 'Night Vision', desc: 'Green image-intensifier',
    post: { lut: 2, scanline: 0.25, vignette: 0.55, bloom: 0.3, grain: 0.09, grainSpeed: 14 } },
  { id: 'hologram', label: 'Hologram', desc: 'Cyan scanline projection',
    post: { lut: 4, scanline: 0.4, aberration: 0.003, bloom: 0.5, bloomRadius: 4, grain: 0.04 } },
  { id: 'comic', label: 'Comic', desc: 'Posterised + ink outlines',
    post: { posterize: 5, edge: 0.8, saturation: 1.25, contrast: 1.1 } },
  { id: 'riso', label: 'Risograph', desc: 'Halftone print, limited ink',
    post: { halftone: 5, posterize: 4, saturation: 1.1, grain: 0.04, grainSpeed: 6 } },
  { id: 'pixel', label: 'Pixelate', desc: 'Lo-fi bitcrush mosaic',
    post: { pixelate: 6, posterize: 5 } },
  { id: 'duotone', label: 'Duotone', desc: 'Two-tone neon ramp',
    post: { lut: 5, contrast: 1.1, bloom: 0.3 } },
];

// Resolve a meta.post block into a full post state. The block may name a
// `preset` (looked up above) and/or override individual keys; explicit keys win
// over the preset, the preset wins over the global default. Unknown / non-finite
// keys (and the `preset` string itself) are ignored. This is the ONE place post
// is resolved — the live tile, the poster renderer and the detail page all use it.
export function resolvePost(metaPost) {
  const out = { ...POST_DEFAULTS };
  const apply = (obj) => {
    for (const [k, v] of Object.entries(obj || {})) {
      if (k in out && Number.isFinite(v)) out[k] = v;
    }
  };
  const named = metaPost && typeof metaPost.preset === 'string'
    ? POST_PRESETS.find((p) => p.id === metaPost.preset)?.post
    : null;
  apply(named);     // preset bundle over the global default
  apply(metaPost);  // explicit meta.post keys over the preset
  return out;
}

// The post pass. Samples the shader's offscreen texture and applies, cheaply:
//   warp (curve/wobble/glitch) -> aberration -> smear -> bloom -> tone ->
//   vignette -> scanline -> CRT mask -> grain -> dither.
// Every block past aberration/tone/vignette/grain/dither is a "look" primitive
// guarded by its own uniform and skipped wholesale at the default (all 0), so a
// plain shader pays nothing and produces byte-identical output to the base chain.
export const POST_FRAGMENT_SRC = `
precision highp float;
uniform sampler2D u_tex;
uniform vec2  u_resolution;
uniform float u_time;
uniform float u_grain;
uniform float u_grainSize;
uniform float u_grainSpeed;
uniform float u_aberration;
uniform float u_vignette;
uniform float u_exposure;
uniform float u_contrast;
uniform float u_saturation;
uniform float u_bloom;
uniform float u_bloomRadius;
uniform float u_dither;
uniform float u_scanline;
uniform float u_curve;
uniform float u_crtMask;
uniform float u_smear;
uniform float u_wobble;
uniform float u_glitch;
uniform float u_pixelate;
uniform float u_posterize;
uniform float u_halftone;
uniform float u_lut;
uniform float u_edge;
uniform vec3  u_palette[4];
uniform float u_backdrop;
uniform float u_backdropScale;

float hash(vec2 p) {
  p = fract(p * vec2(123.34, 345.45));
  p += dot(p, p + 34.345);
  return fract(p.x * p.y);
}

// interleaved-gradient noise: a cheap, well-distributed 0..1 field for dithering
float ign(vec2 p) {
  return fract(52.9829189 * fract(dot(p, vec2(0.06711056, 0.00583715))));
}

float luma(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }

vec3 hsv2rgb(vec3 c) {
  vec3 p = abs(fract(c.xxx + vec3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0);
  return c.z * mix(vec3(1.0), clamp(p - 1.0, 0.0, 1.0), c.y);
}

// gradient-mesh theme backdrop: four slowly drifting Gaussian colour poles, each
// a palette entry, summed into a soft theme-coloured field — the Aura-family core
// distilled into the post pass so ANY shader can sit on a filled background
// instead of dead black. Only called when u_backdrop > 0, so it costs nothing
// (and stays byte-identical) for the vast majority that leave it off.
vec3 meshBackdrop(vec2 p, float t, float scl) {
  float g = 6.0 / max(scl, 0.05);
  vec2 a = vec2(0.27 + 0.06 * sin(t * 0.070),       0.32 + 0.05 * cos(t * 0.053));
  vec2 b = vec2(0.74 + 0.05 * cos(t * 0.061 + 1.3), 0.66 + 0.06 * sin(t * 0.047 + 0.6));
  vec2 c = vec2(0.56 + 0.07 * sin(t * 0.043 + 2.1), 0.22 + 0.05 * cos(t * 0.058 + 2.7));
  vec2 d = vec2(0.36 + 0.05 * cos(t * 0.055 + 3.4), 0.78 + 0.05 * sin(t * 0.050 + 1.9));
  float wa = exp(-dot(p - a, p - a) * g);
  float wb = exp(-dot(p - b, p - b) * g);
  float wc = exp(-dot(p - c, p - c) * g);
  float wd = exp(-dot(p - d, p - d) * g);
  float ws = wa + wb + wc + wd + 1e-4;
  return (wa * u_palette[0] + wb * u_palette[1] + wc * u_palette[2] + wd * u_palette[3]) / ws;
}

// luma -> colour maps for the LUT looks (mode selected by u_lut, see below)
vec3 colorMap(int mode, float l) {
  if (mode == 1) return hsv2rgb(vec3((1.0 - l) * 0.66, 0.9, clamp(l * 1.4 + 0.15, 0.0, 1.0))); // thermal
  if (mode == 2) return vec3(l * 0.12, clamp(l * 1.35 + 0.04, 0.0, 1.0), l * 0.12);             // night vision
  if (mode == 3) return clamp(vec3(l * 1.12, l * 0.88, l * 0.62), 0.0, 1.0);                    // sepia
  if (mode == 4) return clamp(vec3(l * 0.25, l * 0.95 + 0.06, l * 1.05 + 0.08), 0.0, 1.0);      // hologram cyan
  return mix(vec3(0.07, 0.0, 0.18), vec3(0.95, 0.22, 0.7), l);                                  // duotone
}

void main() {
  vec2 uv = gl_FragCoord.xy / u_resolution; // screen UV (vignette/scanline/grain)
  vec2 suv = uv;                            // sampling UV (warped by the looks)
  vec2 toC = uv - 0.5;

  // pixelate — snap the sampling UV to a coarse block grid (lo-fi mosaic)
  if (u_pixelate >= 1.0) {
    vec2 ps = vec2(u_pixelate) / u_resolution;
    suv = (floor(suv / ps) + 0.5) * ps;
  }
  // CRT barrel curvature — bow the sampling UV outward toward the corners
  if (u_curve > 0.0001) {
    vec2 c = suv * 2.0 - 1.0;
    c *= 1.0 + u_curve * dot(c, c) * 0.25;
    suv = c * 0.5 + 0.5;
  }
  // VHS tape wobble — animated per-row horizontal jitter
  if (u_wobble > 0.0001) {
    float row = floor(gl_FragCoord.y / 2.0);
    float j = hash(vec2(row, floor(u_time * 13.0))) - 0.5;
    suv.x += j * u_wobble * 0.02;
  }
  // glitch — banded horizontal tears, time-gated so only some rows jump
  float gtear = 0.0;
  if (u_glitch > 0.0001) {
    float band = floor(suv.y * 28.0);
    float tg = floor(u_time * 9.0);
    float on = step(0.72, hash(vec2(band, tg)));
    gtear = (hash(vec2(band, tg + 7.0)) - 0.5) * on * u_glitch * 0.12;
    suv.x += gtear;
  }
  // CRT bezel: anything the curve pushed past the edge reads black
  if (u_curve > 0.0001) {
    vec2 e = step(vec2(0.0), suv) * step(suv, vec2(1.0));
    if (e.x * e.y < 0.5) { gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0); return; }
  }

  // chromatic aberration — split RGB along a radius that grows toward the edge;
  // a torn glitch band kicks the split harder and shoves it sideways
  float ca = u_aberration + u_glitch * abs(gtear) * 4.0;
  vec3 col;
  if (ca > 0.0001) {
    vec2 off = toC * ca + vec2(gtear * 0.5, 0.0);
    col.r = texture2D(u_tex, suv + off).r;
    col.g = texture2D(u_tex, suv).g;
    col.b = texture2D(u_tex, suv - off).b;
  } else {
    col = texture2D(u_tex, suv).rgb;
  }

  // gradient-mesh theme backdrop — composite the shader emission over a soft
  // palette colour field, keyed by how near-black the pixel is, so dead-black
  // regions fill with theme colour while lit features stay on top. max() never
  // darkens; guarded so the default (0) is a byte-identical no-op.
  if (u_backdrop > 0.0001) {
    vec3 bg = meshBackdrop(uv, u_time, u_backdropScale);
    float empt = 1.0 - smoothstep(0.02, 0.22, luma(col));
    col = mix(col, max(col, bg * empt), u_backdrop);
  }

  // VHS chroma smear — a short leftward colour trail (tape can't hold chroma)
  if (u_smear > 0.0001) {
    vec3 sm = vec3(0.0);
    for (int i = 1; i <= 4; i++) {
      sm += texture2D(u_tex, suv - vec2(float(i) * u_smear * 6.0 / u_resolution.x, 0.0)).rgb;
    }
    col = mix(col, sm * 0.25, u_smear * 0.6);
  }

  // edge ink — Sobel-ish luma gradient draws comic outlines into the image
  if (u_edge > 0.0001) {
    vec2 tx = 1.0 / u_resolution;
    float gx = luma(texture2D(u_tex, suv + vec2(tx.x, 0.0)).rgb)
             - luma(texture2D(u_tex, suv - vec2(tx.x, 0.0)).rgb);
    float gy = luma(texture2D(u_tex, suv + vec2(0.0, tx.y)).rgb)
             - luma(texture2D(u_tex, suv - vec2(0.0, tx.y)).rgb);
    float e = clamp(length(vec2(gx, gy)) * u_edge * 6.0, 0.0, 1.0);
    col = mix(col, vec3(0.0), e);
  }

  // colour map (LUT) — remap luminance to a look palette (1 thermal · 2 night
  // vision · 3 sepia · 4 hologram · 5 duotone). Fully replaces hue when active.
  if (u_lut >= 0.5) {
    col = colorMap(int(u_lut + 0.5), luma(col));
  }

  // bloom — cheap single-pass glow: a golden-angle disk of taps, thresholded
  // so only highlights bleed. Whole block is skipped when bloom is off.
  if (u_bloom > 0.0001) {
    vec3 sum = vec3(0.0);
    for (int i = 0; i < 12; i++) {
      float fi = float(i) + 0.5;
      float ang = fi * 2.39996323;
      float rad = sqrt(fi / 12.0) * u_bloomRadius;
      vec2 o = vec2(cos(ang), sin(ang)) * rad / u_resolution;
      vec3 s = texture2D(u_tex, suv + o).rgb;
      float b = max(max(max(s.r, s.g), s.b) - 0.55, 0.0);
      sum += s * b;
    }
    col += sum * (u_bloom / 12.0);
  }

  // tone — exposure, then contrast about mid-grey, then saturation
  col *= u_exposure;
  col = (col - 0.5) * u_contrast + 0.5;
  float l = dot(col, vec3(0.299, 0.587, 0.114));
  col = mix(vec3(l), col, u_saturation);

  // posterize — quantise each channel to u_posterize flat levels (comic/print)
  if (u_posterize >= 1.5) {
    float n = u_posterize - 1.0;
    col = floor(clamp(col, 0.0, 1.0) * n + 0.5) / n;
  }

  // vignette — quadratic edge falloff
  float vig = 1.0 - u_vignette * dot(toC, toC) * 2.0;
  col *= clamp(vig, 0.0, 1.0);

  // scanline — darken alternate device rows (CRT/VHS line structure)
  if (u_scanline > 0.0001) {
    float s = sin(gl_FragCoord.y * 3.14159265) * 0.5 + 0.5;
    col *= 1.0 - u_scanline * s * 0.6;
  }
  // CRT phosphor mask — tint successive device columns toward R / G / B
  if (u_crtMask > 0.0001) {
    float m = mod(gl_FragCoord.x, 3.0);
    vec3 mask = m < 1.0 ? vec3(1.0, 0.7, 0.7)
              : (m < 2.0 ? vec3(0.7, 1.0, 0.7) : vec3(0.7, 0.7, 1.0));
    col *= mix(vec3(1.0), mask, u_crtMask);
  }

  // halftone — print-style dots on a cell grid, each dot growing with local
  // luminance; outside the dot falls to ink-dark. u_halftone is the cell px.
  if (u_halftone >= 1.0) {
    vec2 q = fract(gl_FragCoord.xy / u_halftone) - 0.5;
    float dot = length(q);
    float r = sqrt(clamp(luma(col), 0.0, 1.0)) * 0.7;
    float ink = smoothstep(r + 0.06, r - 0.06, dot); // 1 inside the dot
    col *= ink;
  }

  // film grain — a fresh, decorrelated field every 1/u_grainSpeed sec so it
  // shimmers at a controllable rate (0 = frozen/static); u_grainSize chunks the
  // cells so the boil is visible. Sign-centred (darkens and lightens equally),
  // eased out of deep shadow and blown highlight so it never muddies the extremes.
  if (u_grain > 0.0001) {
    vec2 cell = floor(gl_FragCoord.xy / max(u_grainSize, 1.0));
    float fr = floor(u_time * u_grainSpeed);
    float n = hash(cell + fr * vec2(63.7, 31.3)) - 0.5;
    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    float vis = clamp(1.0 - abs(lum - 0.5) * 1.1, 0.4, 1.0);
    col += n * u_grain * vis;
  }

  // dither — triangular noise at the 8-bit quantum, breaks the banding the
  // RGBA8 buffer shows on smooth gradients. u_dither is measured in LSBs.
  if (u_dither > 0.0001) {
    float d = ign(gl_FragCoord.xy + fract(u_time)) - 0.5;
    col += d * u_dither / 255.0;
  }

  gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
`;

const MAX_DPR = 2;
const PALETTE_LEN = 4;

// WEBGL_lose_context can only be grabbed while the context is ALIVE —
// getExtension() returns null once it's lost. The gallery reuses a canvas's
// context across scroll-out/scroll-in (a canvas only ever hands out one
// context), so cache the extension per canvas the first time we see it alive.
// We then use it both to free the slot on unmount (loseContext) and, crucially,
// to revive that same context on re-mount (restoreContext) — without the cache
// the revival call no-ops and the tile is stuck on its poster.
const loseCtxExt = new WeakMap(); // canvas -> WEBGL_lose_context
// A context whose webglcontextlost event was NOT preventDefault()'d can never be
// restoreContext()'d ("context restoration not allowed"). We free a tile's slot
// with loseContext() on unmount and want it back on re-mount, so every canvas
// keeps ONE permanent preventDefault listener (the per-mount handlers come and go
// with destroy(), which would otherwise leave the freeing loss un-prevented).
const pdAttached = new WeakSet(); // canvases with the permanent preventDefault
function keepRestorable(canvas) {
  if (pdAttached.has(canvas)) return;
  canvas.addEventListener('webglcontextlost', (e) => e.preventDefault(), false);
  pdAttached.add(canvas);
}

function compile(gl, type, src) {
  const sh = gl.createShader(type);
  gl.shaderSource(sh, src);
  gl.compileShader(sh);
  if (!gl.getShaderParameter(sh, gl.COMPILE_STATUS)) {
    const log = gl.getShaderInfoLog(sh);
    gl.deleteShader(sh);
    // A lost context (the browser evicted it to stay under its ~16-context cap)
    // also reports COMPILE_STATUS=false with an empty log — that's transient, not
    // a real GLSL error, so tag it and let the caller retry instead of giving up.
    const err = new Error(`shader compile failed:\n${log}`);
    if (gl.isContextLost()) err.contextLost = true;
    throw err;
  }
  return sh;
}

function link(gl, vert, frag) {
  const program = gl.createProgram();
  gl.attachShader(program, vert);
  gl.attachShader(program, frag);
  gl.linkProgram(program);
  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(`link failed:\n${gl.getProgramInfoLog(program)}`);
  }
  return program;
}

function flatPalette(palette) {
  const out = new Float32Array(PALETTE_LEN * 3);
  for (let i = 0; i < PALETTE_LEN; i++) {
    const c = palette[i] || palette[palette.length - 1] || [0, 0, 0];
    out[i * 3 + 0] = c[0];
    out[i * 3 + 1] = c[1];
    out[i * 3 + 2] = c[2];
  }
  return out;
}

export function createShaderTile(canvas, fragSource, opts = {}) {
  const gl = canvas.getContext('webgl', { antialias: true, alpha: false });
  if (!gl) throw new Error('WebGL unavailable');

  // GL objects (programs/buffers/uniform locations) are owned by the context, so
  // they all die if the browser loses it (e.g. evicts it to stay under its
  // ~16-context cap). Keep them in `let`s rebuilt by buildGL() so a
  // `webglcontextrestored` event can revive the tile instead of bricking it.
  let program, postProgram, buf, aPos, aPosPost, u, pu;
  // custom float uniforms: values persist across a context loss; locations don't,
  // so re-query them into extraLoc each rebuild.
  const uniformValues = new Map(Object.entries(opts.uniforms || {}));
  const extraLoc = new Map(); // name -> location (rebuilt by buildGL)

  function buildGL() {
    // one vertex shader, shared by the shader pass and the post pass
    const vert = compile(gl, gl.VERTEX_SHADER, VERTEX_SRC);
    program = link(gl, vert, compile(gl, gl.FRAGMENT_SHADER, fragSource));
    postProgram = link(gl, vert, compile(gl, gl.FRAGMENT_SHADER, POST_FRAGMENT_SRC));

    // fullscreen triangle, shared by both passes
    buf = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1, -1, 3, -1, -1, 3]), gl.STATIC_DRAW);
    aPos = gl.getAttribLocation(program, 'a_pos');
    aPosPost = gl.getAttribLocation(postProgram, 'a_pos');

    u = {
      time: gl.getUniformLocation(program, 'u_time'),
      res: gl.getUniformLocation(program, 'u_resolution'),
      mouse: gl.getUniformLocation(program, 'u_mouse'),
      pr: gl.getUniformLocation(program, 'u_pixelRatio'),
      // array uniforms must be queried as "name[0]" — some GL stacks (headless-gl,
      // strict ANGLE) return null for the bare "u_palette".
      palette: gl.getUniformLocation(program, 'u_palette[0]'),
    };
    pu = {
      tex: gl.getUniformLocation(postProgram, 'u_tex'),
      res: gl.getUniformLocation(postProgram, 'u_resolution'),
      time: gl.getUniformLocation(postProgram, 'u_time'),
      grain: gl.getUniformLocation(postProgram, 'u_grain'),
      grainSize: gl.getUniformLocation(postProgram, 'u_grainSize'),
      grainSpeed: gl.getUniformLocation(postProgram, 'u_grainSpeed'),
      aberration: gl.getUniformLocation(postProgram, 'u_aberration'),
      vignette: gl.getUniformLocation(postProgram, 'u_vignette'),
      exposure: gl.getUniformLocation(postProgram, 'u_exposure'),
      contrast: gl.getUniformLocation(postProgram, 'u_contrast'),
      saturation: gl.getUniformLocation(postProgram, 'u_saturation'),
      bloom: gl.getUniformLocation(postProgram, 'u_bloom'),
      bloomRadius: gl.getUniformLocation(postProgram, 'u_bloomRadius'),
      dither: gl.getUniformLocation(postProgram, 'u_dither'),
      scanline: gl.getUniformLocation(postProgram, 'u_scanline'),
      curve: gl.getUniformLocation(postProgram, 'u_curve'),
      crtMask: gl.getUniformLocation(postProgram, 'u_crtMask'),
      smear: gl.getUniformLocation(postProgram, 'u_smear'),
      wobble: gl.getUniformLocation(postProgram, 'u_wobble'),
      glitch: gl.getUniformLocation(postProgram, 'u_glitch'),
      pixelate: gl.getUniformLocation(postProgram, 'u_pixelate'),
      posterize: gl.getUniformLocation(postProgram, 'u_posterize'),
      halftone: gl.getUniformLocation(postProgram, 'u_halftone'),
      lut: gl.getUniformLocation(postProgram, 'u_lut'),
      edge: gl.getUniformLocation(postProgram, 'u_edge'),
      backdrop: gl.getUniformLocation(postProgram, 'u_backdrop'),
      backdropScale: gl.getUniformLocation(postProgram, 'u_backdropScale'),
      // the backdrop tints with the shader's palette → feed it to the post pass too
      palette: gl.getUniformLocation(postProgram, 'u_palette[0]'),
    };

    extraLoc.clear();
    for (const name of uniformValues.keys()) {
      const loc = gl.getUniformLocation(program, name);
      if (loc) extraLoc.set(name, loc);
    }
  }
  // A canvas only ever hands out ONE context. If this tile was mounted before,
  // destroy() lost that context to free a slot (browsers cap simultaneous
  // contexts and never auto-restore one lost to the cap), so getContext() above
  // just handed back the SAME dead context — building on it would throw "shader
  // failed to compile", which is the bug this fixes. Detect that and revive it
  // ourselves with restoreContext() (issued once the listeners are attached,
  // below); the webglcontextrestored handler then builds. A fresh context that's
  // alive just builds now.
  keepRestorable(canvas); // ensure our future loseContext() stays restorable

  let lost = gl.isContextLost();
  // cache the lose/restore extension while the context is alive (see loseCtxExt)
  let loseExt = loseCtxExt.get(canvas);
  if (!loseExt && !lost) {
    loseExt = gl.getExtension('WEBGL_lose_context');
    if (loseExt) loseCtxExt.set(canvas, loseExt);
  }
  if (!lost) buildGL();

  let palette = flatPalette(opts.palette || [[0.5, 0.5, 0.5]]);
  // post-FX state: resolve a meta.post block (may name a `preset` + overrides)
  const post = resolvePost(opts.post);

  // offscreen framebuffer the shader draws into; the post pass samples it
  let fbo = null;
  let fboW = 0;
  let fboH = 0;
  function makeFBO(w, h) {
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    const fb = gl.createFramebuffer();
    gl.bindFramebuffer(gl.FRAMEBUFFER, fb);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex, 0);
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    return { tex, fb };
  }

  const mouse = [0, 0];
  let raf = null;
  let startMs = null;
  let dpr = 1;
  // callers can cap DPR below the global ceiling for big, decorative tiles
  // (e.g. the landing hero panels behind a scrim) to bound their fill cost
  const maxDpr = Math.min(opts.maxDpr || MAX_DPR, MAX_DPR);

  function resize() {
    if (lost) return; // no GL objects to size while the context is gone
    dpr = Math.min(window.devicePixelRatio || 1, maxDpr);
    const rect = canvas.getBoundingClientRect();
    const w = Math.max(1, Math.round(rect.width * dpr));
    const h = Math.max(1, Math.round(rect.height * dpr));
    if (canvas.width !== w || canvas.height !== h) {
      canvas.width = w;
      canvas.height = h;
    }
    // the offscreen texture tracks the drawing-buffer size (recreated on resize)
    if (!fbo || fboW !== canvas.width || fboH !== canvas.height) {
      if (fbo) {
        gl.deleteTexture(fbo.tex);
        gl.deleteFramebuffer(fbo.fb);
      }
      fbo = makeFBO(canvas.width, canvas.height);
      fboW = canvas.width;
      fboH = canvas.height;
    }
  }

  function frame(now) {
    if (lost) { raf = null; return; } // context gone; restore handler resumes us
    if (startMs === null) startMs = now;
    // NB: sizing is driven by the ResizeObserver (below), not polled here — a
    // per-frame getBoundingClientRect() forces a layout flush on every tile every
    // frame, and during an animated resize it also reallocated the FBO 60×/s.
    const t = (now - startMs) / 1000;

    // pass 1 — shader into the offscreen texture
    gl.bindFramebuffer(gl.FRAMEBUFFER, fbo.fb);
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.useProgram(program);
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.enableVertexAttribArray(aPos);
    gl.vertexAttribPointer(aPos, 2, gl.FLOAT, false, 0, 0);
    gl.uniform1f(u.time, t);
    gl.uniform2f(u.res, canvas.width, canvas.height);
    gl.uniform2f(u.mouse, mouse[0], mouse[1]);
    gl.uniform1f(u.pr, dpr);
    if (u.palette) gl.uniform3fv(u.palette, palette);
    for (const [name, value] of uniformValues) {
      const loc = extraLoc.get(name);
      if (loc) gl.uniform1f(loc, value);
    }
    gl.drawArrays(gl.TRIANGLES, 0, 3);

    // pass 2 — post-process onto the canvas
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.useProgram(postProgram);
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.enableVertexAttribArray(aPosPost);
    gl.vertexAttribPointer(aPosPost, 2, gl.FLOAT, false, 0, 0);
    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, fbo.tex);
    gl.uniform1i(pu.tex, 0);
    gl.uniform2f(pu.res, canvas.width, canvas.height);
    gl.uniform1f(pu.time, t);
    gl.uniform1f(pu.grain, post.grain);
    gl.uniform1f(pu.grainSize, post.grainSize);
    gl.uniform1f(pu.grainSpeed, post.grainSpeed);
    gl.uniform1f(pu.aberration, post.aberration);
    gl.uniform1f(pu.vignette, post.vignette);
    gl.uniform1f(pu.exposure, post.exposure);
    gl.uniform1f(pu.contrast, post.contrast);
    gl.uniform1f(pu.saturation, post.saturation);
    gl.uniform1f(pu.bloom, post.bloom);
    gl.uniform1f(pu.bloomRadius, post.bloomRadius);
    gl.uniform1f(pu.dither, post.dither);
    gl.uniform1f(pu.scanline, post.scanline);
    gl.uniform1f(pu.curve, post.curve);
    gl.uniform1f(pu.crtMask, post.crtMask);
    gl.uniform1f(pu.smear, post.smear);
    gl.uniform1f(pu.wobble, post.wobble);
    gl.uniform1f(pu.glitch, post.glitch);
    gl.uniform1f(pu.pixelate, post.pixelate);
    gl.uniform1f(pu.posterize, post.posterize);
    gl.uniform1f(pu.halftone, post.halftone);
    gl.uniform1f(pu.lut, post.lut);
    gl.uniform1f(pu.edge, post.edge);
    gl.uniform1f(pu.backdrop, post.backdrop);
    gl.uniform1f(pu.backdropScale, post.backdropScale);
    if (pu.palette) gl.uniform3fv(pu.palette, palette);
    gl.drawArrays(gl.TRIANGLES, 0, 3);

    raf = requestAnimationFrame(frame);
  }

  // FancyBlazor may explicitly opt out of offscreen pausing. The upstream
  // renderer always observes the canvas; keep that default while allowing the
  // package-level option to select an always-on renderer.
  const pauseWhenOffscreen = opts.pauseWhenOffscreen !== false;
  let onScreen = !pauseWhenOffscreen;
  function start() {
    if (lost) return; // nothing to draw into until the context comes back
    if (raf === null) raf = requestAnimationFrame(frame);
  }
  function stop() {
    if (raf !== null) {
      cancelAnimationFrame(raf);
      raf = null;
      startMs = null; // resume cleanly without a time jump
    }
  }

  function onMove(e) {
    const rect = canvas.getBoundingClientRect();
    mouse[0] = (e.clientX - rect.left) * dpr;
    mouse[1] = (rect.height - (e.clientY - rect.top)) * dpr; // flip to gl_FragCoord space
  }
  canvas.addEventListener('pointermove', onMove);

  // If the browser drops our context (e.g. it evicted us to stay under the
  // ~16-context cap), preventDefault keeps the canvas eligible for restoration;
  // drop the now-dead GL handles and pause. On restore, rebuild and resume if
  // we're still on screen — so an evicted tile heals itself instead of going
  // black or stuck on "failed".
  let restorePending = false;
  function onLost(e) {
    e.preventDefault(); // keep the canvas eligible for restoreContext()
    lost = true;
    stop();
    fbo = null; fboW = 0; fboH = 0; // texture/framebuffer died with the context
    // The browser dropped us to stay under its context cap, and it NEVER
    // auto-restores a capacity loss — so ask for the context back ourselves. Do
    // it on a short delay (coalesced) so we don't fight an active scroll: once it
    // settles there's capacity and the restore sticks; onRestored then rebuilds.
    if (!restorePending) {
      restorePending = true;
      setTimeout(() => {
        restorePending = false;
        if (lost) loseExt?.restoreContext();
      }, 200);
    }
  }
  function onRestored() {
    fbo = null; fboW = 0; fboH = 0; // force a fresh FBO for the new context
    lost = false;
    try {
      buildGL();
    } catch (err) {
      // lost again mid-restore (still over budget) — stay paused for the next event
      if (err && err.contextLost) { lost = true; return; }
      throw err;
    }
    resize(); // recreates the offscreen FBO at the current size
    if (onScreen) start();
  }
  canvas.addEventListener('webglcontextlost', onLost, false);
  canvas.addEventListener('webglcontextrestored', onRestored, false);

  // recycled canvas with a dead context (see above): ask for it back now that
  // onRestored is wired up to rebuild when it returns. Use the cached extension —
  // getExtension() would return null on the already-lost context.
  if (lost) loseExt?.restoreContext();

  // only run while visible
  const io = pauseWhenOffscreen
    ? new IntersectionObserver(
        (entries) => {
          for (const en of entries) {
            onScreen = en.isIntersecting;
            onScreen ? start() : stop();
          }
        },
        { threshold: 0.01 }
      )
    : null;
  io?.observe(canvas);
  if (!pauseWhenOffscreen) start();

  const ro = new ResizeObserver(resize);
  ro.observe(canvas);
  resize();

  return {
    // FancyBlazor diagnostics use this to prove the offscreen RAF gate. This is
    // deliberately not part of the public .NET API.
    isRunning() {
      return raf !== null;
    },
    setPalette(next) {
      palette = flatPalette(next);
    },
    setUniform(name, value) {
      if (uniformValues.has(name) && Number.isFinite(value)) uniformValues.set(name, value);
    },
    setPost(name, value) {
      if (name in post && Number.isFinite(value)) post[name] = value;
    },
    destroy() {
      stop();
      io?.disconnect();
      ro.disconnect();
      canvas.removeEventListener('pointermove', onMove);
      canvas.removeEventListener('webglcontextlost', onLost);
      canvas.removeEventListener('webglcontextrestored', onRestored);
      if (fbo) {
        gl.deleteTexture(fbo.tex);
        gl.deleteFramebuffer(fbo.fb);
      }
      loseExt?.loseContext(); // frees the slot; cached so re-mount can restore it
    },
  };
}

// ── mash engine (foundry "apple-pen") ────────────────────────────────────────
// Combine TWO (or more) WHOLE shaders into one image. Each layer renders to its
// own offscreen texture; a "blend" pass joins them with a chosen operator; the
// shared post pass finishes onto the canvas. Unlike the breeder — which crosses
// one shader's PARAM VECTOR against itself — this crosses different GLSL programs
// by compositing their outputs, so any two catalog shaders combine and it always
// compiles. "On repeat" (PPAP) falls out for free: pass [A, B, C, …] with one op
// between each pair and they fold left — mix(mix(A,B), C) — so the result of one
// mash is the A of the next.

// Blend operators (the gene that makes two shaders read as ONE object, not a
// crossfade of two videos). `op` index → MIX_FRAGMENT_SRC branch below.
export const BLEND_MODES = [
  { id: 'crossfade',  label: 'Crossfade',   hint: 'plain linear blend' },
  { id: 'screen',     label: 'Screen',      hint: 'light on light — both glows survive' },
  { id: 'multiply',   label: 'Multiply',    hint: 'ink on ink — darkens to overlap' },
  { id: 'add',        label: 'Add',         hint: 'additive — hot where they stack' },
  { id: 'lighten',    label: 'Lighten',     hint: 'per-pixel brighter of the two' },
  { id: 'darken',     label: 'Darken',      hint: 'per-pixel darker of the two' },
  { id: 'difference', label: 'Difference',  hint: 'abs diff — edges/interference' },
  { id: 'lumakey',    label: 'Luma key',    hint: 'B shows only where A is bright' },
  { id: 'warp',       label: 'Domain warp', hint: "A's colour bends B's geometry" },
  { id: 'chroma',     label: 'Chroma swap', hint: "A's colour × B's brightness" },
];
export const BLEND_IDS = BLEND_MODES.map((m) => m.id);
const blendIndex = (id) => Math.max(0, BLEND_IDS.indexOf(id));

const MIX_FRAGMENT_SRC = `
precision highp float;
uniform sampler2D u_texA;   // accumulator so far
uniform sampler2D u_texB;   // this layer
uniform vec2  u_resolution;
uniform float u_blend;      // operator index (see BLEND_MODES)
uniform float u_amount;     // 0 = just A, 1 = full blended result
uniform float u_warp;       // domain-warp strength (uv units), op 8
float luma(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }
void main() {
  vec2 uv = gl_FragCoord.xy / u_resolution;
  vec3 a = texture2D(u_texA, uv).rgb;
  // B can be sampled along a warp driven by A; harmless (offset 0) unless op 8.
  vec2 off = (u_blend > 7.5 && u_blend < 8.5) ? (a.xy - 0.5) * u_warp : vec2(0.0);
  vec3 b = texture2D(u_texB, uv + off).rgb;
  vec3 r;
  if      (u_blend < 0.5) r = mix(a, b, 0.5);
  else if (u_blend < 1.5) r = 1.0 - (1.0 - a) * (1.0 - b);
  else if (u_blend < 2.5) r = a * b;
  else if (u_blend < 3.5) r = a + b;
  else if (u_blend < 4.5) r = max(a, b);
  else if (u_blend < 5.5) r = min(a, b);
  else if (u_blend < 6.5) r = abs(a - b);
  else if (u_blend < 7.5) r = mix(a, b, smoothstep(0.12, 0.6, luma(a)));
  else if (u_blend < 8.5) r = b;                                   // warped above
  else                    r = luma(b) * (a / max(luma(a), 1e-3));  // A chroma · B luma
  gl_FragColor = vec4(clamp(mix(a, r, u_amount), 0.0, 1.0), 1.0);
}
`;

// layers: [{ frag, palette, uniforms }]  (>= 1; the breeder genome shape extends
//   trivially — frag is the template's source, uniforms the evolved values).
// ops:    [{ blend, amount, warp }]  length layers.length - 1 (one per join).
// opts:   { post }  — same meta.post block the gallery resolves.
export function createMashTile(canvas, layers, ops = [], opts = {}) {
  const gl = canvas.getContext('webgl', { antialias: true, alpha: false });
  if (!gl) throw new Error('WebGL unavailable');
  if (!layers || layers.length < 1) throw new Error('mash needs at least one layer');

  const vert = compile(gl, gl.VERTEX_SHADER, VERTEX_SRC);
  const buf = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, buf);
  gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1, -1, 3, -1, -1, 3]), gl.STATIC_DRAW);

  // one layer = its own program + palette + extra float uniforms (compiled here;
  // a frag compile error throws, surfaced to the caller as a failed tile).
  function buildLayer(layer) {
    const program = link(gl, vert, compile(gl, gl.FRAGMENT_SHADER, layer.frag));
    const u = {
      time: gl.getUniformLocation(program, 'u_time'),
      res: gl.getUniformLocation(program, 'u_resolution'),
      mouse: gl.getUniformLocation(program, 'u_mouse'),
      pr: gl.getUniformLocation(program, 'u_pixelRatio'),
      palette: gl.getUniformLocation(program, 'u_palette[0]'),
    };
    const aPos = gl.getAttribLocation(program, 'a_pos');
    const extra = new Map();
    for (const [name, value] of Object.entries(layer.uniforms || {})) {
      const loc = gl.getUniformLocation(program, name);
      if (loc) extra.set(name, { loc, value });
    }
    return { program, u, aPos, extra, palette: flatPalette(layer.palette || [[0.5, 0.5, 0.5]]) };
  }
  let built = layers.map(buildLayer);

  // mix + post programs
  const mixProgram = link(gl, vert, compile(gl, gl.FRAGMENT_SHADER, MIX_FRAGMENT_SRC));
  const postProgram = link(gl, vert, compile(gl, gl.FRAGMENT_SHADER, POST_FRAGMENT_SRC));
  const aPosMix = gl.getAttribLocation(mixProgram, 'a_pos');
  const aPosPost = gl.getAttribLocation(postProgram, 'a_pos');
  const mu = {
    texA: gl.getUniformLocation(mixProgram, 'u_texA'),
    texB: gl.getUniformLocation(mixProgram, 'u_texB'),
    res: gl.getUniformLocation(mixProgram, 'u_resolution'),
    blend: gl.getUniformLocation(mixProgram, 'u_blend'),
    amount: gl.getUniformLocation(mixProgram, 'u_amount'),
    warp: gl.getUniformLocation(mixProgram, 'u_warp'),
  };
  // map state-key → actual uniform name (most are `u_<key>`; `res` is the
  // exception — the uniform is `u_resolution`, NOT `u_res`). Getting this wrong
  // leaves u_resolution at (0,0) → the post computes uv = gl_FragCoord/0 → NaN →
  // a black frame, which is exactly what a mis-mapped `res` caused (2026-06-12).
  const pu = {};
  for (const k of ['tex', 'res', 'time', 'grain', 'grainSize', 'grainSpeed', 'aberration',
    'vignette', 'exposure', 'contrast', 'saturation', 'bloom', 'bloomRadius', 'dither',
    'scanline', 'curve', 'crtMask', 'smear', 'wobble', 'glitch', 'pixelate', 'posterize',
    'halftone', 'lut', 'edge', 'backdrop', 'backdropScale']) {
    pu[k] = gl.getUniformLocation(postProgram, k === 'res' ? 'u_resolution' : 'u_' + k);
  }
  // the gradient-mesh backdrop tints with a palette → feed the first layer's
  pu.palette = gl.getUniformLocation(postProgram, 'u_palette[0]');
  const post = resolvePost(opts.post);
  const joins = ops.map((o) => ({ blend: o.blend ?? 'crossfade', amount: o.amount ?? 1, warp: o.warp ?? 0.2 }));

  // FBO pool: two ping-pong accumulators + one scratch for the current layer.
  function makeFBO(w, h) {
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    const fb = gl.createFramebuffer();
    gl.bindFramebuffer(gl.FRAMEBUFFER, fb);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex, 0);
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    return { tex, fb };
  }
  let pool = null; // { acc:[fbo,fbo], scratch:fbo }
  let poolW = 0, poolH = 0;
  function freePool() {
    if (!pool) return;
    for (const f of [...pool.acc, pool.scratch]) { gl.deleteTexture(f.tex); gl.deleteFramebuffer(f.fb); }
    pool = null;
  }

  const mouse = [0, 0];
  let raf = null, startMs = null, dpr = 1;

  function resize() {
    dpr = Math.min(window.devicePixelRatio || 1, MAX_DPR);
    const rect = canvas.getBoundingClientRect();
    const w = Math.max(1, Math.round(rect.width * dpr));
    const h = Math.max(1, Math.round(rect.height * dpr));
    if (canvas.width !== w || canvas.height !== h) { canvas.width = w; canvas.height = h; }
    if (!pool || poolW !== canvas.width || poolH !== canvas.height) {
      freePool();
      pool = { acc: [makeFBO(canvas.width, canvas.height), makeFBO(canvas.width, canvas.height)], scratch: makeFBO(canvas.width, canvas.height) };
      poolW = canvas.width; poolH = canvas.height;
    }
  }

  // draw one layer's shader into the bound framebuffer
  function drawLayer(L, t) {
    gl.useProgram(L.program);
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.enableVertexAttribArray(L.aPos);
    gl.vertexAttribPointer(L.aPos, 2, gl.FLOAT, false, 0, 0);
    gl.uniform1f(L.u.time, t);
    gl.uniform2f(L.u.res, canvas.width, canvas.height);
    gl.uniform2f(L.u.mouse, mouse[0], mouse[1]);
    gl.uniform1f(L.u.pr, dpr);
    if (L.u.palette) gl.uniform3fv(L.u.palette, L.palette);
    for (const e of L.extra.values()) gl.uniform1f(e.loc, e.value);
    gl.drawArrays(gl.TRIANGLES, 0, 3);
  }

  function frame(now) {
    if (startMs === null) startMs = now;
    // sizing handled by the ResizeObserver (below) — see createShaderTile.frame
    const t = (now - startMs) / 1000;
    const vw = canvas.width, vh = canvas.height;

    // fold the layers: render layer 0 into acc[cur], then for each join render
    // the next layer into scratch and mix(acc[cur], scratch) → acc[1-cur].
    let cur = 0;
    gl.bindFramebuffer(gl.FRAMEBUFFER, pool.acc[cur].fb);
    gl.viewport(0, 0, vw, vh);
    drawLayer(built[0], t);

    for (let i = 1; i < built.length; i++) {
      gl.bindFramebuffer(gl.FRAMEBUFFER, pool.scratch.fb);
      gl.viewport(0, 0, vw, vh);
      drawLayer(built[i], t);

      const join = joins[i - 1] || { blend: 'crossfade', amount: 1, warp: 0 };
      const dst = pool.acc[1 - cur];
      gl.bindFramebuffer(gl.FRAMEBUFFER, dst.fb);
      gl.viewport(0, 0, vw, vh);
      gl.useProgram(mixProgram);
      gl.bindBuffer(gl.ARRAY_BUFFER, buf);
      gl.enableVertexAttribArray(aPosMix);
      gl.vertexAttribPointer(aPosMix, 2, gl.FLOAT, false, 0, 0);
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, pool.acc[cur].tex);
      gl.uniform1i(mu.texA, 0);
      gl.activeTexture(gl.TEXTURE1);
      gl.bindTexture(gl.TEXTURE_2D, pool.scratch.tex);
      gl.uniform1i(mu.texB, 1);
      gl.uniform2f(mu.res, vw, vh);
      gl.uniform1f(mu.blend, blendIndex(join.blend));
      gl.uniform1f(mu.amount, join.amount);
      gl.uniform1f(mu.warp, join.warp);
      gl.drawArrays(gl.TRIANGLES, 0, 3);
      cur = 1 - cur;
    }

    // post pass → canvas, sampling the final accumulator
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    gl.viewport(0, 0, vw, vh);
    gl.useProgram(postProgram);
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.enableVertexAttribArray(aPosPost);
    gl.vertexAttribPointer(aPosPost, 2, gl.FLOAT, false, 0, 0);
    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, pool.acc[cur].tex);
    gl.uniform1i(pu.tex, 0);
    gl.uniform2f(pu.res, vw, vh);
    gl.uniform1f(pu.time, t);
    for (const k of Object.keys(post)) if (pu[k]) gl.uniform1f(pu[k], post[k]);
    if (pu.palette) gl.uniform3fv(pu.palette, built[0].palette);
    gl.drawArrays(gl.TRIANGLES, 0, 3);

    raf = requestAnimationFrame(frame);
  }

  function start() { if (raf === null) raf = requestAnimationFrame(frame); }
  function stop() { if (raf !== null) { cancelAnimationFrame(raf); raf = null; startMs = null; } }

  function onMove(e) {
    const rect = canvas.getBoundingClientRect();
    mouse[0] = (e.clientX - rect.left) * dpr;
    mouse[1] = (rect.height - (e.clientY - rect.top)) * dpr;
  }
  canvas.addEventListener('pointermove', onMove);

  const io = new IntersectionObserver((entries) => {
    for (const en of entries) en.isIntersecting ? start() : stop();
  }, { threshold: 0.01 });
  io.observe(canvas);
  const ro = new ResizeObserver(resize);
  ro.observe(canvas);
  resize();

  return {
    layerCount: () => built.length,
    // continuous controls — no program churn
    setBlend(i, id) { if (joins[i]) joins[i].blend = id; },
    setAmount(i, v) { if (joins[i] && Number.isFinite(v)) joins[i].amount = v; },
    setWarp(i, v) { if (joins[i] && Number.isFinite(v)) joins[i].warp = v; },
    setLayerPalette(i, palette) { if (built[i]) built[i].palette = flatPalette(palette); },
    setLayerUniform(i, name, value) {
      const e = built[i]?.extra.get(name);
      if (e && Number.isFinite(value)) e.value = value;
    },
    setPost(name, value) { if (name in post && Number.isFinite(value)) post[name] = value; },
    destroy() {
      stop();
      io.disconnect();
      ro.disconnect();
      canvas.removeEventListener('pointermove', onMove);
      freePool();
      gl.getExtension('WEBGL_lose_context')?.loseContext();
    },
  };
}
