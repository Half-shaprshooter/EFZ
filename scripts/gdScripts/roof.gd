extends Area2D

@onready var roof := $RoofSpriteGroup
@onready var lights := $LightsGroup


func _on_area_2d_body_entered(body) -> void:
		lights.show();
		roof.hide();

func _on_area_2d_body_exited(body) -> void:
		lights.hide();
		roof.show()
