using UnityEngine;

public class RespawnButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneLoader.Load(SceneLoader.SceneName.IslandSurvival);
    }
}
