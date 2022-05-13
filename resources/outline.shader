shader_type spatial;
render_mode cull_front;

uniform bool enabled = true;
uniform vec4 color : hint_color;
uniform float thickness : hint_range(1.0, 5.0, 0.05) = 1.15;

void vertex(){
	if(!enabled)
		return;
		
	VERTEX *= vec3(thickness);
}

void fragment(){
	if(!enabled)
		return;
	
	ALBEDO = color.rgb;
	EMISSION = color.rgb;
}