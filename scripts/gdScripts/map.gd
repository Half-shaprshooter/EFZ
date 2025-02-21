extends CanvasLayer

@export var player : CharacterBody2D

@onready var sub_viewport = $SubViewportContainer/SubViewport

var miniMapPlayer

func _ready() -> void:
	miniMapPlayer = player.duplicate()
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
