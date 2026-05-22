using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using UImGui;
using UnityEngine;
using UnityEngine.InputSystem;

public class PackUI : MonoBehaviour
{
	private void Start()
	{
		UImGuiUtility.Layout += this.DrawPackUI;
	}

	private void Update()
	{
		if (Keyboard.current[Key.F5].wasPressedThisFrame)
		{
			this.Show = !this.Show;
		}
	}

	private void SetPackContextMenu(BoosterpackData bpd, int i)
	{
		if (ImGui.Selectable("Add card to pack"))
		{
			GUIUtility.systemCopyBuffer = string.Format("WorldManager.instance.GetBoosterData(\"{0}\").CardBags[{1}].SetPackCards.Add(\"card_id\");", bpd.BoosterId, i);
		}
		if (ImGui.Selectable("Remove card from pack"))
		{
			GUIUtility.systemCopyBuffer = string.Format("WorldManager.instance.GetBoosterData(\"{0}\").CardBags[{1}].SetPackCards.Remove(\"card_id\");", bpd.BoosterId, i);
		}
	}

	private void ChancesContextMenu(BoosterpackData bpd, int i)
	{
		if (ImGui.Selectable("Add chance"))
		{
			GUIUtility.systemCopyBuffer = string.Format("WorldManager.instance.GetBoosterData(\"{0}\").CardBags[{1}].Chances.Add(new CardChance(\"card_id\", 1))", bpd.BoosterId, i);
		}
	}

	private void SetCardBagContextMenu(BoosterpackData bpd, int i)
	{
		CardBag cardBag = bpd.CardBags[i];
		if (ImGui.Selectable("Add card to SetCardBag"))
		{
			string text;
			if (((int[])Enum.GetValues(typeof(SetCardBagType))).Contains((int)cardBag.SetCardBag))
			{
				text = "SetCardBagType." + EnumHelper.GetName<SetCardBagType>((int)cardBag.SetCardBag);
			}
			else
			{
				text = string.Format("(SetCardBagType){0}", (int)cardBag.SetCardBag);
			}
			GUIUtility.systemCopyBuffer = "WorldManager.instance.GameDataLoader.AddCardToSetCardBag(" + text + ", \"card_id\", 1)";
		}
	}

	private void EnemiesContextMenu(BoosterpackData bpd, int i)
	{
	}

	private void DrawPackUI(global::UImGui.UImGui _)
	{
		if (!this.Show)
		{
			return;
		}
		ImGui.Begin("Pack Data Inspector");
		foreach (BoosterpackData boosterpackData in WorldManager.instance.BoosterPackDatas)
		{
			if (ImGui.CollapsingHeader(boosterpackData.Name + " (" + boosterpackData.BoosterId + ")"))
			{
				ImGui.PushID(boosterpackData.BoosterId);
				for (int i = 0; i < boosterpackData.CardBags.Count; i++)
				{
					CardBag cardBag = boosterpackData.CardBags[i];
					string text = string.Format("{0} - {1}", i, EnumHelper.GetName<CardBagType>((int)cardBag.CardBagType));
					if (cardBag.CardBagType == CardBagType.SetCardBag)
					{
						text = text + ": " + EnumHelper.GetName<SetCardBagType>((int)cardBag.SetCardBag);
					}
					if (cardBag.CardBagType == CardBagType.Enemies)
					{
						text += string.Format(": {0} ({1} strength)", EnumHelper.GetName<EnemySetCardBag>((int)cardBag.EnemyCardBag), cardBag.StrengthLevel);
					}
					text += string.Format(" | {0} card{1}", cardBag.CardsInPack, (cardBag.CardsInPack != 1) ? "s" : "");
					if (ImGui.TreeNode(text))
					{
						if (cardBag.CardBagType == CardBagType.SetPack)
						{
							if (ImGui.BeginPopupContextItem())
							{
								ImGui.Text("Copy code..");
								this.SetPackContextMenu(boosterpackData, i);
								ImGui.EndPopup();
							}
							foreach (string text2 in cardBag.SetPackCards)
							{
								ImGui.Text("  " + text2);
								PackUI.IdTooltip(text2);
							}
						}
						if (cardBag.CardBagType == CardBagType.Chances)
						{
							if (ImGui.BeginPopupContextItem())
							{
								ImGui.Text("Copy code..");
								this.ChancesContextMenu(boosterpackData, i);
								ImGui.EndPopup();
							}
							float num = 0f;
							foreach (CardChance cardChance in cardBag.Chances)
							{
								num += (float)cardChance.Chance;
							}
							foreach (CardChance cardChance2 in cardBag.Chances)
							{
								cardChance2.PercentageChance = (cardChance2.PercentageChance = (float)cardChance2.Chance / num);
							}
							foreach (CardChance cardChance3 in cardBag.Chances)
							{
								string text3 = string.Format("   {0} {1} ({2:F2}%%)", cardChance3.Chance, cardChance3.Id, cardChance3.PercentageChance * 100f);
								if (cardChance3.HasMaxCount)
								{
									text3 += string.Format(" | max {0}", cardChance3.MaxCountToGive);
								}
								if (cardChance3.HasPrerequisiteCard)
								{
									text3 = text3 + " | prereq. " + cardChance3.PrerequisiteCardId;
								}
								ImGui.Text(text3);
								PackUI.IdTooltip(cardChance3.Id);
							}
						}
						if (cardBag.CardBagType == CardBagType.SetCardBag)
						{
							if (ImGui.BeginPopupContextItem())
							{
								ImGui.Text("Copy code..");
								this.SetCardBagContextMenu(boosterpackData, i);
								ImGui.EndPopup();
							}
							List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, cardBag.SetCardBag, null);
							float num2 = 0f;
							foreach (CardChance cardChance4 in chancesForSetCardBag)
							{
								num2 += (float)cardChance4.Chance;
							}
							foreach (CardChance cardChance5 in chancesForSetCardBag)
							{
								cardChance5.PercentageChance = (cardChance5.PercentageChance = (float)cardChance5.Chance / num2);
							}
							foreach (CardChance cardChance6 in chancesForSetCardBag)
							{
								string text4 = string.Format("   {0} {1} ({2:F2}%%)", cardChance6.Chance, cardChance6.Id, cardChance6.PercentageChance * 100f);
								if (cardChance6.HasMaxCount)
								{
									text4 += string.Format(" | max {0}", cardChance6.MaxCountToGive);
								}
								if (cardChance6.HasPrerequisiteCard)
								{
									text4 = text4 + " | prereq. " + cardChance6.PrerequisiteCardId;
								}
								ImGui.Text(text4);
								PackUI.IdTooltip(cardChance6.Id);
							}
						}
						if (cardBag.CardBagType == CardBagType.Enemies)
						{
							if (ImGui.BeginPopupContextItem())
							{
								ImGui.Text("Copy code..");
								this.EnemiesContextMenu(boosterpackData, i);
								ImGui.EndPopup();
							}
							List<CardChance> chancesForSetCardBag2 = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBag(cardBag.EnemyCardBag), null);
							float num3 = 0f;
							foreach (CardChance cardChance7 in chancesForSetCardBag2)
							{
								num3 += (float)cardChance7.Chance;
							}
							foreach (CardChance cardChance8 in chancesForSetCardBag2)
							{
								cardChance8.PercentageChance = (cardChance8.PercentageChance = (float)cardChance8.Chance / num3);
							}
							foreach (CardChance cardChance9 in chancesForSetCardBag2)
							{
								string text5 = string.Format("{0} {1} ({2:F2}%)", cardChance9.Chance, cardChance9.Id, cardChance9.PercentageChance * 100f);
								if (cardChance9.HasMaxCount)
								{
									text5 += string.Format(" | max {0}", cardChance9.MaxCountToGive);
								}
								if (cardChance9.HasPrerequisiteCard)
								{
									text5 = text5 + " | prereq. " + cardChance9.PrerequisiteCardId;
								}
								if (ImGui.TreeNode(text5))
								{
									PackUI.IdTooltip(cardChance9.Id);
									Combatable combatable = (Combatable)WorldManager.instance.GetCardPrefab(cardChance9.Id, true);
									ImGui.Text("Head:");
									foreach (Equipable equipable in combatable.PossibleEquipables.FindAll((Equipable e) => e.EquipableType == EquipableType.Head))
									{
										ImGui.Text("  " + equipable.Id);
										PackUI.IdTooltip(equipable.Id);
									}
									ImGui.Text("Torso:");
									foreach (Equipable equipable2 in combatable.PossibleEquipables.FindAll((Equipable e) => e.EquipableType == EquipableType.Torso))
									{
										ImGui.Text("  " + equipable2.Id);
										PackUI.IdTooltip(equipable2.Id);
									}
									ImGui.Text("Weapon:");
									foreach (Equipable equipable3 in combatable.PossibleEquipables.FindAll((Equipable e) => e.EquipableType == EquipableType.Weapon))
									{
										ImGui.Text("  " + equipable3.Id);
										PackUI.IdTooltip(equipable3.Id);
									}
									ImGui.TreePop();
								}
							}
						}
						ImGui.TreePop();
					}
				}
				ImGui.PopID();
			}
		}
		ImGui.End();
	}

	public static void IdTooltip(string id)
	{
		if (ImGui.IsItemHovered())
		{
			ImGui.BeginTooltip();
			ImGui.Text(WorldManager.instance.GetCardPrefab(id, true).Name);
			ImGui.EndTooltip();
		}
	}

	private bool Show;
}
