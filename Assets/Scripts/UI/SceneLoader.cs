using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader 
{
	public enum SceneName {IslandSurvival, MainMenu, LoadingScene, DeathScreen, None}
	public static SceneName sceneToLoad = SceneName.None;
	public static void Load(SceneName sceneName)
	{
		sceneToLoad = sceneName;
		SceneManager.LoadScene(SceneName.LoadingScene.ToString());
	}

	public static void Load()
	{
		if (sceneToLoad == SceneName.None) { return; }
		SceneManager.LoadScene(sceneToLoad.ToString());
	}

	public static void Callback()
	{
		Load();
	}
}
