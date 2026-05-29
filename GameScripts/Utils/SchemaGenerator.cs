using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SchemaGenerator
{
	public static JObject TypeString()
	{
		return new JObject { { "type", "string" } };
	}

	public static JObject TypeString(string description)
	{
		return new JObject
		{
			{ "type", "string" },
			{ "description", description }
		};
	}

	public static JObject TypeInt()
	{
		return new JObject { { "type", "integer" } };
	}

	public static JObject TypeFloat()
	{
		return new JObject { { "type", "number" } };
	}

	public static JObject TypeBool()
	{
		return new JObject { { "type", "boolean" } };
	}

	public static JObject TypeColor()
	{
		return new JObject
		{
			{ "type", "string" },
			{ "pattern", "^((#[0-9A-Fa-f]{8})|(#[0-9A-Fa-f]{6})|(#[0-9A-Fa-f]{4})|(#[0-9A-Fa-f]{3}))$" }
		};
	}

	public static void GenerateSchemas(string path)
	{
		if (!Directory.Exists(path))
		{
			throw new Exception("'" + path + "' is not a valid folder!");
		}
		SchemaGenerator.GenerateCardSchema(Path.Combine(path, "card.schema.json"));
		SchemaGenerator.GenerateBlueprintSchema(Path.Combine(path, "blueprint.schema.json"));
		SchemaGenerator.GenerateBoosterSchema(Path.Combine(path, "boosterpack.schema.json"));
	}

	public static void GenerateCardSchema(string path)
	{
		Debug.Log("Generating card schema..");
		JObject baseSchema = SchemaGenerator.GetBaseSchema();
		JArray jarray = baseSchema["allOf"] as JArray;
		jarray.Add(SchemaGenerator.CardBaseProps);
		jarray.Add(SchemaGenerator.OneOfAny(new string[] { "nameTerm", "nameOverride" }));
		jarray.Add(SchemaGenerator.OneOfAny(new string[] { "descriptionTerm", "descriptionOverride" }));
		JObject jobject = new JObject();
		jarray.Add(jobject);
		JArray jarray2 = new JArray();
		jobject["anyOf"] = jarray2;
		foreach (Type type in ModManager.CardClasses.Values)
		{
			if (!typeof(Blueprint).IsAssignableFrom(type))
			{
				Type type2 = type;
				List<Type> list = new List<Type> { type };
				while (type2 != typeof(CardData))
				{
					type2 = type2.BaseType;
					list.Add(type2);
				}
				JObject jobject2 = new JObject
				{
					{
						"if",
						new JObject
						{
							{
								"properties",
								new JObject { 
								{
									"script",
									new JObject { 
									{
										"const",
										type.ToString()
									} }
								} }
							},
							{
								"required",
								new JArray { "script" }
							}
						}
					},
					{
						"then",
						new JObject()
					}
				};
				jarray2.Add(jobject2);
				JObject jobject3 = new JObject();
				jobject2["then"]["properties"] = jobject3;
				foreach (Type type3 in list)
				{
					foreach (FieldInfo fieldInfo in type3.GetFields())
					{
						JObject jobject4;
						if (SchemaGenerator.FieldToJson(fieldInfo, out jobject4))
						{
							jobject3["_" + fieldInfo.Name] = jobject4;
						}
					}
				}
				jobject2["required"] = new JArray { "script" };
			}
		}
		jarray2.Add(new JObject());
		baseSchema["$defs"] = SchemaGenerator.Defs;
		baseSchema["$defs"]["enum"] = SchemaGenerator.EnumDefs;
		File.WriteAllText(path, baseSchema.ToString());
		Debug.Log("Done!");
	}

	public static void GenerateBlueprintSchema(string path)
	{
		Debug.Log("Generating blueprint schema..");
		JObject baseSchema = SchemaGenerator.GetBaseSchema();
		JArray jarray = baseSchema["allOf"] as JArray;
		jarray.Add(SchemaGenerator.BlueprintBaseProps);
		jarray.Add(SchemaGenerator.OneOfAny(new string[] { "nameTerm", "nameOverride" }));
		JObject jobject = new JObject();
		jarray.Add(jobject);
		JArray jarray2 = new JArray();
		jobject["anyOf"] = jarray2;
		foreach (Type type in ModManager.CardClasses.Values)
		{
			if (typeof(Blueprint).IsAssignableFrom(type))
			{
				Type type2 = type;
				List<Type> list = new List<Type> { type };
				while (type2 != typeof(CardData))
				{
					type2 = type2.BaseType;
					list.Add(type2);
				}
				JObject jobject2 = new JObject
				{
					{
						"if",
						new JObject
						{
							{
								"properties",
								new JObject { 
								{
									"script",
									new JObject { 
									{
										"const",
										type.ToString()
									} }
								} }
							},
							{
								"required",
								new JArray { "script" }
							}
						}
					},
					{
						"then",
						new JObject()
					}
				};
				jarray2.Add(jobject2);
				JObject jobject3 = new JObject();
				jobject2["then"]["properties"] = jobject3;
				foreach (Type type3 in list)
				{
					foreach (FieldInfo fieldInfo in type3.GetFields())
					{
						JObject jobject4;
						if (SchemaGenerator.FieldToJson(fieldInfo, out jobject4))
						{
							jobject3["_" + fieldInfo.Name] = jobject4;
						}
					}
				}
				jobject2["required"] = new JArray { "script" };
			}
		}
		jarray2.Add(new JObject());
		baseSchema["$defs"] = SchemaGenerator.Defs;
		baseSchema["$defs"]["enum"] = SchemaGenerator.EnumDefs;
		File.WriteAllText(path, baseSchema.ToString());
		Debug.Log("Done!");
	}

	public static void GenerateBoosterSchema(string path)
	{
		Debug.Log("Generating boosterpack schema..");
		JObject baseSchema = SchemaGenerator.GetBaseSchema();
		JArray jarray = baseSchema["allOf"] as JArray;
		jarray.Add(new JObject
		{
			{ "type", "object" },
			{
				"properties",
				new JObject
				{
					{
						"$schema",
						SchemaGenerator.TypeString()
					},
					{
						"id",
						SchemaGenerator.TypeString()
					},
					{
						"nameTerm",
						SchemaGenerator.Ref("term")
					},
					{
						"nameOverride",
						SchemaGenerator.TypeString()
					},
					{
						"minQuestCount",
						SchemaGenerator.TypeInt()
					},
					{
						"cost",
						SchemaGenerator.TypeInt()
					},
					{
						"icon",
						SchemaGenerator.TypeString("Sprite. Value must be the file name of an image in your mods Icons/ folder.")
					},
					{
						"location",
						SchemaGenerator.NamesFromEnum(typeof(Location))
					},
					{
						"cardBags",
						new JObject
						{
							{ "type", "array" },
							{
								"items",
								SchemaGenerator.Ref("typeCardBag")
							}
						}
					}
				}
			},
			{
				"required",
				new JArray { "id" }
			}
		});
		jarray.Add(SchemaGenerator.OneOfAny(new string[] { "nameTerm", "nameOverride" }));
		baseSchema["$defs"] = SchemaGenerator.Defs;
		baseSchema["$defs"]["enum"] = SchemaGenerator.EnumDefs;
		File.WriteAllText(path, baseSchema.ToString());
		Debug.Log("Done!");
	}

	public static bool FieldToJson(FieldInfo field, out JObject obj)
	{
		obj = new JObject();
		string text = string.Format("{0}.{1}", field.DeclaringType, field.Name);
		JObject jobject;
		if (SchemaGenerator.PropOverride.TryGetValue(text, out jobject))
		{
			obj = jobject;
			return true;
		}
		if (SchemaGenerator.PropBlacklist.Contains(text))
		{
			return false;
		}
		if (field.FieldType.IsEnum)
		{
			obj = SchemaGenerator.NamesFromEnum(field.FieldType);
			return true;
		}
		JObject jobject2;
		if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && SchemaGenerator.TypeLookup.TryGetValue(field.FieldType.GetGenericArguments()[0], out jobject2))
		{
			obj = new JObject
			{
				{ "type", "array" },
				{ "items", jobject2 }
			};
			return true;
		}
		JObject jobject3;
		if (field.FieldType.IsArray && SchemaGenerator.TypeLookup.TryGetValue(field.FieldType.GetElementType(), out jobject3))
		{
			obj = new JObject
			{
				{ "type", "array" },
				{ "items", jobject3 }
			};
			return true;
		}
		JObject jobject4;
		if (SchemaGenerator.TypeLookup.TryGetValue(field.FieldType, out jobject4))
		{
			obj = jobject4;
			return true;
		}
		Debug.LogWarning(string.Format("FieldToJson matched nothing for {0}.{1} ({2})", field.DeclaringType, field.Name, field.FieldType));
		return false;
	}

	public static JObject NamesFromEnum(Type enumType)
	{
		if (SchemaGenerator.EnumDefs[enumType.ToString()] == null)
		{
			List<string> list = new List<string>();
			foreach (string text in EnumHelper.GetNames(enumType))
			{
				list.Add(text);
			}
			SchemaGenerator.EnumDefs[enumType.ToString()] = new JArray(list);
		}
		return SchemaGenerator.Ref("enum/" + enumType.ToString());
	}

	public static JObject GetCombatStats()
	{
		JObject jobject = new JObject
		{
			{ "type", "object" },
			{
				"properties",
				new JObject()
			}
		};
		JObject jobject2 = jobject["properties"] as JObject;
		foreach (FieldInfo fieldInfo in typeof(CombatStats).GetFields())
		{
			JObject jobject3;
			if (SchemaGenerator.TypeLookup.TryGetValue(fieldInfo.FieldType, out jobject3))
			{
				jobject2[fieldInfo.Name] = jobject3;
			}
		}
		return jobject;
	}

	public static JObject GetSubprint()
	{
		JObject jobject = new JObject
		{
			{ "type", "object" },
			{
				"properties",
				new JObject()
			}
		};
		JObject jobject2 = jobject["properties"] as JObject;
		foreach (FieldInfo fieldInfo in typeof(Subprint).GetFields())
		{
			JObject jobject3;
			if (SchemaGenerator.FieldToJson(fieldInfo, out jobject3))
			{
				jobject2[fieldInfo.Name] = jobject3;
			}
		}
		return jobject;
	}

	public static JObject GetCardBag()
	{
		JObject jobject = new JObject
		{
			{ "type", "object" },
			{
				"allOf",
				new JArray
				{
					new JObject { 
					{
						"properties",
						new JObject { 
						{
							"CardBagType",
							SchemaGenerator.TypeString()
						} }
					} }
				}
			}
		};
		JArray jarray = jobject["allOf"] as JArray;
		foreach (KeyValuePair<CardBagType, List<string>> keyValuePair in new Dictionary<CardBagType, List<string>>
		{
			{
				CardBagType.Chances,
				new List<string> { "Chances" }
			},
			{
				CardBagType.SetPack,
				new List<string> { "SetPackCards" }
			},
			{
				CardBagType.SetCardBag,
				new List<string> { "SetCardBag", "UseFallbackBag", "FallbackBag" }
			},
			{
				CardBagType.Enemies,
				new List<string> { "EnemyCardBag", "StrengthLevel" }
			}
		})
		{
			CardBagType key = keyValuePair.Key;
			List<string> value = keyValuePair.Value;
			CardBagType cardBagType = key;
			JObject jobject2 = new JObject
			{
				{
					"if",
					new JObject
					{
						{
							"properties",
							new JObject { 
							{
								"CardBagType",
								new JObject { 
								{
									"const",
									cardBagType.ToString()
								} }
							} }
						},
						{
							"required",
							new JArray { "CardBagType" }
						}
					}
				},
				{
					"then",
					new JObject()
				}
			};
			JObject jobject3 = new JObject();
			jobject2["then"]["properties"] = jobject3;
			foreach (string text in value)
			{
				JObject jobject4;
				if (SchemaGenerator.FieldToJson(typeof(CardBag).GetField(text), out jobject4))
				{
					jobject3[text] = jobject4;
				}
			}
			jarray.Add(jobject2);
		}
		return jobject;
	}

	public static JObject GetCardChance()
	{
		return new JObject { 
		{
			"allOf",
			new JArray
			{
				new JObject { 
				{
					"properties",
					new JObject
					{
						{
							"Id",
							SchemaGenerator.Ref("cardId")
						},
						{
							"Chance",
							SchemaGenerator.TypeInt()
						},
						{
							"HasMaxCount",
							SchemaGenerator.TypeBool()
						},
						{
							"HasPrerequisiteCard",
							SchemaGenerator.TypeBool()
						},
						{
							"IsEnemy",
							SchemaGenerator.TypeBool()
						}
					}
				} },
				new JObject
				{
					{
						"if",
						new JObject
						{
							{
								"properties",
								new JObject { 
								{
									"HasPrerequisiteCard",
									new JObject { { "const", true } }
								} }
							},
							{
								"required",
								new JArray { "HasPrerequisiteCard" }
							}
						}
					},
					{
						"then",
						new JObject { 
						{
							"properties",
							new JObject { 
							{
								"PrerequisiteCardId",
								SchemaGenerator.Ref("cardId")
							} }
						} }
					}
				},
				new JObject
				{
					{
						"if",
						new JObject { 
						{
							"allOf",
							new JArray
							{
								new JObject
								{
									{
										"properties",
										new JObject { 
										{
											"HasMaxCount",
											new JObject { { "const", true } }
										} }
									},
									{
										"required",
										new JArray { "HasMaxCount" }
									}
								},
								new JObject { 
								{
									"not",
									new JObject
									{
										{
											"properties",
											new JObject { 
											{
												"IsEnemy",
												new JObject { { "const", true } }
											} }
										},
										{
											"required",
											new JArray { "IsEnemy" }
										}
									}
								} }
							}
						} }
					},
					{
						"then",
						new JObject { 
						{
							"properties",
							new JObject { 
							{
								"MaxCountToGive",
								SchemaGenerator.TypeInt()
							} }
						} }
					}
				},
				new JObject
				{
					{
						"if",
						new JObject
						{
							{
								"properties",
								new JObject { 
								{
									"IsEnemy",
									new JObject { { "const", true } }
								} }
							},
							{
								"required",
								new JArray { "IsEnemy" }
							}
						}
					},
					{
						"then",
						new JObject { 
						{
							"properties",
							new JObject
							{
								{
									"EnemyBag",
									SchemaGenerator.NamesFromEnum(typeof(EnemySetCardBag))
								},
								{
									"Strength",
									SchemaGenerator.TypeFloat()
								}
							}
						} }
					}
				}
			}
		} };
	}

	public static JObject GetCardIds()
	{
		List<string> list = WorldManager.instance.CardDataPrefabs.Select<CardData, string>((CardData c) => c.Id).ToList<string>();
		list.Remove("ideas_base");
		return new JObject
		{
			{ "type", "string" },
			{
				"enum",
				new JArray(list)
			}
		};
	}

	public static JObject GetTerms()
	{
		JObject jobject = new JObject();
		jobject.Add("type", "string");
		string text = "enum";
		JArray jarray = new JArray();
		jarray.Add(MewtationsLoc.instance.CurrentLocSet.AllTerms.Select<MewtationsTerm, string>((MewtationsTerm t) => t.Id));
		jobject.Add(text, jarray);
		return jobject;
	}

	public static JObject GetBaseSchema()
	{
		string text = "Stacklands (v" + Application.version + ")";
		foreach (Mod mod in ModManager.LoadedMods)
		{
			text = string.Concat(new string[]
			{
				text,
				"; ",
				mod.Manifest.Id,
				" (v",
				mod.Manifest.Version,
				")"
			});
		}
		return new JObject
		{
			{ "$comment", text },
			{ "type", "object" },
			{
				"allOf",
				new JArray()
			}
		};
	}

	public static JObject OneOfAny(params string[] props)
	{
		JObject jobject = new JObject { 
		{
			"oneOf",
			new JArray()
		} };
		foreach (string text in props)
		{
			(jobject["oneOf"] as JArray).Add(new JObject { 
			{
				"required",
				new JArray { text }
			} });
		}
		return jobject;
	}

	public static JObject Ref(string str)
	{
		return new JObject { 
		{
			"$ref",
			"#/$defs/" + str
		} };
	}

	private static JObject EnumDefs = new JObject();

	public static JObject CardBaseProps = new JObject
	{
		{ "type", "object" },
		{
			"properties",
			new JObject
			{
				{
					"$schema",
					SchemaGenerator.TypeString()
				},
				{
					"id",
					SchemaGenerator.TypeString()
				},
				{
					"nameTerm",
					SchemaGenerator.Ref("term")
				},
				{
					"nameOverride",
					SchemaGenerator.TypeString()
				},
				{
					"descriptionTerm",
					SchemaGenerator.Ref("term")
				},
				{
					"descriptionOverride",
					SchemaGenerator.TypeString()
				},
				{
					"type",
					SchemaGenerator.NamesFromEnum(typeof(CardType))
				},
				{
					"icon",
					SchemaGenerator.TypeString("Sprite. Value must be the file name of an image in your mods Icons/ folder.")
				},
				{
					"pickupSound",
					SchemaGenerator.TypeString()
				},
				{
					"value",
					SchemaGenerator.TypeInt()
				},
				{
					"hideFromCardopedia",
					SchemaGenerator.TypeBool()
				},
				{
					"script",
					SchemaGenerator.TypeString()
				}
			}
		},
		{
			"required",
			new JArray { "id" }
		}
	};

	public static JObject BlueprintBaseProps = new JObject
	{
		{ "type", "object" },
		{
			"properties",
			new JObject
			{
				{
					"$schema",
					SchemaGenerator.TypeString()
				},
				{
					"id",
					SchemaGenerator.TypeString()
				},
				{
					"nameTerm",
					SchemaGenerator.Ref("term")
				},
				{
					"nameOverride",
					SchemaGenerator.TypeString()
				},
				{
					"group",
					SchemaGenerator.NamesFromEnum(typeof(BlueprintGroup))
				},
				{
					"icon",
					SchemaGenerator.TypeString("Sprite. Value must be the file name of an image in your mods Icons/ folder.")
				},
				{
					"value",
					SchemaGenerator.TypeInt()
				},
				{
					"hideFromCardopedia",
					SchemaGenerator.TypeBool()
				},
				{
					"hideFromIdeasTab",
					SchemaGenerator.TypeBool()
				},
				{
					"isInvention",
					SchemaGenerator.TypeBool()
				},
				{
					"needsExactMatch",
					SchemaGenerator.TypeBool()
				},
				{
					"script",
					SchemaGenerator.TypeString()
				},
				{
					"subprints",
					new JObject
					{
						{ "type", "array" },
						{
							"items",
							SchemaGenerator.Ref("typeSubprint")
						}
					}
				}
			}
		},
		{
			"required",
			new JArray { "id" }
		}
	};

	public static List<string> PropBlacklist = new List<string>
	{
		"CardData.Id", "CardData.descriptionOverride", "CardData.nameOverride", "CardData.NameTerm", "CardData.DescriptionTerm", "CardData.PickupSound", "CardData.UniqueId", "CardData.ParentUniqueId", "CardData.EquipmentHolderUniqueId", "CardData.Value",
		"CardData.Icon", "CardData.HideFromCardopedia", "CardData.MyGameCard", "CardData.MyCardType", "CardData.StatusEffects", "CardData.CreationMonth", "CardData.ExpectedValue", "Altar.inCutscene", "Animal.CreateTimer",
		"Combatable._combatableDescription", "Combatable.MyConflict",
		"Combatable.CurrentHitText", "Combatable.StunTimer", "Conveyor.Direction", "Conveyor.corners", "DragonEgg.NormalIcon", "DragonEgg.CrackedIcon", "DragonEgg.CrackedIcon_2", "DragonEgg.CrackedSound", "Food.SpoilTime",
		"Mimic.TreasureChestIcon", "Mimic.RealIcon", "Mob.MoveTimer", "Mob.CurrentTarget", "Mob.moveFlag", "Poop.MakeSickTimer", "ResourceChest.SpecialIcon", "Royal.MoveTimer", "Spirit.SpiritSounds", "StrangePortal.SpawnTimer",
		"StrangePortal.TravelTimer", "TrashCan.DestroySounds", "University.InventionSound", "University.SpecialIcon", "WickedWitch.WitchDieSounds", "WickedWitch.NormalIcon", "WickedWitch.OldLadyIcon", "WishingWell.SpecialIcon", "WishingWell.WishSound", "Equipable.AttackSounds",
		"Equipable.blueprint", "Equipable._equipableInfo", "Subprint.SubprintIndex", "Subprint.ParentBlueprint", "Blueprint.BlueprintGroup", "Blueprint.HideFromIdeasTab", "Blueprint.IsInvention", "Blueprint.NeedsExactMatch", "Blueprint.Subprints"
	};

	public static Dictionary<string, JObject> PropOverride = new Dictionary<string, JObject>
	{
		{
			"CardBag.SetPackCards",
			SchemaGenerator.Ref("cardIdArray")
		},
		{
			"Subprint.RequiredCards",
			SchemaGenerator.TypeString()
		},
		{
			"Subprint.CardsToRemove",
			SchemaGenerator.TypeString()
		},
		{
			"Subprint.ExtraResultCards",
			SchemaGenerator.TypeString()
		},
		{
			"Subprint.ResultCard",
			SchemaGenerator.Ref("cardId")
		}
	};

	public static Dictionary<Type, JObject> TypeLookup = new Dictionary<Type, JObject>
	{
		{
			typeof(string),
			SchemaGenerator.TypeString()
		},
		{
			typeof(int),
			SchemaGenerator.TypeInt()
		},
		{
			typeof(float),
			SchemaGenerator.TypeFloat()
		},
		{
			typeof(bool),
			SchemaGenerator.TypeBool()
		},
		{
			typeof(BaitBag),
			SchemaGenerator.Ref("typeBaitBag")
		},
		{
			typeof(CardBag),
			SchemaGenerator.Ref("typeCardBag")
		},
		{
			typeof(CardChance),
			SchemaGenerator.Ref("typeCardChance")
		},
		{
			typeof(CardPalette),
			SchemaGenerator.Ref("typeCardPalette")
		},
		{
			typeof(Color),
			SchemaGenerator.Ref("typeColor")
		},
		{
			typeof(CombatStats),
			SchemaGenerator.Ref("typeCombatStats")
		},
		{
			typeof(Subprint),
			SchemaGenerator.Ref("typeSubprint")
		},
		{
			typeof(Sprite),
			SchemaGenerator.TypeString("Sprite. Value must be the file name of an image in your mods Icons/ folder.")
		}
	};

	public static JObject Defs = new JObject
	{
		{
			"typeColor",
			SchemaGenerator.TypeColor()
		},
		{
			"typeCardPalette",
			new JObject
			{
				{ "type", "object" },
				{
					"properties",
					new JObject
					{
						{
							"Color",
							SchemaGenerator.Ref("typeColor")
						},
						{
							"Color2",
							SchemaGenerator.Ref("typeColor")
						},
						{
							"Icon",
							SchemaGenerator.Ref("typeColor")
						}
					}
				}
			}
		},
		{
			"typeCombatStats",
			SchemaGenerator.GetCombatStats()
		},
		{
			"typeBaitBag",
			new JObject
			{
				{ "type", "object" },
				{
					"properties",
					new JObject { 
					{
						"baitId",
						SchemaGenerator.TypeString()
					} }
				}
			}
		},
		{
			"typeCardChance",
			SchemaGenerator.GetCardChance()
		},
		{
			"typeCardBag",
			SchemaGenerator.GetCardBag()
		},
		{
			"typeSubprint",
			SchemaGenerator.GetSubprint()
		},
		{
			"cardId",
			SchemaGenerator.GetCardIds()
		},
		{
			"cardIdArray",
			new JObject
			{
				{ "type", "array" },
				{
					"items",
					SchemaGenerator.Ref("cardId")
				}
			}
		},
		{
			"term",
			SchemaGenerator.GetTerms()
		}
	};
}
