using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Slot : MonoBehaviour
{
	public int index;
    public Item item;
	public RawImage itemImage;
	TMP_Text count;

	UiHandler uiHandler;

	private void Start()
	{
		uiHandler = FindFirstObjectByType<UiHandler>();
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
		Texture2D texture = null;
		foreach (ItemTexture itemTexture in uiHandler.itemTextures)
		{
			if (itemTexture.name == item.name)
			{
				texture = itemTexture.texture;
				break;
			}
		}

		if (texture == null) { print("Image not found"); return; }

		itemImage.texture = texture;
	}

	public void IsPressed()
	{
		uiHandler.SlotPressed(index);
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
}