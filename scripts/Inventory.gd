extends Control

@onready var main_node = $"../../.."
@onready var player = $"../.."
@onready var slot_scene = preload("res://Scenes/slot.tscn")
@onready var grid_container = $ColorRect/MarginContainer/VBoxContainer/ScrollContainer/GridContainer
@onready var item_scene = preload("res://Scenes/item.tscn")
@onready var scroll_container = $ColorRect/MarginContainer/VBoxContainer/ScrollContainer
@onready var col_count = grid_container.columns 
@onready var UIinventory = $"."
@onready var colorRect1 = $ColorRect
@onready var colorRect2 = $ColorRect2
@onready var picking_slot_scene = preload("res://Scenes/pickingSlot.tscn")
@onready var picking_slot_weapon_first = $ColorRect2/WeaponFirst
@onready var ui = $".."
var floor_item_scene = preload("res://Scenes/floorItem.tscn") 
var grid_array := []
var item_held = null 
var current_slot = null
var can_place := false
var icon_anchor : Vector2
var fastPlace = false
func _ready() -> void:
	UIinventory.visible = false
	for i in range(64):
		create_slot()
		
func _process(delta):
	if Input.is_action_just_pressed("Inventory"):
		UIinventory.visible = !UIinventory.visible
	if item_held:
		
		if Input.is_action_just_pressed("InventoryRotate"):
			rotate_item()
			
		if Input.is_action_just_pressed("InventoryClick"):
			if scroll_container.get_global_rect().has_point(get_global_mouse_position()):
				place_item()
			
			elif !colorRect1.get_global_rect().has_point(get_global_mouse_position()) and !colorRect2.get_global_rect().has_point(get_global_mouse_position()):
				delete_held_item()
			
			elif picking_slot_weapon_first.get_global_rect().has_point(get_global_mouse_position()):
				pass
				
		if Input.is_action_just_pressed("InventoryRightClick"):
			delete_held_item()
			UIinventory.visible = false
				
		if Input.is_action_just_pressed("InventoryDelete"):
			delete_held_item()
	else:
		if Input.is_action_just_pressed("InventoryClick"):
			if scroll_container.get_global_rect().has_point(get_global_mouse_position()):
				pick_item()

func drop_item():
	pass

func create_slot():
	var new_slot = slot_scene.instantiate()
	new_slot.slot_ID = grid_array.size()
	grid_array.push_back(new_slot)
	grid_container.add_child(new_slot)
	new_slot.slot_entered.connect(_on_slot_mouse_entered)
	new_slot.slot_exited.connect(_on_slot_mouse_exited)
	
func _on_slot_mouse_entered(a_Slot):
	icon_anchor = Vector2(10000, 10000)
	current_slot = a_Slot
	if item_held:
		check_slot_availavbility(current_slot)
		set_grids.call_deferred(current_slot)
	
func _on_slot_mouse_exited(a_Slot):
	clear_grid()

# в процессе разработки	
func place_item_in_cells(item, item_size, start_slot_id):
	if item.get_parent() == grid_container:
		for grid in item.item_grids:
			var grid_to_clear = item.grid_anchor.slot_ID + grid[0] - grid[1] * col_count 
			if grid_to_clear >= 0 and grid_to_clear < grid_array.size():
				grid_array[grid_to_clear].state = grid_array[grid_to_clear].States.FREE
				grid_array[grid_to_clear].item_stored = null
	var can_place_here = true
	for x in range(item_size[0]):
		for y in range(item_size[1]):
			var cell_to_check = start_slot_id + x - y * col_count
			if (start_slot_id % col_count) + x >= col_count or (int(start_slot_id / col_count) + y) >= col_count or cell_to_check < 0 or cell_to_check >= grid_array.size():
				can_place_here = false
				break
			if grid_array[cell_to_check].state == grid_array[cell_to_check].States.TAKEN and grid_array[cell_to_check].item_stored != item:
				can_place_here = false
				break
		if not can_place_here:
			break
	if not can_place_here:
		return false
	item.item_grids = []
	for x in range(item_size[0]):
		for y in range(item_size[1]):
			var cell_to_check = start_slot_id + x - y * col_count
			grid_array[cell_to_check].state = grid_array[cell_to_check].States.TAKEN
			grid_array[cell_to_check].item_stored = item
			item.item_grids.append(Vector2(x, -y))

	if item.get_parent() != grid_container:
		item.get_parent().remove_child(item)
		grid_container.add_child(item)
	item._snap_to(grid_array[start_slot_id].global_position)
	item.grid_anchor = grid_array[start_slot_id]

	return true

func add_item(item_id) -> void:
	item_id = int(item_id)
	fastPlace = true
	var item = item_scene.instantiate()
	print("Взят ", item.get_item_name(item_id))
	#add_child(item)
	#item.load_item(int(item_id))  
	 # Ширина, высота
	var col_count = 1
	for start_slot_id in range(0, grid_array.size(), col_count):
		# if place_item_in_cells(item, item_size, start_slot_id):
			# print("Предмет успешно размещён в слоте ", start_slot_id)
			# return
		UIinventory.visible = true
		manual_item_place(item_id)
		return
	item.queue_free()

func _on_button_spawn_pressed() -> void:
	var item = item_scene.instantiate()
	add_child(item)
	item.load_item(8)  
	var item_size = [1, 1] # Ширина, высота
	var col_count = 1
	for start_slot_id in range(0, grid_array.size(), col_count):
		if place_item_in_cells(item, item_size, start_slot_id):
			print("Предмет успешно размещён в слоте ", start_slot_id)
			return
	print("Предмет не удалось разместить!")
	item.queue_free()
	
func manual_item_place(item_id):
	var new_item = item_scene.instantiate()
	add_child(new_item)
	new_item.load_item(item_id)
	new_item.selected = true
	item_held = new_item
	
func check_slot_availavbility(a_Slot) -> void: 
	for grid in item_held.item_grids:
		var grid_to_check = a_Slot.slot_ID + grid[0] + grid[1] * col_count
		var line_switch_check = a_Slot.slot_ID % col_count + grid[0]
		if line_switch_check < 0 or line_switch_check >= col_count:
			can_place = false
			return
		if grid_to_check < 0 or grid_to_check >= grid_array.size():
			can_place = false
			return
		if grid_array[grid_to_check].state == grid_array[grid_to_check].States.TAKEN:
			can_place = false
			return
		
	can_place = true

func set_grids(a_Slot):
	for grid in item_held.item_grids:
		var grid_to_check = a_Slot.slot_ID + grid[0] + grid[1] * col_count
		if grid_to_check < 0 or grid_to_check >= grid_array.size():
			continue

		var line_switch_check = a_Slot.slot_ID % col_count + grid[0]
		if line_switch_check <0 or line_switch_check >= col_count:
			continue
		
		if can_place:
			grid_array[grid_to_check].set_color(grid_array[grid_to_check].States.FREE)
			if grid[1] < icon_anchor.x: icon_anchor.x = grid[1]
			if grid[0] < icon_anchor.y: icon_anchor.y = grid[0]
				
		else:
			grid_array[grid_to_check].set_color(grid_array[grid_to_check].States.TAKEN)
			
func clear_grid():
	for grid in grid_array:
		grid.set_color(grid.States.DEFAULT)
		
func rotate_item():
	item_held.rotate_item()
	clear_grid()
	if current_slot:
		_on_slot_mouse_entered(current_slot)


func place_item_pickingSlot():
	pass
	

func place_item():
	if not can_place or not current_slot: 
		return
		
	item_held.get_parent().remove_child(item_held)
	grid_container.add_child(item_held)
	item_held.global_position = get_global_mouse_position()

	var calculated_grid_id = current_slot.slot_ID + icon_anchor.x * col_count + icon_anchor.y
	item_held._snap_to(grid_array[calculated_grid_id].global_position)
	print(calculated_grid_id)
	
	item_held.grid_anchor = current_slot
	for grid in item_held.item_grids:
		var grid_to_check = current_slot.slot_ID + grid[0] + grid[1] * col_count
		grid_array[grid_to_check].state = grid_array[grid_to_check].States.TAKEN 
		grid_array[grid_to_check].item_stored = item_held
	
	item_held = null
	clear_grid()
	if fastPlace:
		await get_tree().create_timer(0.2).timeout
		UIinventory.visible = false
		fastPlace = false
	
func pick_item():
	if not current_slot or not current_slot.item_stored: 
		return
	item_held = current_slot.item_stored
	item_held.selected = true
	
	item_held.get_parent().remove_child(item_held)
	add_child(item_held)
	item_held.global_position = get_global_mouse_position()
	
	for grid in item_held.item_grids:
		var grid_to_check = item_held.grid_anchor.slot_ID + grid[0] + grid[1] * col_count
		grid_array[grid_to_check].state = grid_array[grid_to_check].States.FREE 
		grid_array[grid_to_check].item_stored = null
	
	check_slot_availavbility(current_slot)
	set_grids.call_deferred(current_slot)

func delete_held_item():
	item_held.queue_free()
	item_held = null 
	clear_grid() 
	can_place = false 
