using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneBootstrapper : MonoBehaviour
{
	private void Awake()
	{
		GameObject gameObject = new GameObject("Boards");
		foreach (GameBoard gameBoard in this.Boards)
		{
			GameBoard gameBoard2 = Object.Instantiate<GameBoard>(gameBoard);
			gameBoard2.transform.SetParent(gameObject.transform, true);
			gameBoard2.gameObject.name = gameBoard.gameObject.name;
		}
		GameObject gameObject2 = new GameObject("Managers");
		foreach (GameObject gameObject3 in this.ObjectsToInstantiate)
		{
			GameObject gameObject4;
			try
			{
				gameObject4 = Object.Instantiate<GameObject>(gameObject3);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception during scene bootstrapping:");
				Debug.LogException(ex);
				continue;
			}
			if (gameObject3.name.Contains("Manager") || gameObject3.name.Contains("Controller"))
			{
				gameObject4.transform.SetParent(gameObject2.transform, true);
			}
			gameObject4.name = gameObject3.name;
		}
		if (PlatformHelper.HasModdingSupport)
		{
			ModManager.instance.ReadyUpMods();
		}
	}

	public List<GameBoard> Boards;

	public List<GameObject> ObjectsToInstantiate;
}
