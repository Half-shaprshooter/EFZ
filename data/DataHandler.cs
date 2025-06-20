using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DataHandler : Node
{
	public static Dictionary<string, Dictionary<string, JsonElement>> itemData = new();
	public static Dictionary<string, List<List<string>>> itemGridData = new();
	
	public string itemDataPath = "res://data/Item_data.json";
	

	public override void _Ready()
	{
		LoadData(itemDataPath);
		SetGridData();
	}
	
	/// <summary>
	/// Загружает и парсит JSON-файл с данными о предметах
	/// </summary>
	/// <param name="path">Путь к JSON-файлу</param>
	private void LoadData(string path)
	{
		if (!Godot.FileAccess.FileExists(path))
		{
			GD.Print("Item data file not found");
			return;
		}

		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		var content = file.GetAsText();

		try
		{
			itemData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(content);
			GD.Print("Loaded Data:\n" + JsonSerializer.Serialize(itemData, new JsonSerializerOptions { WriteIndented = true }));
		}
		catch (Exception e)
		{
			GD.PrintErr("JSON Parse Error: " + e.Message);
		}
	}
	
	/// <summary>
	/// Обрабатывает данные о сетке предметов для использования в инвентаре.
	/// </summary>
	private void SetGridData()
	{
		foreach (var item in itemData)
		{
			List<List<string>> tempGridArray = new();

			if (item.Value.TryGetValue("Grid", out JsonElement gridElement) && gridElement.ValueKind == JsonValueKind.String)
			{
				var gridString = gridElement.GetString();
				string[] gridRows = gridString.Split('/');

				foreach (var row in gridRows)
				{
					tempGridArray.Add(new List<string>(row.Split(',')));
				}
			}

			itemGridData[item.Key] = tempGridArray;
		}

		GD.Print("Grid Data:\n" + JsonSerializer.Serialize(itemGridData, new JsonSerializerOptions { WriteIndented = true }));
	}
}
