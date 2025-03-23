@tool
extends Node2D

var player_in_area = false

@export var item_id: int = 10 
@export var item_name: String = "AK47" 
@export var item_texture: Texture = preload("res://sprites/AK47.png")

var scene_path: String = "res://Entities/floorItem.tscn"

@onready var inventoryUI = $"../Player/UI/Inventory"
@onready var icon_sprite = $Sprite2D
@onready var interact_ui = $interactItem

func _ready() -> void:
	interact_ui.visible = false
	if not Engine.is_editor_hint():
		icon_sprite.texture = item_texture

func set_item_id(new_item_id: int):
	item_id = new_item_id
	
func set_item_texture(texture: Texture):
	item_texture = texture

func _on_body_entered(body: Node2D) -> void:
	if body.is_in_group("Player"):
		interact_ui.visible = true
		player_in_area = true


func _on_body_exited(body: Node2D) -> void:
	if body.is_in_group("Player"):
		interact_ui.visible = false
		player_in_area = false
		
		
func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		icon_sprite.texture = item_texture
	
	if player_in_area and Input.is_action_just_pressed("interact"):
		inventoryUI.add_item(item_id)
		interact_ui.visible = true
		queue_free()
		print("Произошло действие")
