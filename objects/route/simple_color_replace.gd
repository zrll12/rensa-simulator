extends Node2D

@export
var route_color = Color(1, 0, 0)

var sprite_material = ShaderMaterial.new()

func _ready() -> void:
	sprite_material.shader = preload("res://shader/color_replace.gdshader")
	sprite_material.set_shader_parameter("from", Color("#1c499f"))
	sprite_material.set_shader_parameter("to", route_color)
	
	$Sprite2D.material = sprite_material
