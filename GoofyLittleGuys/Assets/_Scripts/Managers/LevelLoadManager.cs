using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class LevelLoadManager : SingletonBase<LevelLoadManager>
	{
		public static LevelLoadManager _Instance;
		[SerializeField] private GameObject loadingScreen;
		[SerializeField] private bool isLoadingLevel = false;
		[SerializeField] private List<string> currentLevelList;	// The list of scenes currently loaded in (except the persistent scene)

		private WaitForSeconds bufferTime;

		private void Start()
		{
			LoadNewLevel("00_MainMenu");
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
		}

		/// <summary>
		/// Helper method that loads the given scene in
		/// </summary>
		/// <param name="levelName">The name of the scene we should load</param>
		public void LoadNewLevel(string levelName)
		{
			StartCoroutine(LoadLevel(levelName));
		}

		/// <summary>
		/// Coroutine that loads the scene of the given scene name
		/// </summary>
		/// <param name="levelName">The name of the scene we are loading</param>
		/// <returns></returns>
		private IEnumerator LoadLevel(string levelName)
		{
			isLoadingLevel = true;
			yield return bufferTime;

			// Unload all opened scenes (Not the persistent scene
			if (currentLevelList.Count != 0)
			{
				for (int i = 0; i < currentLevelList.Count; i++)
					SceneManager.UnloadSceneAsync(currentLevelList[i]);
				currentLevelList.Clear();
			}

			// Load new scene
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
			while (!asyncLoad.isDone)
			{
				// If we have a loading screen, we can update the slider progress, and pass asyncLoad.progress
				yield return null;
			}
			//SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelName));

			currentLevelList.Add(levelName);
			yield return bufferTime;

			//loadingScreen.SetActive(false);
			isLoadingLevel = false;

			yield break;
		}
	}
}
