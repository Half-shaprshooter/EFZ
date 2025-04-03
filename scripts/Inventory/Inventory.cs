using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{
	// Ссылки на ноды сцены
	private Node2D _mainNode;
	private Node2D _player;
	private PackedScene _slotScene;
	private GridContainer _gridContainer;
	private PackedScene _itemScene;
	private ScrollContainer _scrollContainer;
	private int _colCount;
	private Control _uiInventory;
	private ColorRect _colorRect1;
	private ColorRect _colorRect2;
	private PackedScene _pickingSlotScene;
	private Control _pickingSlotWeaponFirst;
	private CanvasLayer _ui;
	private PackedScene _floorItemScene;

	// Состояние инвентаря
	private List<Slot> _gridArray = new List<Slot>();
	private Item _itemHeld = null;
	private Slot _currentSlot = null;
	private bool _canPlace = false;
	private Vector2 _iconAnchor;
	private bool _fastPlace = false;

	public override void _Ready()
	{
		// Получаем ссылки на ноды
		_mainNode = GetNode<Node2D>("../../..");
		_player = GetNode<Node2D>("../..");
		_slotScene = GD.Load<PackedScene>("res://Scenes/Inventory/slot.tscn");
		_gridContainer = GetNode<GridContainer>("ColorRect/MarginContainer/VBoxContainer/ScrollContainer/GridContainer");
		_itemScene = GD.Load<PackedScene>("res://Scenes/Items/item.tscn");
		_scrollContainer = GetNode<ScrollContainer>("ColorRect/MarginContainer/VBoxContainer/ScrollContainer");
		_colCount = _gridContainer.Columns;
		_uiInventory = this;
		_colorRect1 = GetNode<ColorRect>("ColorRect");
		_colorRect2 = GetNode<ColorRect>("ColorRect2");
		_pickingSlotScene = GD.Load<PackedScene>("res://Scenes/Inventory/pickingSlot.tscn");
		_pickingSlotWeaponFirst = GetNode<Control>("ColorRect2/WeaponFirst");
		_ui = GetNode<CanvasLayer>("..");
		_floorItemScene = GD.Load<PackedScene>("res://Scenes/Items/floorItem.tscn");

		_uiInventory.Visible = false;
		
		// Создаем начальные слоты
		for (int i = 0; i < 64; i++)
		{
			CreateSlot();
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Inventory"))
		{
			_uiInventory.Visible = !_uiInventory.Visible;
		}

		if (_itemHeld != null)
		{
			if (Input.IsActionJustPressed("InventoryRotate"))
			{
				RotateItem();
			}

			if (Input.IsActionJustPressed("InventoryClick"))
			{
				if (_scrollContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					PlaceItem();
				}
				else if (!_colorRect1.GetGlobalRect().HasPoint(GetGlobalMousePosition()) && 
						 !_colorRect2.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					DeleteHeldItem();
				}
				else if (_pickingSlotWeaponFirst.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					
				}
			}

			if (Input.IsActionJustPressed("InventoryRightClick"))
			{
				DeleteHeldItem();
				_uiInventory.Visible = false;
			}

			if (Input.IsActionJustPressed("InventoryDelete"))
			{
				DeleteHeldItem();
			}
		}
		else
		{
			if (Input.IsActionJustPressed("InventoryClick"))
			{
				if (_scrollContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					PickItem();
				}
			}
		}
	}

	/// <summary>
	/// Создает новый слот инвентаря
	/// </summary>
	private void CreateSlot()
	{
		Slot newSlot = (Slot)_slotScene.Instantiate();
		newSlot.SlotID = _gridArray.Count;
		_gridArray.Add(newSlot);
		_gridContainer.AddChild(newSlot);
		newSlot.SlotEntered += OnSlotMouseEntered;
		newSlot.SlotExited += OnSlotMouseExited;
	}

	/// <summary>
	/// Обработчик входа курсора в слот
	/// </summary>
	private void OnSlotMouseEntered(Slot slot)
	{
		_iconAnchor = new Vector2(10000, 10000);
		_currentSlot = slot;
		
		if (_itemHeld != null)
		{
			CheckSlotAvailability(_currentSlot);
			CallDeferred(nameof(SetGrids), _currentSlot);
		}
	}

	/// <summary>
	/// Обработчик выхода курсора из слота
	/// </summary>
	private void OnSlotMouseExited(Slot slot)
	{
		ClearGrid();
	}

	/// <summary>
	/// Размещает предмет в ячейках инвентаря
	/// </summary>
	private bool PlaceItemInCells(Item item, int[] itemSize, int startSlotId)
	{
		// Очищаем предыдущие позиции если предмет уже в инвентаре
		if (item.GetParent() == _gridContainer)
		{
			foreach (Vector2I grid in item.ItemGrids)
			{
				int gridToClear = ((Slot)item.GridAnchor).SlotID + grid.X - grid.Y * _colCount;
				if (gridToClear >= 0 && gridToClear < _gridArray.Count)
				{
					_gridArray[gridToClear].State = Slot.States.Free;
					_gridArray[gridToClear].ItemStored = null;
				}
			}
		}

		// Проверяем можно ли разместить предмет
		bool canPlaceHere = true;
		for (int x = 0; x < itemSize[0]; x++)
		{
			for (int y = 0; y < itemSize[1]; y++)
			{
				int cellToCheck = startSlotId + x - y * _colCount;
				if ((startSlotId % _colCount) + x >= _colCount || 
					((int)(startSlotId / _colCount) + y) >= _colCount || 
					cellToCheck < 0 || 
					cellToCheck >= _gridArray.Count)
				{
					canPlaceHere = false;
					break;
				}

				if (_gridArray[cellToCheck].State == Slot.States.Taken && 
					_gridArray[cellToCheck].ItemStored != item)
				{
					canPlaceHere = false;
					break;
				}
			}
			if (!canPlaceHere) break;
		}

		if (!canPlaceHere) return false;

		// Размещаем предмет
		item.ItemGrids.Clear();
		for (int x = 0; x < itemSize[0]; x++)
		{
			for (int y = 0; y < itemSize[1]; y++)
			{
				int cellToCheck = startSlotId + x - y * _colCount;
				_gridArray[cellToCheck].State = Slot.States.Taken;
				_gridArray[cellToCheck].ItemStored = item;
				item.ItemGrids.Add(new Vector2I(x, -y));
			}
		}

		// Перемещаем предмет в инвентарь если нужно
		if (item.GetParent() != _gridContainer)
		{
			item.GetParent().RemoveChild(item);
			_gridContainer.AddChild(item);
		}

		item.SnapTo(_gridArray[startSlotId].GlobalPosition);
		item.GridAnchor = _gridArray[startSlotId];

		return true;
	}

	/// <summary>
	/// Добавляет предмет в инвентарь
	/// </summary>
	public void AddItem(int itemId)
	{
		itemId = (int)itemId;
		_fastPlace = true;
		Item item = (Item)_itemScene.Instantiate();
		GD.Print("Picked up ", item.GetItemName(itemId));
		
		_uiInventory.Visible = true;
		ManualItemPlace(itemId);
	}
	
	private void OnButtonSpawnPressed()
	{
		Item item = (Item)_itemScene.Instantiate();
		AddChild(item);
		item.LoadItem(8);
	
		int[] itemSize = {1, 1}; // Ширина, высота
		int colCount = 1;
	
		for (int startSlotId = 0; startSlotId < _gridArray.Count; startSlotId += colCount)
		{
			if (PlaceItemInCells(item, itemSize, startSlotId))
			{
				GD.Print("Предмет успешно размещён в слоте ", startSlotId);
				return;
			}
		}
	
		GD.Print("Предмет не удалось разместить!");
		item.QueueFree();
	}

	/// <summary>
	/// Размещает предмет вручную (для взаимодействия игрока)
	/// </summary>
	private void ManualItemPlace(int itemId)
	{
		Item newItem = (Item)_itemScene.Instantiate();
		AddChild(newItem);
		newItem.LoadItem(itemId);
		newItem.Selected = true;
		_itemHeld = newItem;
	}

	/// <summary>
	/// Проверяет доступность слота для размещения
	/// </summary>
	private void CheckSlotAvailability(Slot slot)
	{
		foreach (Vector2I grid in _itemHeld.ItemGrids)
		{
			int gridToCheck = slot.SlotID + grid.X + grid.Y * _colCount;
			int lineSwitchCheck = slot.SlotID % _colCount + grid.X;
			
			if (lineSwitchCheck < 0 || lineSwitchCheck >= _colCount)
			{
				_canPlace = false;
				return;
			}
			
			if (gridToCheck < 0 || gridToCheck >= _gridArray.Count)
			{
				_canPlace = false;
				return;
			}
			
			if (_gridArray[gridToCheck].State == Slot.States.Taken)
			{
				_canPlace = false;
				return;
			}
		}
		
		_canPlace = true;
	}

	/// <summary>
	/// Устанавливает цвета сетки в зависимости от доступности
	/// </summary>
	private void SetGrids(Slot slot)
	{
		foreach (Vector2I grid in _itemHeld.ItemGrids)
		{
			int gridToCheck = slot.SlotID + grid.X + grid.Y * _colCount;
			if (gridToCheck < 0 || gridToCheck >= _gridArray.Count) continue;

			int lineSwitchCheck = slot.SlotID % _colCount + grid.X;
			if (lineSwitchCheck < 0 || lineSwitchCheck >= _colCount) continue;
			
			if (_canPlace)
			{
				_gridArray[gridToCheck].SetColor(Slot.States.Free);
				if (grid.Y < _iconAnchor.X) _iconAnchor.X = grid.Y;
				if (grid.X < _iconAnchor.Y) _iconAnchor.Y = grid.X;
			}
			else
			{
				_gridArray[gridToCheck].SetColor(Slot.States.Taken);
			}
		}
	}

	/// <summary>
	/// Очищает подсветку сетки
	/// </summary>
	private void ClearGrid()
	{
		foreach (Slot grid in _gridArray)
		{
			grid.SetColor(Slot.States.Default);
		}
	}

	/// <summary>
	///  Поворачивает удерживаемый предмет
	/// </summary>
	private void RotateItem()
	{
		_itemHeld.RotateItem();
		ClearGrid();
		if (_currentSlot != null)
		{
			OnSlotMouseEntered(_currentSlot);
		}
	}

	/// <summary>
	/// Размещает предмет в инвентаре
	/// </summary>
	private async void PlaceItem()
	{
		if (!_canPlace || _currentSlot == null) return;
		
		_itemHeld.GetParent().RemoveChild(_itemHeld);
		_gridContainer.AddChild(_itemHeld);
		_itemHeld.GlobalPosition = GetGlobalMousePosition();

		int calculatedGridId = _currentSlot.SlotID + (int)_iconAnchor.X * _colCount + (int)_iconAnchor.Y;
		_itemHeld.SnapTo(_gridArray[calculatedGridId].GlobalPosition);
		GD.Print(calculatedGridId);
		
		_itemHeld.GridAnchor = _currentSlot;
		foreach (Vector2I grid in _itemHeld.ItemGrids)
		{
			int gridToCheck = _currentSlot.SlotID + grid.X + grid.Y * _colCount;
			_gridArray[gridToCheck].State = Slot.States.Taken;
			_gridArray[gridToCheck].ItemStored = _itemHeld;
		}
		
		_itemHeld = null;
		ClearGrid();
		
		if (_fastPlace)
		{
			await ToSignal(GetTree().CreateTimer(0.2), "timeout");
			_uiInventory.Visible = false;
			_fastPlace = false;
		}
	}

	/// <summary>
	/// Подбирает предмет из инвентаря
	/// </summary>
	private void PickItem()
	{
		if (_currentSlot == null || _currentSlot.ItemStored == null) return;
		
		_itemHeld = (Item)_currentSlot.ItemStored;
		_itemHeld.Selected = true;
		
		_itemHeld.GetParent().RemoveChild(_itemHeld);
		AddChild(_itemHeld);
		_itemHeld.GlobalPosition = GetGlobalMousePosition();
		
		foreach (Vector2I grid in _itemHeld.ItemGrids)
		{
			int gridToCheck = ((Slot)_itemHeld.GridAnchor).SlotID + grid.X + grid.Y * _colCount;
			_gridArray[gridToCheck].State = Slot.States.Free;
			_gridArray[gridToCheck].ItemStored = null;
		}
		
		CheckSlotAvailability(_currentSlot);
		CallDeferred(nameof(SetGrids), _currentSlot);
	}

	/// <summary>
	/// Удаляет удерживаемый предмет
	/// </summary>
	private void DeleteHeldItem()
	{
		_itemHeld.QueueFree();
		_itemHeld = null;
		ClearGrid();
		_canPlace = false;
	}
}
