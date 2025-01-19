using UnityEngine;
using UnityEngine.SceneManagement;
public static class SceneLoader 
{
	public enum SceneName {IslandSurvival, MainMenu, LoadingScene, None}
	public static SceneName sceneToLoad = SceneName.None;
	public static Scene lastScene;
	public static void Load(SceneName sceneName)
	{
		if (sceneName == SceneName.None) { return; }
		lastScene = SceneManager.GetActiveScene();
		sceneToLoad = sceneName;
		SceneManager.LoadScene(SceneName.LoadingScene.ToString());
	}

	public static void FinishLoad()
	{
		if (sceneToLoad == SceneName.None) { return; }
		SceneManager.LoadScene(sceneToLoad.ToString());
	}

	public static void Callback()
	{
		FinishLoad();
	}
}
