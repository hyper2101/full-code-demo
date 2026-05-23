using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectReservation
{
	public string CardUniqueId { get; }
	public string TargetStackUniqueId { get; }
	public float TimeRemaining { get; set; }

	public CollectReservation(string cardUniqueId, string targetStackUniqueId, float timeout = 3.0f)
	{
		CardUniqueId = cardUniqueId;
		TargetStackUniqueId = targetStackUniqueId;
		TimeRemaining = timeout;
	}
}

public class RelicAutomationSystem : MonoBehaviour
{
	public static RelicAutomationSystem Instance { get; private set; }

	private readonly Dictionary<RelicEffectType, int> _activeRelicEffects = new Dictionary<RelicEffectType, int>();
	private readonly Dictionary<RelicEffectType, IRelicEffect> _relicStrategies = new Dictionary<RelicEffectType, IRelicEffect>();

	// Quản lý Reservation để tránh tranh chấp stack gộp thẻ (Target Contention)
	private readonly Dictionary<string, CollectReservation> _reservations = new Dictionary<string, CollectReservation>();

	private float _relicActionTimer = 0f;
	private float _revalidationTimer = 0f;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// Đăng ký các chiến lược tự động hóa cổ vật (Strategy Pattern)
		_relicStrategies[RelicEffectType.AutoFarm] = new AutoFarmRelicEffect();
		_relicStrategies[RelicEffectType.AutoHeal] = new AutoHealRelicEffect();
		_relicStrategies[RelicEffectType.AutoCollect] = new AutoCollectRelicEffect();
	}

	private void OnEnable()
	{
		// Subscribe đối xứng qua Event Bus để tránh ghost callback
		EventBus.Subscribe<OnShrineStackChangedEvent>(OnShrineStackChanged);
	}

	private void OnDisable()
	{
		// Unsubscribe đối xứng tuyệt đối chống memory leak
		EventBus.Unsubscribe<OnShrineStackChangedEvent>(OnShrineStackChanged);
	}

	private void OnShrineStackChanged(OnShrineStackChangedEvent ev)
	{
		// Phát tín hiệu rebuild lại danh sách cổ vật hoạt động lập tức
		RebuildRelicRegistry();
	}

	public void RebuildRelicRegistry()
	{
		_activeRelicEffects.Clear();

		if (WorldManager.instance == null || WorldManager.instance.AllCards == null) return;

		foreach (GameCard gc in WorldManager.instance.AllCards)
		{
			if (gc != null && gc.CardData is ShrineCardData shrine && !gc.Destroyed)
			{
				GameCard curr = gc.Child;
				while (curr != null)
				{
					if (curr.CardData != null && !curr.Destroyed && curr.CardData.IsAncientRelic)
					{
						string rid = curr.CardData.Id.ToLower();
						RelicEffectType? effect = null;

						if (rid == "item_ancient_relic_auto_farm") effect = RelicEffectType.AutoFarm;
						else if (rid == "item_ancient_relic_auto_heal") effect = RelicEffectType.AutoHeal;
						else if (rid == "item_ancient_relic_auto_collect") effect = RelicEffectType.AutoCollect;

						if (effect.HasValue)
						{
							if (!_activeRelicEffects.ContainsKey(effect.Value))
							{
								_activeRelicEffects[effect.Value] = 0;
							}
							_activeRelicEffects[effect.Value]++; // Cộng dồn số stack cổ vật!
						}
					}
					curr = curr.Child;
				}
			}
		}
	}

	private void Update()
	{
		if (WorldManager.instance == null || !WorldManager.instance.IsPlaying || WorldManager.instance.InAnimation) return;

		float dt = Time.deltaTime;

		// 1. Thực thi các cổ vật theo chu kỳ 5 giây bằng Strategy Pattern
		_relicActionTimer += dt;
		if (_relicActionTimer >= 5.0f)
		{
			_relicActionTimer = 0f;
			ExecuteRelicActions();
		}

		// 2. Revalidation liên tục mỗi 0.25 giây chống deadlock
		_revalidationTimer += dt;
		if (_revalidationTimer >= 0.25f)
		{
			_revalidationTimer = 0f;
			RevalidateReservations();
		}

		// 3. Cập nhật đếm ngược Timeout của các gộp thẻ đang bay
		TickReservations(dt);

		// 4. Thực thi Auto Collect (Lerp mượt mà mỗi frame)
		ExecuteAutoCollect();
	}

	private void ExecuteRelicActions()
	{
		foreach (var pair in _activeRelicEffects)
		{
			RelicEffectType effectType = pair.Key;
			int stackCount = pair.Value;

			if (_relicStrategies.TryGetValue(effectType, out var strategy))
			{
				strategy.Execute(WorldManager.instance, stackCount);
			}
		}
	}

	private void TickReservations(float dt)
	{
		List<string> expiredKeys = new List<string>();
		foreach (var pair in _reservations)
		{
			pair.Value.TimeRemaining -= dt;
			if (pair.Value.TimeRemaining <= 0f)
			{
				expiredKeys.Add(pair.Key);
			}
		}

		// Xóa các đăng ký đã quá hạn 3s để giải phóng gộp thẻ (Failsafe)
		foreach (string key in expiredKeys)
		{
			_reservations.Remove(key);
		}
	}

	private void RevalidateReservations()
	{
		if (WorldManager.instance == null) return;

		List<string> invalidKeys = new List<string>();
		foreach (var pair in _reservations)
		{
			string freeCardUid = pair.Key;
			string targetStackUid = pair.Value.TargetStackUniqueId;

			// Kiểm tra xem thẻ tự do và stack đích có còn tồn tại và hợp lệ không
			bool freeValid = WorldManager.instance.UniqueIdToCard.TryGetValue(freeCardUid, out var freeCard) 
			                 && freeCard != null && !freeCard.Destroyed && freeCard.Parent == null && freeCard.Child == null && !freeCard.BeingDragged;
			
			bool targetValid = WorldManager.instance.UniqueIdToCard.TryGetValue(targetStackUid, out var targetCard) 
			                   && targetCard != null && !targetCard.Destroyed;

			if (!freeValid || !targetValid)
			{
				invalidKeys.Add(freeCardUid);
			}
		}

		// Giải phóng và dọn sạch ghost reference
		foreach (string key in invalidKeys)
		{
			_reservations.Remove(key);
		}
	}

	private void ExecuteAutoCollect()
	{
		if (WorldManager.instance == null || !_activeRelicEffects.ContainsKey(RelicEffectType.AutoCollect) || _activeRelicEffects[RelicEffectType.AutoCollect] <= 0) return;

		foreach (GameCard freeCard in WorldManager.instance.AllCards)
		{
			if (freeCard == null || freeCard.CardData == null || freeCard.Destroyed) continue;

			string freeUid = freeCard.CardData.UniqueId;

			// Chỉ gom thẻ tự do hoàn toàn (không có parent, child, không đang bị drag)
			if (freeCard.Parent == null && freeCard.Child == null && !freeCard.BeingDragged)
			{
				if (freeCard.CardData.MyCardType == CardType.Resources || freeCard.CardData.MyCardType == CardType.Food)
				{
					// Kiểm tra xem đã có đăng ký thu gom an toàn chưa
					if (_reservations.TryGetValue(freeUid, out var res))
					{
						// Đang bay tới stack đích đã đăng ký
						if (WorldManager.instance.UniqueIdToCard.TryGetValue(res.TargetStackUniqueId, out var targetStack) && targetStack != null)
						{
							GameCard lastCard = targetStack;
							while (lastCard.Child != null)
							{
								lastCard = lastCard.Child;
							}

							if (lastCard != freeCard)
							{
								// Hút Lerp mượt mà mỗi frame
								freeCard.transform.position = Vector3.Lerp(freeCard.transform.position, lastCard.transform.position + new Vector3(0f, 0.1f, -0.1f), Time.deltaTime * 5f);
								if (Vector3.Distance(freeCard.transform.position, lastCard.transform.position) < 0.25f)
								{
									// Gắn kết nối stack chính thức và xóa đăng ký
									lastCard.SetChild(freeCard);
									freeCard.SetParent(lastCard);
									_reservations.Remove(freeUid);
								}
							}
						}
					}
					else
					{
						// Chưa có đăng ký, tìm kiếm một stack Key X hợp lệ để giữ chỗ
						string targetId = freeCard.CardData.Id;
						GameCard bestStack = null;

						foreach (GameCard otherCard in WorldManager.instance.AllCards)
						{
							if (otherCard != null && otherCard != freeCard && !otherCard.Destroyed && otherCard.Parent == null)
							{
								// Kiểm tra xem stack này có chứa thẻ cùng loại bên dưới không
								GameCard childSearch = otherCard;
								bool hasMatch = false;
								int stackCount = 0;
								while (childSearch != null)
								{
									stackCount++;
									if (childSearch.CardData != null && childSearch.CardData.Id == targetId)
									{
										hasMatch = true;
									}
									childSearch = childSearch.Child;
								}

								// Cân bằng giới hạn stack tối đa là 15 thẻ để tạo gameplay pressure
								if (hasMatch && stackCount < 15)
								{
									// Đếm xem có bao nhiêu thẻ tự do khác đang đăng ký bay về stack này
									int reservedCount = 0;
									foreach (var r in _reservations.Values)
									{
										if (r.TargetStackUniqueId == otherCard.CardData.UniqueId)
										{
											reservedCount++;
										}
									}

									// Chỉ đăng ký nếu tổng số thẻ trong stack + số thẻ đang bay tới chưa vượt quá 15
									if (stackCount + reservedCount < 15)
									{
										bestStack = otherCard;
										break;
									}
								}
							}
						}

						if (bestStack != null)
						{
							// Đăng ký giữ chỗ thu gom an toàn (Timeout 3 giây cực nhanh)
							_reservations[freeUid] = new CollectReservation(freeUid, bestStack.CardData.UniqueId, 3.0f);
						}
					}
				}
			}
		}
	}

	public void ClearAllReservations()
	{
		_reservations.Clear();
	}
}
