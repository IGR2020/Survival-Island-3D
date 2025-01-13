using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiHandler : MonoBehaviour
{
	public Slider healthBar;
	public Slider hungerBar;
	public List<Slot> slots;
	public GameObject activeSlot = null;

	float slotY;
	public int lift;

	public Player player;
	public ItemTexture[] itemTextures;

	int activeSlotIndex;

	private void Start()
	{
		slotY = slots[0].GetComponent<RectTransform>().position.y;

		activeSlotIndex = 0;
		activeSlot = slots[activeSlotIndex].gameObject;
		LiftSlot(0);

	}

	private void Update()
	{
		healthBar.value = Mathf.Clamp01(player.health / player.maxHealth);
		hungerBar.value = Mathf.Clamp01(player.hunger / player.maxHunger);
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
public struct ItemTexture
{
	public string name;
	public Texture2D texture;
}
