using UnityEngine;


public class ItemStorage : MonoBehaviour
{
	public int storageCount;
	UiHandler uiHandler;
	Item[] items;

	private void Start()
	{
		uiHandler = FindFirstObjectByType<UiHandler>();
		items = new Item[storageCount];
		for (int i = 0; i < storageCount; i++) 
		{
			items[i].name = "Empty";
		}
	}

	public void Activate()
	{
		uiHandler.inventoryUi.gameObject.SetActive(true);
		uiHandler.inventoryUi.onClickCallback = OnSlotClick;
		
		uiHandler.inventoryUi.SetSlotCount(storageCount);
		uiHandler.inventoryUi.SetSlotItems(items);
		uiHandler.inventoryUi.UpdateSlotImages();
	}

	public void OnSlotClick()
	{
		Slot activeSlot = uiHandler.inventoryUi.GetActiveSlot();
		Item heldItem = uiHandler.GetHeldItem();

		heldItem = activeSlot.SwapItems(heldItem);
		uiHandler.SetHeldItem(heldItem);
		uiHandler.inventoryUi.SetActiveItem(activeSlot.item);

		SyncItems();
		uiHandler.inventoryUi.UpdateSlotImages();
		uiHandler.UpdateSlotImages();
	}

	public void SyncItems()
	{
		for (int i = 0; i < storageCount;i++)
		{
			items[i] = uiHandler.inventoryUi.slots[i].item;
		}
	}
}