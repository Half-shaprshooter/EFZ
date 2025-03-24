extends Panel

const ItemDB = preload("res://Scenes/Items/ItemDB.tscn")
@onready var slots = get_children()
var items = {}

func _ready():
	for slot in slots:
		items[slot.name] = null

func insert_item(item):
	# Определяем позицию предмета, используя глобальные координаты
	var item_pos = item.global_position + item.size / 2
	var slot = get_slot_under_pos(item_pos)
	if slot == null:
		return false

	# Проверяем слот предмета по данным в ItemDB
	var item_slot = ItemDB.get_item(item.get_meta("id"))["slot"]
	if item_slot != slot.name:
		return false
	if items[item_slot] != null:
		return false

	# Вставляем предмет и позиционируем его в центре слота
	items[item_slot] = item
	item.global_position = slot.global_position + slot.size / 2 - item.size / 2
	return true

func grab_item(pos):
	var item = get_item_under_pos(pos)
	if item == null:
		return null

	# Получаем слот, к которому относится предмет, и освобождаем его
	var item_slot = ItemDB.get_item(item.get_meta("id"))["slot"]
	items[item_slot] = null
	return item

func get_slot_under_pos(pos: Vector2):
	return get_thing_under_pos(slots, pos)

func get_item_under_pos(pos: Vector2):
	return get_thing_under_pos(items.values(), pos)

# Универсальный метод поиска объекта под позицией
func get_thing_under_pos(arr, pos: Vector2):
	for thing in arr:
		if thing != null and thing.get_global_transform().get_rect().has_point(pos):
			return thing
	return null
