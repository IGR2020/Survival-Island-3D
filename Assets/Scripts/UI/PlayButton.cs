using UnityEngine;

public class PlayButton : MonoBehaviour
{
    public SceneLoader.SceneName sceneName;

	public void OnClick()
    {
        SceneLoader.Load(sceneName);
    }
}
