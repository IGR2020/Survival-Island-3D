using UnityEngine;

public class ReturnToMainMenu : MonoBehaviour
{
    public void OnClick()
    {
        SceneLoader.Load(SceneLoader.SceneName.MainMenu);
    }
}
