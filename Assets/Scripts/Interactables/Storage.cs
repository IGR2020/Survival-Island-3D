using UnityEngine;
using System.Collections.Generic;

public class Storage : MonoBehaviour
{
	public int storageCount;
    public InventoryDisplay inventory;
    UiHandler uiHandler;
	Item[] slots;

	private void Start()
	{
		uiHandler = FindFirstObjectByType<UiHandler>();
		inventory = FindAnyObjectByType<InventoryDisplay>();
		slots = new Item[storageCount];
	}

	public void Activate()
	{
		inventory.gameObject.SetActive(true);
		inventory.onClickCallback = OnSlotClick;
		inventory.SetSlotCount(storageCount);
	}

	public void OnSlotClick()
    {
		Slot activeSlot = inventory.GetActiveSlot();
		Item heldItem = uiHandler.GetHeldItem();

		heldItem = activeSlot.SwapItems(heldItem);
		uiHandler.SetHeldItem(heldItem);
		inventory.SetActiveItem(activeSlot.item);

		slots[inventory.activeSlot] = inventory.GetActiveItem();
	}
}
