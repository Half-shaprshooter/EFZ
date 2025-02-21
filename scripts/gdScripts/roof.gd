extends Area2D

@onready var roof := $RoofSpriteGroup

func _on_area_2d_body_entered(body) -> void:
		roof.hide();

func _on_area_2d_body_exited(body) -> void:
		roof.show()
