using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
	private void Awake()
	{
		DrawManager.instance = this;
		foreach (DrawManager.ShapeDrawerPrefab shapeDrawerPrefab in this.Prefabs)
		{
			for (int i = 0; i < shapeDrawerPrefab.Count; i++)
			{
				if (!this.shapeObjectPools.ContainsKey(shapeDrawerPrefab.Prefab.DrawingType))
				{
					this.shapeObjectPools.Add(shapeDrawerPrefab.Prefab.DrawingType, new List<ShapeDrawer>());
				}
				this.MakeShapeObject(shapeDrawerPrefab.Prefab);
			}
		}
	}

	private ShapeDrawer GetPrefabFromShape(IShape shape)
	{
		if (shape == null)
		{
			return null;
		}
		DrawManager.ShapeDrawerPrefab shapeDrawerPrefab = this.Prefabs.Find((DrawManager.ShapeDrawerPrefab x) => x.Prefab.DrawingType == shape.GetType());
		if (shapeDrawerPrefab == null)
		{
			return null;
		}
		return shapeDrawerPrefab.Prefab;
	}

	private ShapeDrawer MakeShapeObject(ShapeDrawer prefab)
	{
		ShapeDrawer shapeDrawer = Object.Instantiate<ShapeDrawer>(prefab);
		shapeDrawer.transform.SetParentClean(base.transform);
		shapeDrawer.gameObject.SetActive(false);
		this.shapeObjectPools[shapeDrawer.DrawingType].Add(shapeDrawer);
		return shapeDrawer;
	}

	private void Update()
	{
		this.ShapesToDraw.Clear();
		foreach (ShapeDrawer shapeDrawer in this.takenShapeDrawers)
		{
			shapeDrawer.gameObject.SetActive(false);
			this.shapeObjectPools[shapeDrawer.DrawingType].Insert(0, shapeDrawer);
		}
		this.takenShapeDrawers.Clear();
	}

	private ShapeDrawer GetShapeDrawerForShape(IShape shape)
	{
		List<ShapeDrawer> list = this.shapeObjectPools[shape.GetType()];
		ShapeDrawer shapeDrawer = null;
		if (list.Count > 0)
		{
			shapeDrawer = list[0];
		}
		else
		{
			ShapeDrawer prefabFromShape = this.GetPrefabFromShape(shape);
			if (prefabFromShape != null)
			{
				shapeDrawer = this.MakeShapeObject(prefabFromShape);
			}
		}
		this.takenShapeDrawers.Add(shapeDrawer);
		list.Remove(shapeDrawer);
		return shapeDrawer;
	}

	private void LateUpdate()
	{
		this.ShapesToDrawCount = this.ShapesToDraw.Count;
		foreach (IShape shape in this.ShapesToDraw)
		{
			ShapeDrawer shapeDrawerForShape = this.GetShapeDrawerForShape(shape);
			if (shapeDrawerForShape == null)
			{
				Debug.LogError(string.Format("ShapeDrawer pool is empty, could not draw {0}!", shape.GetType()));
			}
			else
			{
				shapeDrawerForShape.gameObject.SetActive(true);
				shapeDrawerForShape.MyShape = shape;
				shapeDrawerForShape.UpdateShape();
			}
		}
	}

	public void DrawShape(IShape shape)
	{
		this.ShapesToDraw.Add(shape);
	}

	public static DrawManager instance;

	public List<DrawManager.ShapeDrawerPrefab> Prefabs;

	public List<IShape> ShapesToDraw = new List<IShape>();

	private List<ShapeDrawer> takenShapeDrawers = new List<ShapeDrawer>();

	private Dictionary<Type, List<ShapeDrawer>> shapeObjectPools = new Dictionary<Type, List<ShapeDrawer>>();

	public int ShapesToDrawCount = -1;

	[Serializable]
	public class ShapeDrawerPrefab
	{
		public ShapeDrawer Prefab;

		public int Count;
	}
}
