shader_type canvas_item;

// This uniform sets the virtual low resolution to simulate. For a 1280x720 screen, 160x90 is a good ratio.
uniform vec2 virtual_resolution = vec2(160.0, 90.0);

// The spacing of the high-resolution scanlines. A value of 2.0 means a line every 2 pixels.
uniform float scanline_spacing = 2.0;

// The thickness of the scanline. A value of 1.0 makes it a single pixel thick.
uniform float scanline_thickness = 1.0;

// The darkness of the scanlines. I've lowered this to make them more subtle.
uniform float scanline_strength = 0.3;

// The amount of barrel distortion for a lens-like curvature.
uniform float barrel_distortion = 0.15;

// The amount of static/noise to add to the screen. A small value like 0.05 is usually enough.
uniform float noise_strength = 0.01;

// The strength of the vignette (darkened edges). A value between 0.5 and 1.5 is a good range.
uniform float vignette_strength = 0.8;

// A new uniform to control how bright a pixel needs to be to start glowing. A lower value means more pixels glow.
uniform float bloom_threshold = 0.1;

// The strength of the glow/bloom effect on bright pixels. Now much more powerful.
uniform float bloom_strength = 100.0;

// The screen texture, now using the modern Godot hint for post-processing.
uniform sampler2D screen_texture : hint_screen_texture;

// A simple random function to generate noise.
float random(vec2 uv) {
    return fract(sin(dot(uv.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    // 1. Apply Barrel Distortion with subtle wobble for a lens-like effect.
    vec2 distorted_uv = UV;
    vec2 center = vec2(0.5, 0.5);
    vec2 offset = distorted_uv - center;
    float dist = dot(offset, offset);
    // The sin(TIME) adds a subtle, gentle wobble over time.
    distorted_uv += offset * dist * (barrel_distortion + sin(TIME) * 0.01);

    // 2. Pixelate the distorted UV coordinates.
    vec2 screen_size = 1.0 / SCREEN_PIXEL_SIZE;
    vec2 pixel_size = screen_size / virtual_resolution;
    vec2 uv_snapped = floor(distorted_uv * virtual_resolution) / virtual_resolution;

    // 3. Sample the colors.
    // High-resolution color for the bloom effect using the new uniform.
    vec4 high_rez_color = texture(screen_texture, SCREEN_UV);

    // 4. Bloom/Glow effect applied to the high-res texture before any pixelation.
    vec3 bloom_color = max(vec3(0.0), high_rez_color.rgb - vec3(bloom_threshold)) * bloom_strength;

    // 5. Sample the pixelated color and add the bloom to it.
    vec4 low_rez_color = texture(TEXTURE, uv_snapped) + vec4(bloom_color, 0.0);

    // 6. Apply high-resolution scanlines with controllable thickness.
    if (mod(FRAGCOORD.y, scanline_spacing) < scanline_thickness) {
        // The key change is here: We make the scanline strength dependent on the pixel's brightness.
        // `1.0 - low_rez_color.r` will be close to 1.0 for dark pixels and 0.0 for bright pixels.
        float dynamic_scanline_strength = scanline_strength * (1.0 - low_rez_color.r);
        low_rez_color.rgb = mix(low_rez_color.rgb, vec3(0.0), dynamic_scanline_strength);
    }

    // 7. Add subtle noise/static.
    float noise = random(FRAGCOORD.xy) * noise_strength;
    low_rez_color.rgb += noise;

    // 8. Add a vignette to darken the edges.
    float vignette = smoothstep(0.4, 1.0, dist) * vignette_strength;
    low_rez_color.rgb *= 1.0 - vignette;

    COLOR = low_rez_color;
}
