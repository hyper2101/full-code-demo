using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
	public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
	{
		if (sequence == null)
		{
			yield break;
		}
		List<T> list = sequence.ToList<T>();
		if (!list.Any<T>())
		{
			yield return Enumerable.Empty<T>();
		}
		else
		{
			int startingElementIndex = 0;
			foreach (T startingElement in list)
			{
				int index = startingElementIndex;
				IEnumerable<T> enumerable = list.Where<T>((T e, int i) => i != index);
				foreach (IEnumerable<T> enumerable2 in enumerable.Permute<T>())
				{
					yield return enumerable2.Prepend(startingElement);
				}
				IEnumerator<IEnumerable<T>> enumerator2 = null;
				int num = startingElementIndex;
				startingElementIndex = num + 1;
				startingElement = default(T);
			}
			List<T>.Enumerator enumerator = default(List<T>.Enumerator);
		}
		yield break;
		yield break;
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int i = list.Count;
		while (i > 1)
		{
			i--;
			int num = Random.Range(0, i + 1);
			T t = list[num];
			list[num] = list[i];
			list[i] = t;
		}
	}

	public static void SetParentClean(this Transform t, Transform parent)
	{
		t.SetParent(parent);
		t.localScale = Vector3.one;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
	}

	public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
	{
		Bounds bounds = default(Bounds);
		if (source != null)
		{
			source.GetWorldCorners(Extensions.corners);
			Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Matrix4x4 worldToLocalMatrix = target.worldToLocalMatrix;
			for (int i = 0; i < 4; i++)
			{
				Vector3 vector3 = worldToLocalMatrix.MultiplyPoint3x4(Extensions.corners[i]);
				vector = Vector3.Min(vector3, vector);
				vector2 = Vector3.Max(vector3, vector2);
			}
			bounds = new Bounds(vector, Vector3.zero);
			bounds.Encapsulate(vector2);
		}
		return bounds;
	}

	public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = ((viewport != null) ? viewport : scrollRect.GetComponent<RectTransform>());
		Bounds bounds = new Bounds(rectTransform.rect.center, rectTransform.rect.size);
		RectTransform content = scrollRect.content;
		float num = ((content != null) ? content.TransformBoundsTo(rectTransform) : default(Bounds)).size[axis] - bounds.size[axis];
		return distance / num;
	}

	public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target, bool clampVerticalPos = true)
	{
		RectTransform rectTransform = ((scrollRect.viewport != null) ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		float num = rect.center.y - bounds.center.y;
		float num2 = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, num);
		if (clampVerticalPos)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Clamp(num2, 0f, 1f);
			return;
		}
		scrollRect.verticalNormalizedPosition = num2;
	}

	public static LocParam LocParam_Action(string actionName)
	{
		return LocParam.Create("action_" + actionName, InputController.instance.GetActionDisplayString(actionName));
	}

	public static string TranslateEnum<T>(this T value) where T : struct, IConvertible
	{
		return MewtationsLoc.Translate(string.Format("{0}_{1}", value.GetType().ToString().ToLower(), value));
	}

	public static List<T> AsList<T>(this T value)
	{
		return new List<T> { value };
	}

	public static T Choose<T>(this List<T> list)
	{
		return list[Random.Range(0, list.Count)];
	}

	public static T Choose<T>(this T[] arr)
	{
		return arr[Random.Range(0, arr.Length)];
	}

	public static Vector3 Perlin(float t)
	{
		return new Vector3(Mathf.PerlinNoise(t, 0f), 0f, Mathf.PerlinNoise(t, 0.5f));
	}

	public static Vector3 PerlinNormalized(float t)
	{
		return new Vector3(Extensions.PerlinNoise2(t, 0f), 0f, Extensions.PerlinNoise2(t, 0.5f));
	}

	private static float PerlinNoise2(float x, float y)
	{
		return Mathf.PerlinNoise(x, y) * 2f - 1f;
	}

	public static List<string> FindMatches(string str, string[] arr)
	{
		List<string> list = new List<string>();
		foreach (string text in arr)
		{
			if (text.Contains(str))
			{
				list.Add(text);
			}
		}
		return list;
	}

	private static Vector3[] corners = new Vector3[4];
}
