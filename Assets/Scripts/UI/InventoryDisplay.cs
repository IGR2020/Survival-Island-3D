using UnityEngine;
using System.Collections.Generic;

public class InventoryDisplay : MonoBehaviour
{
    public GameObject uiContainer;
    public GameObject slotPreset;
    public List<GameObject> slotUis;
    public List<Slot> slots;
    public int slotCount = 0;
    public int activeSlot = 0;
    public System.Action onClickCallback = null;

    public void SetSlotCount(int count)
    {
        slotCount = count;
        slotUis = Crafter.ClearGameObjectList(slotUis);
        slots.Clear();

		for (int i = 0; i < slotCount; i++) 
        {
            GameObject slotUiObject = Instantiate(slotPreset, uiContainer.transform);
            slotUis.Add(slotUiObject);
            slots.Add(slotUiObject.GetComponent<Slot>());
            slots[i].index = i;
        }
    }

    public void SetSlotItems(Item[] items)
    {
        for (int i = 0; i < items.Length; i++) 
        {
            slots[i].item = items[i];
        }
    }

	public void UpdateSlotImages()
	{
		foreach (Slot slot in slots)
        {
            slot.UpdateImage();
        }
	}

	public void OnSlotClicked(int index)
    {
        activeSlot = index;
        if (onClickCallback == null) return;
        onClickCallback();
    }

    public Slot GetActiveSlot()
    {
        return slots[activeSlot];
    }

    public Item GetActiveItem()
    {
        return slots[activeSlot].item;
    }

    public void SetItem(int index, Item item)
    {
        slots[index].item = item;
    }

    public void SetActiveItem(Item item)
    {
        slots[activeSlot].item = item;
    }
}
