extends Node2D

@onready var box := $Box

func _on_area_2d_body_entered(body) -> void:
		box.show()
	
func _on_area_2d_body_exited(body) -> void:
		box.hide()
