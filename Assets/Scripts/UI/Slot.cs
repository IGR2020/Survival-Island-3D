using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Slot : MonoBehaviour
{
	public int index;
    public Item item;
	public Image itemImage;
	TMP_Text count;

	public enum SendMessageTo {UiHandler, InventoryDisplay};
	public SendMessageTo messageTo;
	UiHandler uiHandler;
	InventoryDisplay inventoryDisplay;

	private void Start()
	{
		uiHandler = FindFirstObjectByType<UiHandler>();
		inventoryDisplay = FindFirstObjectByType<InventoryDisplay>();
		count = GetComponentInChildren<TMP_Text>();
		print("Text object found as " + count.gameObject.name);
		UpdateImage();
	}

	public void SetItem(Item item) 
	{
		this.item = item;
		UpdateImage();
	}

	public void UpdateImage()
	{
		count.text = (item.count < 1) ? "" : item.count.ToString();

		itemImage.sprite = uiHandler.FindItemSprite(item.name);
	}

	public void IsPressed()
	{
        if (messageTo == SendMessageTo.InventoryDisplay)
        {
			inventoryDisplay.OnSlotClicked(index);
        }
		else
		{
			uiHandler.SlotPressed(index);
		}
	}

	public static Item SetEmptyIf0(Item item)
	{
		if (item.count < 1)
		{
			item.count = 0;
			item.name = "Empty";
			item.tags = new List<string>();
			item.tagData = new List<float>();
		}
		return item;
	}

	// Returns swaped item
	public Item SwapItems(Item other)
	{
		Item temp = other;
		other = item;
		item = temp;
		return other;
	}
}
