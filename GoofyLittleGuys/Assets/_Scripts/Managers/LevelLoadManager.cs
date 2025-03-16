using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class LevelLoadManager : SingletonBase<LevelLoadManager>
	{
		public static LevelLoadManager _Instance;
		[SerializeField] private LoadingScreen loadingScreen;
		[SerializeField] private bool isLoadingLevel = false;
		[SerializeField] private List<string> currentLevelList; // The list of scenes currently loaded in (except the persistent scene)

		private float bufferTime = 1.5f;
		private bool firstLoad = true; // Track if it's the first load

		private void Start()
		{
			LoadNewLevel("00_MainMenu");
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
		}

		/// <summary>
		/// Loads a new level.
		/// If it's the first time loading, we skip the loading screen and load instantly.
		/// </summary>
		public void LoadNewLevel(string levelName)
		{
			if (firstLoad)
			{
				// First-time load -> No loading screen, no delay, just load the scene instantly
				firstLoad = false; // Ensure future loads go through the normal sequence
				InstantLoadLevel(levelName);
			}
			else
			{
				// Use normal loading process with the loading screen
				StartCoroutine(LoadLevel(levelName));
			}
		}

		/// <summary>
		/// Loads the scene instantly without a loading screen or delay.
		/// </summary>
		private void InstantLoadLevel(string levelName)
		{
			// Unload previous scenes
			if (currentLevelList.Count != 0)
			{
				for (int i = 0; i < currentLevelList.Count; i++)
					SceneManager.UnloadSceneAsync(currentLevelList[i]);
				currentLevelList.Clear();
			}

			// Load new scene immediately
			SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
			currentLevelList.Add(levelName);
		}

		/// <summary>
		/// Coroutine that loads the scene with a loading screen and delays.
		/// </summary>
		private IEnumerator LoadLevel(string levelName)
		{
			isLoadingLevel = true;
			loadingScreen.gameObject.SetActive(true);

			float fakeProgress = 0f; // Fake progress value to control visual loading speed

			if (levelName == "TerrainWhitebox")
			{
				yield return new WaitForSecondsRealtime(bufferTime * 2);
			}
			else
			{
				yield return new WaitForSecondsRealtime(bufferTime);
			}

			// Unload previous scenes
			if (currentLevelList.Count != 0)
			{
				for (int i = 0; i < currentLevelList.Count; i++)
					SceneManager.UnloadSceneAsync(currentLevelList[i]);
				currentLevelList.Clear();
			}

			// Start async scene loading
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
			asyncLoad.allowSceneActivation = false; // Prevent scene from loading instantly

			while (!asyncLoad.isDone)
			{
				// Gradually increase fake progress until it matches actual progress
				while (fakeProgress < asyncLoad.progress)
				{
					fakeProgress = Mathf.MoveTowards(fakeProgress, asyncLoad.progress, Time.unscaledDeltaTime * 0.5f); // Adjust speed here
					loadingScreen.LoadingBarOption2.value = fakeProgress;
					loadingScreen.LoadProgress.text = (fakeProgress * 100f).ToString("0") + "%";

					if (fakeProgress >= 0.5f)
						loadingScreen.LoadingBackground.sprite = loadingScreen.LoadProgressBGs[1];
					else
						loadingScreen.LoadingBackground.sprite = loadingScreen.LoadProgressBGs[0];

					yield return null;
				}

				// If scene is fully loaded but we haven't activated it yet, wait a bit more
				if (asyncLoad.progress >= 0.9f)
				{
					yield return new WaitForSecondsRealtime(levelName == "TerrainWhitebox" ? 2.5f : 1.5f); // Artificial delay to let bar reach 100% 
					asyncLoad.allowSceneActivation = true; // Finally activate the scene
				}

				yield return null;
			}

			currentLevelList.Add(levelName);

			loadingScreen.gameObject.SetActive(false);
			loadingScreen.LoadingBarOption2.value = 0;
			loadingScreen.LoadProgress.text = "0%";
			isLoadingLevel = false;
		}
	}
}
