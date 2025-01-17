using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiHandler : MonoBehaviour
{
	public GameObject presetGroundedItem;
	MeshRenderer buildGridRenderer;
	//Assumes grid size is 10x10 (default unity plane size)
	public GameObject buildGrid;
	public bool showBuildGrid;
	public float gridPlacementHeightOffset;
	public Slider healthBar;
	public Slider hungerBar;
	public Slider thirstBar;
	public GameObject craftingUi;
	public GameObject inventoryUi;
	public Crafter craftingHandler;
	public List<Slot> slots;
	public GameObject activeSlot = null;

	float slotY;
	public int lift;

	public Player player;
	public Sprite missingTexture;
	[HideInInspector()]
	public ItemSprite[] itemSprites;

	int activeSlotIndex;

	private void Start()
	{
		Sprite[] sprites = Resources.LoadAll<Sprite>("Items");
		itemSprites = new ItemSprite[sprites.Length];
		for (int i = 0; i < itemSprites.Length; i++)
		{
			itemSprites[i] = new ItemSprite(sprites[i].name, sprites[i]);
		}

		buildGridRenderer = buildGrid.GetComponent<MeshRenderer>();
		craftingUi.SetActive(false);
		craftingHandler = craftingUi.GetComponent<Crafter>();

		inventoryUi.SetActive(false);

		slotY = slots[0].GetComponent<RectTransform>().position.y;

		activeSlotIndex = 0;
		activeSlot = slots[activeSlotIndex].gameObject;
		LiftSlot(0);

		foreach (Slot slot in slots)
		{
			slot.item = Slot.SetEmptyIf0(slot.item);
		}

		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].index = i;
		}

		UpdateSlots();
	}

	public void AllowCrating() 
	{
		craftingUi.SetActive(true);
	}

	public void CraftConfirmed()
	{
		Recipe craftedRecipe = craftingHandler.GetCraftedRecipe();
		player.CraftRecipe(craftedRecipe);
	}

	private void Update()
	{
		healthBar.value = Mathf.Clamp01(player.health / player.maxHealth);
		hungerBar.value = Mathf.Clamp01(player.hunger / player.maxHunger);
		thirstBar.value = Mathf.Clamp01(player.thirst / player.maxThirst);

		player.isUsingGui = craftingUi.activeSelf || inventoryUi.activeSelf;

		if (craftingHandler.craftConfirmed)
		{
			craftingHandler.craftConfirmed = false;
			CraftConfirmed();
		}

		buildGrid.SetActive(player.buildMode && showBuildGrid);
		if (player.buildMode && showBuildGrid)
		{
			buildGrid.transform.position = player.buildPreview.transform.position - 
				Vector3.up * gridPlacementHeightOffset + 
				player.buildPreviewSize.x/2 * Vector3.right + 
				player.buildPreviewSize.z/2 * Vector3.forward;
		}
	}

	public void SetGrid()
	{
		if (!showBuildGrid) { return; }
		Material gridMat = buildGridRenderer.material;
		gridMat.SetVector("_Tiling", new Vector4(player.buildPreviewSize.x*5, player.buildPreviewSize.z*5));
		buildGridRenderer.material = gridMat;
	}

	public Sprite FindItemSprite(string name)
	{
		foreach (ItemSprite item in itemSprites)
		{
			if (item.name == name)
			{
				return item.sprite;
			}
		}

		return missingTexture;
	}

	public void SlotPressed(int index)
	{
		LiftSlot(index);
		activeSlotIndex = index;
		activeSlot = slots[activeSlotIndex].gameObject;
	}

	void LiftSlot(int slotIndex)
	{
		if (activeSlot == null) return;

		RectTransform activeSlotTransform = activeSlot.GetComponent<RectTransform>();
		activeSlotTransform.position = new Vector3(activeSlotTransform.position.x, slotY);

		RectTransform newSlotTransform = slots[slotIndex].gameObject.GetComponent<RectTransform>();
		newSlotTransform.position = new Vector3(newSlotTransform.position.x, slotY + lift);
	}

	public void UpdateSlots()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (i > player.inventory.Count - 1) break;
			print("Updated Slot " + i.ToString());
			slots[i].SetItem(player.inventory[i]);
		}
	}

	public void ForceUpdateAllSlots()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (i > player.inventory.Count - 1) slots[i].SetItem(new Item("Empty", 0));
			else slots[i].SetItem(player.inventory[i]);
		}
	}

	public Item GetHeldItem()
	{
		return slots[activeSlotIndex].item;
	}

	public void SetHeldItem(Item item)
	{
		player.inventory[activeSlotIndex] = item;
		if (player.inventory[activeSlotIndex].name == "Empty") player.inventory.RemoveAt(activeSlotIndex); 	
		ForceUpdateAllSlots();
	}
}

[System.Serializable]
public struct ItemSprite
{
	public string name;
	public Sprite sprite;

	public ItemSprite(string name, Sprite sprite)
	{
		this.name = name;
		this.sprite = sprite;
	}
}
