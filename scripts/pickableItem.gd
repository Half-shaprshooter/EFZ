extends Area2D

signal picked_up(item_name)

@export var item_name: String = "Item"  
@export var amount: int = 1          

func _ready():
	_spawn_pickup()

func _spawn_pickup():
	var pickup_scene = preload("res://scripts/pickableItem.gd")
	var pickup_instance = pickup_scene.instantiate()
	add_child(pickup_instance)
	pickup_instance.connect("picked_up", Callable(self, "_on_pickup"))

func _on_body_entered(body):
	if body.name == "Player":
		_pick_up()
 
func _pick_up():
	emit_signal("picked_up", item_name)
	queue_free()
