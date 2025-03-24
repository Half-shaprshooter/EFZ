extends TextureRect

var items = []
var grid = {}
var cell_size = 32
var grid_width = 0
var grid_height = 0

func _ready():
	var s = get_grid_size(self)
	grid_width = s.x
	grid_height = s.y
	
	for x in range(grid_width):
		grid[x] = {}
		for y in range(grid_height):
			grid[x][y] = false

func insert_item(item):
	var item_pos = item.global_position + Vector2(cell_size / 2, cell_size / 2)
	var g_pos = pos_to_grid_coord(item_pos)
	var item_size = get_grid_size(item)
	if is_grid_space_available(g_pos.x, g_pos.y, item_size.x, item_size.y):
		set_grid_space(g_pos.x, g_pos.y, item_size.x, item_size.y, true)
		item.global_position = global_position + Vector2(g_pos.x, g_pos.y) * cell_size
		items.append(item)
		return true
	else:
		return false

func grab_item(pos: Vector2):
	var item = get_item_under_pos(pos)
	if item == null:
		return null

	var item_pos = item.global_position + Vector2(cell_size / 2, cell_size / 2)
	var g_pos = pos_to_grid_coord(item_pos)
	var item_size = get_grid_size(item)
	set_grid_space(g_pos.x, g_pos.y, item_size.x, item_size.y, false)

	items.erase(item)
	return item

func pos_to_grid_coord(pos: Vector2) -> Vector2i:
	var local_pos = pos - global_position
	return Vector2i(int(local_pos.x / cell_size), int(local_pos.y / cell_size))

func get_grid_size(item) -> Vector2i:
	var s = item.size
	return Vector2i(clamp(int(s.x / cell_size), 1, 500), clamp(int(s.y / cell_size), 1, 500))

func is_grid_space_available(x: int, y: int, w: int, h: int) -> bool:
	if x < 0 or y < 0 or x + w > grid_width or y + h > grid_height:
		return false
	for i in range(x, x + w):
		for j in range(y, y + h):
			if grid[i][j]:
				return false
	return true

func set_grid_space(x: int, y: int, w: int, h: int, state: bool):
	for i in range(x, x + w):
		for j in range(y, y + h):
			grid[i][j] = state

func get_item_under_pos(pos: Vector2):
	for item in items:
		if item.get_global_transform().get_rect().has_point(pos):
			return item
	return null

func insert_item_at_first_available_spot(item):
	for y in range(grid_height):
		for x in range(grid_width):
			if !grid[x][y]:
				item.global_position = global_position + Vector2(x, y) * cell_size
				if insert_item(item):
					return true
	return false
