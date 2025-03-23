extends Panel

@onready var panel = $"."
@onready var filter = $StatusFilter
var is_hovering := false
enum States {DEFAULT, TAKEN, FREE}
var state = States.DEFAULT
var item_stored = null 

func _process(delta):
	pass

func set_color(a_state = States.DEFAULT) -> void:
	match a_state:
		States.DEFAULT:
			filter.color = Color(Color.WHITE, 0.0)
		States.TAKEN:
			filter.color = Color(Color.RED, 0.2)
		States.FREE:
			filter.color = Color(Color.GREEN, 0.2)
