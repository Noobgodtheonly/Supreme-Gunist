shader_type canvas_item;

// The virtual low resolution to simulate. For a 1280x720 screen, 160x90 is a good ratio.
uniform vec2 virtual_resolution = vec2(160.0, 90.0);

// The spacing of the high-resolution scanlines. A value of 2.0 means a line every 2 pixels.
uniform float scanline_spacing = 2.0;

// The thickness of the scanline. A value of 1.0 makes it a single pixel thick.
uniform float scanline_thickness = 1.0;

// The darkness of the scanlines. A value around 0.4 to 0.7 looks more natural.
uniform float scanline_strength = 0.5;

// The amount of barrel distortion for a lens-like curvature.
uniform float barrel_distortion = 0.15;

// The amount of static/noise to add to the screen. A small value like 0.05 is usually enough.
uniform float noise_strength = 0.01;

// The strength of the vignette (darkened edges). A value between 0.5 and 1.5 is a good range.
uniform float vignette_strength = 0.8;

// The strength of the glow/bloom effect on bright pixels.
uniform float bloom_strength = 3;

// A simple random function to generate noise.
float random(vec2 uv) {
    return fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    // 1. Apply Barrel Distortion with subtle wobble for a lens-like effect.
    vec2 uv = UV;
    vec2 center = vec2(0.5, 0.5);
    vec2 offset = uv - center;
    float dist = dot(offset, offset);
    // The sin(TIME) adds a subtle, gentle wobble over time.
    uv += offset * dist * (barrel_distortion + sin(TIME) * 0.01);

    // 2. Pixelate the image.
    vec2 screen_size = 1.0 / SCREEN_PIXEL_SIZE;
    vec2 pixel_size = screen_size / virtual_resolution;
    vec2 uv_snapped = floor(uv * virtual_resolution) / virtual_resolution;
    
    // 3. Sample the color.
    vec4 color = texture(TEXTURE, uv_snapped);

    // This is the key change: Bloom/Glow is now applied to the pixelated color *before* other effects.
    // We check the brightness of the pixel and if it's bright enough, we add a glow.
    float brightness = color.r; // Since it's black and white, all channels are the same.
    float glow = max(0.0, brightness - 0.5) * bloom_strength;
    
    // We apply the glow to the sampled color itself.
    color.rgb += vec3(glow);

    // 4. Apply high-resolution scanlines with controllable thickness.
    if (mod(FRAGCOORD.y, scanline_spacing) < scanline_thickness) {
        color.rgb = mix(color.rgb, vec3(0.0), scanline_strength);
    }

    // 5. Add subtle noise/static.
    float noise = random(FRAGCOORD.xy) * noise_strength;
    color.rgb += noise;

    // 6. Add a vignette to darken the edges.
    float vignette = smoothstep(0.4, 1.0, dist) * vignette_strength;
    color.rgb *= 1.0 - vignette;

    COLOR = color;
}
