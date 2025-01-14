using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class StructureData : ScriptableObject 
{
	public StructureDataPoint[] structures;
	public StructureDataPoint[] playerStructures;
	public StructureDataPoint generic;
	public LootTable[] lootTables;
	public ItemData itemData;
	public LootTable FindLootTable(string name)
	{
		for (int i = 0; i < lootTables.Length; i++)
		{
			if (lootTables[i].name == name)
			{
				return lootTables[i];
			}
		}

		return new LootTable(new string[0], "Empty");
	}


	public StructureDataPoint FindPlayerStructure(string name)
	{
		for (int i = 0;i < playerStructures.Length; i++)
		{
			if ((playerStructures[i].name == name))
			{
				return playerStructures[i];
			}
		}

		return playerStructures[0];
	}

	public Item FindItem(string name)
	{
		for (int i = 0;i < itemData.items.Length;i++)
		{
			if (itemData.items[i].name == name)
			{
				return itemData.items[i];
			}
		}

		return new Item("Empty", 0);
	}
}

[System.Serializable]
public struct StructureDataPoint
{
	public string name;
	public GameObject structure;
	public float minSpawnHeight;
	public float maxSpawnHeight;
	public float spacing;

	public bool useGeneric;
	public float health;
	public string lootTable;
}

[System.Serializable]
public struct Item
{
	public string name;
	public int count;
	public List<string> tags;
	public List<float> tagData;

	public Item(string name, int count)
	{
		this.name = name;
		this.count = count;
		this.tags = new List<string>();
		this.tagData = new List<float>();
	}

	public Item(string name, int count, List<string> tags, List<float> tagData)
	{
		this.name = name;
		this.count = count;
		this.tags = tags;
		this.tagData = tagData;
	}


}

[System.Serializable]
public struct LootTable
{
	public string name;
	public string[] drops;
	public int dropCountMin;
	public int dropCountMax;

	public LootTable(string[] drops, string name)
	{
		this.drops = drops;
		this.name = name;
		dropCountMin = 0;
		dropCountMax = 0;
	}
}
