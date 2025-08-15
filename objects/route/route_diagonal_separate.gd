extends Node2D

@export
var route1_color = Color(1, 0, 0)
@export
var route2_color = Color(1, 0, 0)

var sprite_material = ShaderMaterial.new()

func _ready() -> void:
	sprite_material.shader = preload("res://shader/double_color_replace.gdshader")
	sprite_material.set_shader_parameter("from1", Color("#1c499f"))
	sprite_material.set_shader_parameter("to1", route1_color)
	sprite_material.set_shader_parameter("from2", Color("#03ff00"))
	sprite_material.set_shader_parameter("to2", route2_color)
	
	$Sprite2D.material = sprite_material
