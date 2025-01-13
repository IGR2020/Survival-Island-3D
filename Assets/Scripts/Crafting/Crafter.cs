using UnityEngine;

public class Crafter : MonoBehaviour
{
	public GameObject craftingUiContainer;
	public CraftingData craftingData;

	public void CreateGUI()
	{
		
	}
}

[CreateAssetMenu()]
public class CraftingData : ScriptableObject
{
    public Recipe[] recipes;
}

[System.Serializable]
public struct Recipe
{
    public Item[] neededItems;
    public Item[] outputItems;
}