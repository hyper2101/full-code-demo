using System;
using System.Collections.Generic;
using UnityEngine;

public class GetComponentCacher<T> where T : MonoBehaviour
{
	public T GetComponent(GameObject go)
	{
		int instanceID = go.GetInstanceID();
		T t;
		if (!this.gameObjectToComponent.TryGetValue(instanceID, out t))
		{
			T component = go.GetComponent<T>();
			this.gameObjectToComponent[instanceID] = component;
			return component;
		}
		return t;
	}

	private Dictionary<int, T> gameObjectToComponent = new Dictionary<int, T>();
}
