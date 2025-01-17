using UnityEngine;

public class Storage : MonoBehaviour
{
	public int storageCount;
    public GameObject slotUiContainerPreset;
    UiHandler uiHandler;
	Slot slots;

	private void Start()
	{
		uiHandler = GetComponent<UiHandler>();
	}

	public void SlotClick(int index)
    {

    }
}
