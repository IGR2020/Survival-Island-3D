using UnityEngine;

public class RespawnButton : MonoBehaviour
{
    public GameObject otherSceneContainer;

    public void OnClick()
    {
        otherSceneContainer.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }
}
