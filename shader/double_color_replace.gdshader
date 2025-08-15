shader_type canvas_item;

// 要替换的颜色
uniform vec3 from1 : source_color = vec3(0.0);
// 替换成这个颜色
uniform vec3 to1 : source_color = vec3(0.0);
uniform vec3 from2 : source_color = vec3(0.0);
uniform vec3 to2 : source_color = vec3(0.0);

// 替换颜色阈值，阈值越高替换的颜色值范围越广
uniform float threshold : hint_range(0.0, 1.0, 0.01) = 0.1;


void fragment() {
	vec3 d = abs(COLOR.rgb - from1);
	float m = (d.r + d.g + d.b) / 3.0;
	if(m < threshold){
		COLOR.rgb = to1;
	} else {
		vec3 d2 = abs(COLOR.rgb - from2);
		float m2 = (d2.r + d2.g + d2.b) / 3.0;
		if (m2 < threshold) {
			COLOR.rgb = to2;
		}
	}
}
