using System;
using System.IO;
using ImGuiNET;
using UImGui;
using UnityEngine;
using UnityEngine.InputSystem;

public class SchemaUI : MonoBehaviour
{
	private void Start()
	{
		UImGuiUtility.Layout += this.DrawSchemaUI;
	}

	private void Update()
	{
		if (Keyboard.current[Key.F6].wasPressedThisFrame)
		{
			this.Show = !this.Show;
		}
	}

	private void DrawSchemaUI(global::UImGui.UImGui _)
	{
		if (!this.Show)
		{
			return;
		}
		ImGui.SetNextWindowSize(new Vector2(700f, 100f), ImGuiCond.Once);
		ImGui.Begin("JSON Schema Generator");
		ImGui.InputTextWithHint("Schema Folder", "C:/Users/cyber/Documents/stacklands-schemas", ref this.FolderInput, 500U);
		if (ImGui.Button("Generate card.schema.json"))
		{
			SchemaGenerator.GenerateCardSchema(Path.Combine(this.FolderInput, "card.schema.json"));
		}
		ImGui.SameLine();
		if (ImGui.Button("Generate blueprint.schema.json"))
		{
			SchemaGenerator.GenerateBlueprintSchema(Path.Combine(this.FolderInput, "blueprint.schema.json"));
		}
		ImGui.SameLine();
		if (ImGui.Button("Generate boosterpack.schema.json"))
		{
			SchemaGenerator.GenerateBoosterSchema(Path.Combine(this.FolderInput, "boosterpack.schema.json"));
		}
		if (ImGui.Button("Generate ALL schemas"))
		{
			SchemaGenerator.GenerateSchemas(this.FolderInput);
		}
		ImGui.End();
	}

	private bool Show;

	private string FolderInput = "";
}
