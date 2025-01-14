using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public void OnClose()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
