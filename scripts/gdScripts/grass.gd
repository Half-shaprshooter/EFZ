extends Area2D

signal grass_entered
signal grass_exited

func _on_body_entered(body) -> void:
	if body.has_method("Player"):
		emit_signal("grass_entered")

func _on_body_exited(body) -> void:
	if body.has_method("Player"):
		emit_signal("grass_exited")
