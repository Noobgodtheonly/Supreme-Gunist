shader_type canvas_item;
uniform float flashColThresh : hint_range(0.0, 1) = 0;
uniform float opacityThresh : hint_range(0.0, 1) = 0;
void fragment() {
	vec4 col = texture(TEXTURE,UV);
	col.rgb = mix(col.rgb,vec3(1,1,1),flashColThresh);
    col.a *= opacityThresh;
	COLOR = col;
}
