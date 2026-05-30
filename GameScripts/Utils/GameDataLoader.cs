using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GameDataLoader
{
	public GameDataLoader(bool loadSpiritDlc = true, bool loadCitiesDlc = true)
	{
		GameDataLoader.instance = this;
		AssetBundle.UnloadAllAssetBundles(true);
		List<CardData> list = Resources.LoadAll<CardData>("Cards/Main_Cards").ToList<CardData>();
		List<CardData> list2 = Resources.LoadAll<CardData>("Cards/Island_Cards").ToList<CardData>();
		List<CardData> list3 = Resources.LoadAll<CardData>("Cards/Forest_Cards").ToList<CardData>();
		List<CardData> list4 = Resources.LoadAll<CardData>("Cards/Order_Cards").ToList<CardData>();
		foreach (CardData cardData in list)
		{
			cardData.CardUpdateType = CardUpdateType.Main;
		}
		foreach (CardData cardData2 in list2)
		{
			cardData2.CardUpdateType = CardUpdateType.Island;
		}
		foreach (CardData cardData3 in list3)
		{
			cardData3.CardUpdateType = CardUpdateType.Forest;
		}
		foreach (CardData cardData4 in list4)
		{
			cardData4.CardUpdateType = CardUpdateType.Order;
		}
		this.CardDataPrefabs = list;
		this.CardDataPrefabs.AddRange(list2);
		this.CardDataPrefabs.AddRange(list3);
		this.CardDataPrefabs.AddRange(list4);
		if (Application.isPlaying && PlatformHelper.HasModdingSupport)
		{
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				Converters = new List<JsonConverter>
				{
					new StringColorConverter(),
					new StringEnumConverter(),
					new StringSpriteConverter()
				}
			};
			try
			{
				this.CardDataPrefabs.AddRange(this.LoadModCards());
				this.CardDataPrefabs.AddRange(this.LoadModBlueprints());
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		this.BoosterpackDatas = Resources.LoadAll<BoosterpackData>("Boosters").ToList<BoosterpackData>();
		if (Application.isPlaying && PlatformHelper.HasModdingSupport)
		{
			try
			{
				this.BoosterpackDatas.AddRange(this.LoadModBoosters());
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
		}
		this.Demands = Resources.LoadAll<Demand>("Greed_Demands").ToList<Demand>();
		GameDataLoader.codeReferencedCards = Resources.Load<TextAsset>("Misc/CodeReferencedCards").text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		TextAsset textAsset = Resources.Load<TextAsset>("Misc/VillagerNames");
		this.VillagerNames = textAsset.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		this.SetCardBags = new List<SetCardBagData>();
		this.SetCardBags.AddRange(Resources.LoadAll<SetCardBagData>("SetCardBags"));
		this.Cutscenes = Resources.LoadAll<ScriptableCutscene>("Cutscenes").ToList<ScriptableCutscene>();
		if (loadSpiritDlc)
		{
			this.SpiritDlcLoaded = this.TryLoadDlc("Spirits_DLC", CardUpdateType.Spirit);
		}
		if (loadCitiesDlc)
		{
			this.CitiesDlcLoaded = this.TryLoadDlc("Cities_DLC", CardUpdateType.Cities);
		}
		this.InitializeLoadedData();
		this.ProfanityChecker = new ProfanityChecker();
		this.CurrentlyLoadingMod = null;
	}

	private bool TryLoadDlc(string folderPath, CardUpdateType updateType)
	{
		ISokBundle sokBundle2;
		if (!Application.isEditor)
		{
			ISokBundle sokBundle = new RuntimeSokBundle();
			sokBundle2 = sokBundle;
		}
		else
		{
			ISokBundle sokBundle = new EditorSokBundle();
			sokBundle2 = sokBundle;
		}
		ISokBundle sokBundle3 = sokBundle2;
		if (!sokBundle3.Load(folderPath))
		{
			Debug.LogError("Failed loading the " + folderPath + " bundle");
			return false;
		}
		foreach (GameObject gameObject in sokBundle3.LoadAssets<GameObject>())
		{
			CardData component = gameObject.GetComponent<CardData>();
			if (component != null)
			{
				component.CardUpdateType = updateType;
				this.CardDataPrefabs.Add(component);
			}
		}
		this.SetCardBags.AddRange(sokBundle3.LoadAssets<SetCardBagData>());
		this.BoosterpackDatas.AddRange(sokBundle3.LoadAssets<BoosterpackData>());
		return true;
	}

	private void InitializeLoadedData()
	{
		foreach (CardData cardData in this.CardDataPrefabs)
		{
			if (this.idToCard.ContainsKey(cardData.Id))
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Duplicate card id! ",
					cardData.Id,
					" - ",
					cardData.gameObject.name,
					" & ",
					this.idToCard[cardData.Id].gameObject.name
				}));
			}
			else
			{
				this.idToCard[cardData.Id] = cardData;
			}
		}
		this.BoosterpackDatas = this.BoosterpackDatas.OrderBy<BoosterpackData, int>((BoosterpackData x) => x.MinAchievementCount).ToList<BoosterpackData>();
		foreach (BoosterpackData boosterpackData in this.BoosterpackDatas)
		{
			this.idToBooster[boosterpackData.BoosterId] = boosterpackData;
		}
		this.BlueprintPrefabs = this.CardDataPrefabs.OfType<Blueprint>().ToList<Blueprint>();
		foreach (Blueprint blueprint in this.BlueprintPrefabs)
		{
			blueprint.Init(this);
		}
	}

	public List<CardData> LoadModCards()
	{
		List<CardData> list = new List<CardData>();
		foreach (Mod mod in ModManager.LoadedMods)
		{
			this.CurrentlyLoadingMod = mod;
			string text = Path.Combine(mod.Path, "Cards");
			if (Directory.Exists(text))
			{
				foreach (FileInfo fileInfo in new DirectoryInfo(text).GetFiles("*.json", SearchOption.AllDirectories))
				{
					string text2 = File.ReadAllText(Path.Combine(text, fileInfo.Name));
					if (!string.IsNullOrEmpty(text2))
					{
						try
						{
							ModCard modCard = JsonConvert.DeserializeObject<ModCard>(text2);
							Debug.Log("loading modded card: " + modCard.Id);
							CardData cardData = this.LoadModCard(modCard, mod);
							cardData.gameObject.name = EnumHelper.GetName<CardType>((int)cardData.MyCardType) + "_" + cardData.Id;
							list.Add(cardData);
						}
						catch (Exception ex)
						{
							Debug.LogError("Failed to load card: " + ex.Message);
						}
					}
				}
			}
		}
		foreach (CardData cardData2 in list)
		{
			cardData2.CardUpdateType = CardUpdateType.Mod;
		}
		return list;
	}

	public CardData LoadModCard(ModCard mc, Mod mod)
	{
		GameObject gameObject = new GameObject("testcard");
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		gameObject.SetActive(false);
		if (!ModManager.CardClasses.ContainsKey(mc.Script))
		{
			throw new Exception("invalid script: " + mc.Script);
		}
		CardData cd = gameObject.AddComponent(ModManager.CardClasses[mc.Script]) as CardData;
		cd.Id = mc.Id;
		cd.NameTerm = mc.NameTerm ?? "";
		cd.DescriptionTerm = mc.DescriptionTerm ?? "";
		cd.nameOverride = mc.NameOverride;
		cd.descriptionOverride = mc.DescriptionOverride;
		cd.Value = mc.Value;
		cd.MyCardType = EnumHelper.ParseEnum<CardType>(mc.Type, new int?(0));
		cd.HideFromCardopedia = mc.HideFromCardopedia;
		if (!string.IsNullOrEmpty(mc.Icon) && !mc.Icon.EndsWith(".png"))
		{
			CardData cd2 = cd;
			CardData cardData = this.CardDataPrefabs.Find((CardData c) => c.Id == mc.Icon);
			cd2.Icon = ((cardData != null) ? cardData.Icon : null);
			if (cd.Icon == null)
			{
				Debug.LogWarning(string.Concat(new string[] { "Tried to find vanilla card ", mc.Icon, " for icon, but it does not exist! Did you mean ", mc.Icon, ".png?" }));
			}
		}
		else
		{
			string text = Path.Combine(mod.Path, "Icons", mc.Icon ?? (mc.Id + ".png"));
			if (File.Exists(text))
			{
				cd.Icon = ResourceHelper.LoadSpriteFromPath(text);
			}
			else
			{
				Debug.LogWarning("Missing or invalid Icon for " + cd.Id + "!");
			}
		}
		string text2 = Path.Combine(mod.Path, "Sounds", mc.PickupSound ?? (mc.Id + ".wav"));
		if (File.Exists(text2))
		{
			cd.PickupSoundGroup = PickupSoundGroup.Custom;
			ModManager.instance.StartCoroutine(ResourceHelper.LoadAudioClipFromPath(text2, delegate(AudioClip ac)
			{
				Debug.Log("Loaded audio for " + cd.Id);
				cd.PickupSound = ac;
			}, null));
		}
		else
		{
			Debug.LogWarning("Missing or invalid PickupSound for " + cd.Id + "!");
		}
		if (mc.ExtraProps != null)
		{
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(null);
			foreach (KeyValuePair<string, JToken> keyValuePair in mc.ExtraProps)
			{
				if (keyValuePair.Key.StartsWith("_"))
				{
					string text3 = keyValuePair.Key.TrimStart('_');
					FieldInfo field = cd.GetType().GetField(text3);
					if (field != null)
					{
						field.SetValue(cd, jsonSerializer.Deserialize(keyValuePair.Value.CreateReader(), field.FieldType));
					}
					else
					{
						Debug.LogError(string.Format("Property {0} doesn't exist on {1}", text3, cd.GetType()));
					}
				}
			}
		}
		return cd;
	}

	public List<CardData> LoadModBlueprints()
	{
		List<CardData> list = new List<CardData>();
		foreach (Mod mod in ModManager.LoadedMods)
		{
			this.CurrentlyLoadingMod = mod;
			string text = Path.Combine(mod.Path, "Blueprints");
			if (Directory.Exists(text))
			{
				foreach (FileInfo fileInfo in new DirectoryInfo(text).GetFiles("*.json", SearchOption.AllDirectories))
				{
					string text2 = File.ReadAllText(Path.Combine(text, fileInfo.Name));
					if (!string.IsNullOrEmpty(text2))
					{
						try
						{
							ModBlueprint modBlueprint = JsonConvert.DeserializeObject<ModBlueprint>(text2);
							Debug.Log("loading modded blueprint: " + modBlueprint.Id);
							CardData cardData = this.LoadModBlueprint(modBlueprint, mod);
							cardData.gameObject.name = "Blueprint_" + cardData.Id;
							list.Add(cardData);
						}
						catch (Exception ex)
						{
							Debug.LogError("Failed to load blueprint: " + ex.Message + "\n" + ex.StackTrace);
						}
					}
				}
			}
		}
		foreach (CardData cardData2 in list)
		{
			cardData2.CardUpdateType = CardUpdateType.Mod;
		}
		return list;
	}

	public CardData LoadModBlueprint(ModBlueprint mp, Mod mod)
	{
		GameObject gameObject = new GameObject("testblueprint");
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		gameObject.SetActive(false);
		Blueprint blueprint = gameObject.AddComponent(ModManager.CardClasses[mp.Script]) as Blueprint;
		blueprint.Id = mp.Id;
		blueprint.NameTerm = mp.NameTerm ?? "";
		blueprint.nameOverride = mp.NameOverride;
		blueprint.BlueprintGroup = EnumHelper.ParseEnum<BlueprintGroup>(mp.Group, new int?(0));
		blueprint.Value = mp.Value;
		blueprint.HideFromCardopedia = mp.HideFromCardopedia;
		blueprint.HideFromIdeasTab = mp.HideFromIdeasTab;
		blueprint.IsInvention = mp.IsInvention;
		blueprint.NeedsExactMatch = mp.NeedsExactMatch;
		blueprint.MyCardType = CardType.Ideas;
		if (!string.IsNullOrEmpty(mp.Icon) && !mp.Icon.EndsWith(".png"))
		{
			CardData cardData = blueprint;
			CardData cardData2 = this.CardDataPrefabs.Find((CardData c) => c.Id == mp.Icon);
			cardData.Icon = ((cardData2 != null) ? cardData2.Icon : null);
			if (blueprint.Icon == null)
			{
				Debug.LogWarning(string.Concat(new string[] { "Tried to find vanilla card ", mp.Icon, " for icon, but it does not exist! Did you mean ", mp.Icon, ".png?" }));
			}
		}
		else
		{
			string text = Path.Combine(mod.Path, "Icons", mp.Icon ?? (mp.Id + ".png"));
			if (File.Exists(text))
			{
				blueprint.Icon = ResourceHelper.LoadSpriteFromPath(text);
			}
		}
		foreach (ModSubprint modSubprint in mp.Subprints)
		{
			Subprint subprint = new Subprint();
			subprint.RequiredCards = (from str in modSubprint.RequiredCards.Split(',', StringSplitOptions.None)
				select str.Trim()).ToArray<string>();
			Subprint subprint2 = subprint;
			string cardsToRemove = modSubprint.CardsToRemove;
			string[] array;
			if (cardsToRemove == null)
			{
				array = null;
			}
			else
			{
				array = (from str in cardsToRemove.Split(',', StringSplitOptions.None)
					select str.Trim()).ToArray<string>();
			}
			subprint2.CardsToRemove = array;
			subprint.ResultCard = modSubprint.ResultCard;
			subprint.ResultAction = modSubprint.ResultAction;
			Subprint subprint3 = subprint;
			string extraResultCards = modSubprint.ExtraResultCards;
			string[] array2;
			if (extraResultCards == null)
			{
				array2 = null;
			}
			else
			{
				array2 = (from str in extraResultCards.Split(',', StringSplitOptions.None)
					select str.Trim()).ToArray<string>();
			}
			subprint3.ExtraResultCards = array2 ?? new string[0];
			subprint.Time = modSubprint.Time;
			subprint.StatusTerm = modSubprint.StatusTerm ?? "";
			subprint.statusOverride = modSubprint.StatusOverride;
			blueprint.Subprints.Add(subprint);
		}
		return blueprint;
	}

	public List<BoosterpackData> LoadModBoosters()
	{
		List<BoosterpackData> list = new List<BoosterpackData>();
		if (ModManager.LoadedMods == null)
		{
			return list;
		}
		foreach (Mod mod in ModManager.LoadedMods)
		{
			this.CurrentlyLoadingMod = mod;
			string text = Path.Combine(mod.Path, "Boosterpacks");
			if (Directory.Exists(text))
			{
				foreach (FileInfo fileInfo in new DirectoryInfo(text).GetFiles("*.json", SearchOption.AllDirectories))
				{
					string text2 = File.ReadAllText(Path.Combine(text, fileInfo.Name));
					if (!string.IsNullOrEmpty(text2))
					{
						try
						{
							ModBoosterpack modBoosterpack = JsonConvert.DeserializeObject<ModBoosterpack>(text2);
							Debug.Log("loading modded boosterpack: " + modBoosterpack.Id);
							BoosterpackData boosterpackData = this.LoadModBooster(modBoosterpack, mod);
							list.Add(boosterpackData);
						}
						catch (Exception ex)
						{
							Debug.LogError("Failed to load pack: " + ex.Message);
						}
					}
				}
			}
		}
		return list;
	}

	public BoosterpackData LoadModBooster(ModBoosterpack mp, Mod mod)
	{
		BoosterpackData boosterpackData = ScriptableObject.CreateInstance<BoosterpackData>();
		boosterpackData.hideFlags = HideFlags.HideAndDontSave;
		boosterpackData.name = "modbooster";
		boosterpackData.BoosterId = mp.Id;
		boosterpackData.NameTerm = mp.NameTerm ?? "";
		boosterpackData.nameOverride = mp.NameOverride;
		boosterpackData.MinAchievementCount = mp.MinQuestCount;
		boosterpackData.Cost = mp.Cost;
		boosterpackData.BoosterLocation = EnumHelper.ParseEnum<Location>(mp.Location, new int?(0));
		if (!string.IsNullOrEmpty(mp.Icon) && !mp.Icon.EndsWith(".png"))
		{
			BoosterpackData boosterpackData2 = boosterpackData;
			BoosterpackData boosterpackData3 = this.BoosterpackDatas.Find((BoosterpackData c) => c.BoosterId == mp.Icon);
			boosterpackData2.Icon = ((boosterpackData3 != null) ? boosterpackData3.Icon : null);
			if (boosterpackData.Icon == null)
			{
				Debug.LogWarning(string.Concat(new string[] { "Tried to find vanilla booster ", mp.Icon, " for icon, but it does not exist! Did you mean ", mp.Icon, ".png?" }));
			}
		}
		else
		{
			string text = Path.Combine(mod.Path, "Icons", mp.Icon ?? (mp.Id + ".png"));
			if (File.Exists(text))
			{
				boosterpackData.Icon = ResourceHelper.LoadSpriteFromPath(text);
			}
			else
			{
				Debug.LogWarning("Missing or invalid Icon for " + boosterpackData.BoosterId + "!");
			}
		}
		boosterpackData.CardBags = new List<CardBag>();
		foreach (ModCardBag modCardBag in mp.CardBags)
		{
			CardBag cardBag = new CardBag();
			cardBag.CardBagType = EnumHelper.ParseEnum<CardBagType>(modCardBag.CardBagType, new int?(0));
			cardBag.CardsInPack = modCardBag.CardsInPack;
			if (cardBag.CardBagType == CardBagType.Chances)
			{
				cardBag.Chances = modCardBag.Chances;
			}
			if (cardBag.CardBagType == CardBagType.SetPack)
			{
				cardBag.SetPackCards = modCardBag.SetPackCards;
			}
			if (cardBag.CardBagType == CardBagType.SetCardBag)
			{
				cardBag.SetCardBag = EnumHelper.ParseEnum<SetCardBagType>(modCardBag.SetCardBag, new int?(0));
				cardBag.UseFallbackBag = modCardBag.UseFallbackBag;
				if (cardBag.UseFallbackBag)
				{
					cardBag.FallbackBag = EnumHelper.ParseEnum<SetCardBagType>(modCardBag.FallbackBag, new int?(0));
				}
			}
			if (cardBag.CardBagType == CardBagType.Enemies)
			{
				cardBag.EnemyCardBag = EnumHelper.ParseEnum<EnemySetCardBag>(modCardBag.EnemyCardBag, new int?(0));
				cardBag.StrengthLevel = modCardBag.StrengthLevel;
			}
			boosterpackData.CardBags.Add(cardBag);
		}
		return boosterpackData;
	}

	public CardData GetCardFromId(string cardId, bool throwError = true)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		if (cardId == "any_villager_old")
		{
			return this.idToCard["old_villager"];
		}
		if (cardId == "any_villager_young")
		{
			return this.idToCard["teenage_villager"];
		}
		if (cardId == "any_villager" || cardId == "breedable_villager")
		{
			return this.idToCard["villager"];
		}
		if (cardId == "any_worker")
		{
			return this.idToCard["worker"];
		}
		if (cardId == "any_educated_worker")
		{
			return this.idToCard["educated_worker"];
		}
		CardData cardData;
		if (this.idToCard.TryGetValue(cardId, out cardData))
		{
			return cardData;
		}
		if (throwError)
		{
			Debug.LogError("Could not find card with id '" + cardId + "'");
		}
		return null;
	}

	public void AddCardToSetCardBag(SetCardBagType bagType, string cardId, int chance)
	{
		SetCardBagData setCardBagData = this.SetCardBags.Find((SetCardBagData s) => s.SetCardBagType == bagType);
		if (setCardBagData == null)
		{
			throw new Exception(string.Format("No matching card bag found for {0}", bagType));
		}
		setCardBagData.Chances.Add(new SimpleCardChance(cardId, chance));
	}

	public SetCardBagType GetSetCardBagForEnemyCardBag(EnemySetCardBag bag)
	{
		string text = bag.ToString();
		string[] names = Enum.GetNames(typeof(SetCardBagType));
		SetCardBagType[] array = (SetCardBagType[])Enum.GetValues(typeof(SetCardBagType));
		int num = Array.IndexOf<string>(names, text);
		if (num == -1)
		{
			throw new Exception(string.Format("No matching card bag found for {0}", bag));
		}
		return array[num];
	}

	public List<SetCardBagType> GetSetCardBagForEnemyCardBagList(List<EnemySetCardBag> bags)
	{
		List<SetCardBagType> list = new List<SetCardBagType>();
		foreach (EnemySetCardBag enemySetCardBag in bags)
		{
			string text = enemySetCardBag.ToString();
			string[] names = Enum.GetNames(typeof(SetCardBagType));
			SetCardBagType[] array = (SetCardBagType[])Enum.GetValues(typeof(SetCardBagType));
			int num = Array.IndexOf<string>(names, text);
			if (num == -1)
			{
				throw new Exception(string.Format("No matching card bag found for {0}", enemySetCardBag));
			}
			list.Add(array[num]);
		}
		return list;
	}

	public BoosterpackData GetBoosterData(string boosterId)
	{
		BoosterpackData boosterpackData;
		if (!this.idToBooster.TryGetValue(boosterId, out boosterpackData))
		{
			return null;
		}
		return boosterpackData;
	}

	private void AddReference(ICardReference reference)
	{
		string key = reference.GetKey();
		if (this.cardReferenceKeys.Contains(key))
		{
			return;
		}
		this.cardReferences.Add(reference);
		this.cardReferenceKeys.Add(key);
	}

	public List<ICardReference> DetermineCardReferences()
	{
		if (this.cardReferences != null)
		{
			return this.cardReferences;
		}
		this.cardReferences = new List<ICardReference>();
		this.cardReferenceKeys = new HashSet<string>();
		foreach (string text in GameDataLoader.codeReferencedCards)
		{
			this.AddReference(new CardReferenceCode(text));
		}
		foreach (CardData cardData in this.CardDataPrefabs)
		{
			Equipable equipable = cardData as Equipable;
			if (equipable != null)
			{
				this.AddReference(new CardReferenceCard(equipable.VillagerTypeOverride, cardData.Id));
			}
			Combatable combatable = cardData as Combatable;
			if (combatable != null)
			{
				foreach (Equipable equipable2 in combatable.PossibleEquipables)
				{
					this.AddReference(new CardReferenceCard(equipable2.Id, cardData.Id));
					if (equipable2.blueprint != null)
					{
						this.AddReference(new CardReferenceCard(equipable2.blueprint.Id, cardData.Id));
					}
				}
			}
			foreach (string text2 in this.GetCardsReferencedByCardBags(cardData))
			{
				this.AddReference(new CardReferenceCard(text2, cardData.Id));
			}
		}
		foreach (BoosterpackData boosterpackData in this.BoosterpackDatas)
		{
			foreach (CardBag cardBag in boosterpackData.CardBags)
			{
				foreach (string text3 in cardBag.GetCardsInBag(this))
				{
					this.AddReference(new CardReferenceBooster(text3, boosterpackData.BoosterId));
				}
			}
		}
		foreach (EnemySetCardBag enemySetCardBag in GameDataLoader.codeReferencedEnemySetCardBags)
		{
			foreach (CardChance cardChance in CardBag.GetChancesForSetCardBag(this, this.GetSetCardBagForEnemyCardBag(enemySetCardBag), null))
			{
				this.AddReference(new CardReferenceCode(cardChance.Id));
			}
		}
		foreach (Blueprint blueprint in this.BlueprintPrefabs)
		{
			if (!(blueprint is BlueprintGrowth))
			{
				foreach (Subprint subprint in blueprint.Subprints)
				{
					foreach (string text4 in subprint.RequiredCards.Distinct<string>())
					{
						if (text4 == "any_villager" || text4 == "breedable_villager" || text4 == "any_villager_old" || text4 == "any_villager_young")
						{
							text4 = "villager";
						}
						else if (text4.Split('|', StringSplitOptions.None).Length != 0)
						{
							text4 = text4.Split('|', StringSplitOptions.None)[0];
						}
						text4 == "villager";
					}
					this.AddReference(new CardReferenceCard(subprint.ResultCard, blueprint.Id));
					foreach (string text5 in subprint.ExtraResultCards)
					{
						this.AddReference(new CardReferenceCard(text5, blueprint.Id));
					}
				}
			}
		}
		return this.cardReferences;
	}

	private List<string> GetCardsReferencedByCardBags(CardData card)
	{
		List<string> list = new List<string>();
		foreach (CardBag cardBag in card.GetCardBags())
		{
			list.AddRange(cardBag.GetCardsInBag(this));
		}
		return list;
	}

	public ScriptableCutscene GetCutsceneWithId(string id)
	{
		return this.Cutscenes.FirstOrDefault<ScriptableCutscene>((ScriptableCutscene x) => x.CutsceneId == id);
	}

	public static GameDataLoader instance;

	public List<BoosterpackData> BoosterpackDatas;

	public List<CardData> CardDataPrefabs;

	public List<Blueprint> BlueprintPrefabs;

	public List<Demand> Demands;

	public List<ScriptableCutscene> Cutscenes;

	private Dictionary<string, CardData> idToCard = new Dictionary<string, CardData>();

	private Dictionary<string, BoosterpackData> idToBooster = new Dictionary<string, BoosterpackData>();

	public List<SetCardBagData> SetCardBags = new List<SetCardBagData>();

	public string[] VillagerNames;

	private static string[] codeReferencedCards;

	private static EnemySetCardBag[] codeReferencedEnemySetCardBags = new EnemySetCardBag[]
	{
		EnemySetCardBag.BasicEnemy,
		EnemySetCardBag.Forest_AdvancedEnemy,
		EnemySetCardBag.Forest_BasicEnemy
	};

	private List<ICardReference> cardReferences;

	private HashSet<string> cardReferenceKeys;

	public bool SpiritDlcLoaded;

	public bool CitiesDlcLoaded;

	public ProfanityChecker ProfanityChecker;

	internal Mod CurrentlyLoadingMod;
}
