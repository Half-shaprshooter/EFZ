using Godot;
using System;
using System.Collections.Generic;
using EscapeFromZone.scripts.Inventory;
using EscapeFromZone.scripts.Items;

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
	
	private EquipmentSlot _currentEquipmentSlot = null;
	private List<EquipmentSlot> _equipmentSlots = new List<EquipmentSlot>();
	private PackedScene _equipmentSlotScene;
	
	private CanvasLayer _ui;
	private PackedScene _floorItemScene;

	// Состояние инвентаря
	private List<Slot> _gridArray = new List<Slot>();
	private Item _itemHeld = null;
	private Slot _currentSlot = null;
	private bool _canPlace = false;
	private Vector2 _iconAnchor;
	
	
	private ItemUseHandler _itemUseHandler;

	[Export] public bool IsPlayerInventory;

	private TradeManager _tradeManager;
	public bool IsTradeMode { get; set; }

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
		_ui = GetNode<CanvasLayer>("..");
		_floorItemScene = GD.Load<PackedScene>("res://Scenes/Items/floorItem.tscn");
		_itemUseHandler = new ItemUseHandler();
		_tradeManager = GetNode<TradeManager>("/root/TradeManager");

		if (IsPlayerInventory)
		{
			_tradeManager.GetPlayerInventory(this);
		}

		_uiInventory.Visible = false;
		

		InitializeEquipmentSlots();
		
		// Создаем начальные слоты
		for (int i = 0; i < 64; i++)
		{
			CreateSlot();
		}
	}
	
	private void InitializeEquipmentSlots()
	{
		// Находим все EquipmentSlot в ColorRect2
		foreach (var child in _colorRect2.GetChildren())
		{
			if (child is EquipmentSlot equipmentSlot)
			{
				_equipmentSlots.Add(equipmentSlot);
				equipmentSlot.SlotEntered += OnEquipmentSlotMouseEntered;
				equipmentSlot.SlotExited += OnEquipmentSlotMouseExited;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Inventory") && !IsTradeMode && IsPlayerInventory)
		{
			_uiInventory.Visible = !_uiInventory.Visible;
			PlayerData.CanFire = !PlayerData.CanFire;
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
				//else if (!_colorRect1.GetGlobalRect().HasPoint(GetGlobalMousePosition()) && 
				//		 !_colorRect2.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				//{
				//	DeleteHeldItem();
				//}
				else
				{
					foreach (var equipmentSlot in _equipmentSlots)
					{
						if (equipmentSlot.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
							PlaceItem();
					}
				}
				
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
				else
				{
					foreach (var equipmentSlot in _equipmentSlots)
					{
						if (equipmentSlot.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
							PickItem();
					}
				}
			}
			if (Input.IsActionJustPressed("InventoryRightClick"))
			{
				if (_scrollContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
				{
					UseItem();
				}
			}
		}
	}
	
	// Получение данных всего инвентаря
	public InventoryData GetInventoryData()
	{
		var data = new InventoryData();

		// Сохраняем обычные слоты
		foreach (var slot in _gridArray)
		{
			data.Slots.Add(new SlotData
			{
				SlotId = slot.SlotID,
				State = slot.State,
				ItemData = slot.ItemStored != null ? new ItemData((Item)slot.ItemStored) : null
			});
		}

		// Сохраняем слоты экипировки
		foreach (var equipmentSlot in _equipmentSlots)
		{
			data.EquipmentSlots.Add(new EquipmentSlotData
			{
				SlotId = equipmentSlot.SlotID,
				State = equipmentSlot.State,
				ItemData = equipmentSlot.ItemStored != null ? new ItemData((Item)equipmentSlot.ItemStored) : null,
				SlotCategory =  equipmentSlot.SlotCategory
			});
		}

		return data;
	}

	private ItemData GetItemData(Item slotItemStored)
	{
		throw new NotImplementedException();
	}

	private void UseItem()
	{
		if (_currentSlot != null && _currentSlot.ItemStored != null)
		{
			var item = (Item)_currentSlot.ItemStored;
			if (_itemUseHandler.UseItem(item))
			{
				foreach (Vector2I grid in item.ItemGrids)
				{
					var gridToCheck = _currentSlot.SlotID + grid.X + grid.Y * _colCount;
					if (gridToCheck >= 0 && gridToCheck < _gridArray.Count)
					{
						_gridArray[gridToCheck].State = Slot.States.Free;
						_gridArray[gridToCheck].ItemStored = null;
					}
				}
				item.QueueFree();
				ClearGrid();
			}
		}
	}

	public void UseItemFromSlot(int slotId)
	{
		if (slotId >= 0 && slotId < _gridArray.Count)
		{
			_currentSlot = _gridArray[slotId];
			UseItem();
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
	
	// Обработчик входа курсора в слот экипировки
	private void OnEquipmentSlotMouseEntered(EquipmentSlot slot)
	{
		_currentEquipmentSlot = slot;
		if (_itemHeld != null && slot.CanAcceptItem(_itemHeld))
		{
			slot.SetColor(EquipmentSlot.States.Free);
			_canPlace = true;
		}
		else
		{
			slot.SetColor(EquipmentSlot.States.Taken);
			_canPlace = false;
		}
	}

	// Обработчик выхода курсора из слота экипировки
	private void OnEquipmentSlotMouseExited(EquipmentSlot slot)
	{
		_currentEquipmentSlot = null;
		slot.SetColor(EquipmentSlot.States.Default);
		_canPlace = false;
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
		if (_itemHeld != null)
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
		if (!_canPlace) return;

		if (_currentEquipmentSlot != null && _itemHeld != null && _currentEquipmentSlot.CanAcceptItem(_itemHeld))
		{
			// Размещение в слоте экипировки
			_itemHeld.GetParent().RemoveChild(_itemHeld);
			_currentEquipmentSlot.AddChild(_itemHeld);
			_itemHeld.ResetAngle();
			_itemHeld.SnapTo(_currentEquipmentSlot.GlobalPosition);
			_currentEquipmentSlot.State = EquipmentSlot.States.Taken;
			_currentEquipmentSlot.ItemStored = _itemHeld;
			_itemHeld.GridAnchor = _currentEquipmentSlot;
			_itemHeld = null;
			_currentEquipmentSlot.SetColor(EquipmentSlot.States.Default);
		}
		else if (_currentSlot != null)
		{
			// Существующая логика для размещения в обычный слот
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
			
		}
	}

	private void DoTransfer()
	{
		if (IsPlayerInventory)
		{
			var item = _itemHeld;
			DeleteHeldItem();
			_tradeManager.TryMakeItemFromPlayerTransfer(item);
		}
		else
		{
			var item = _itemHeld;
			DeleteHeldItem();
			_tradeManager.TryMakeItemFromSellerTransfer(item);
		}
	}

	/// <summary>
	/// Подбирает предмет из инвентаря
	/// </summary>
	private void PickItem()
	{
		if (_currentSlot != null && _currentSlot.ItemStored != null)
		{
			// Существующая логика для обычного слота
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

			if (IsTradeMode)
			{
				DoTransfer();
			}
			
		}
		else if (_currentEquipmentSlot != null && _currentEquipmentSlot.ItemStored != null)
		{
			// Подбор из слота экипировки
			_itemHeld = _currentEquipmentSlot.ItemStored;
			_itemHeld.Selected = true;

			_currentEquipmentSlot.RemoveChild(_itemHeld);
			AddChild(_itemHeld);
			_itemHeld.GlobalPosition = GetGlobalMousePosition();

			_currentEquipmentSlot.State = EquipmentSlot.States.Free;
			_currentEquipmentSlot.ItemStored = null;
			_currentEquipmentSlot.SetColor(EquipmentSlot.States.Default);
		}
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
