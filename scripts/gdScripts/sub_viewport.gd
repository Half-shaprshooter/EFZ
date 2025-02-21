extends SubViewport

@onready var camera = $Camera2D

func _physics_process(_delta: float) -> void:
	camera.position = owner.find_child("CharacterBody2D").position 
