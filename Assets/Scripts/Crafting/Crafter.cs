using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Crafter : MonoBehaviour
{
	public GameObject craftingUiContainer;
	public GameObject recipeInfoContainer;
	public GameObject recipeButtonPreset;
	public GameObject recipeInfoTextPreset;
	public List<GameObject> recipeInfoObjects;
	public CraftingData craftingData;
	public CraftButton[] craftButtons;

	private void Start()
	{
		CreateGUI();
	}

	public void CreateGUI()
	{
		craftButtons = new CraftButton[craftingData.recipes.Length];
		for (int i = 0; i < craftingData.recipes.Length; i++)
		{
			GameObject buttonDuplicate = Instantiate(recipeButtonPreset);
			buttonDuplicate.transform.SetParent(craftingUiContainer.transform, false);
			craftButtons[i] = buttonDuplicate.GetComponent<CraftButton>();
			craftButtons[i].recipe = craftingData.recipes[i];
			craftButtons[i].index = i;
		}
	}

	public void OnCraftClick(int index) 
	{ 
		print("Crafting Button " + index.ToString() + " has called") ; 
		
		ClearRecipeInfo();

		CraftButton craftButton = craftButtons[index];

		GameObject infoCaption1 =  Instantiate(recipeInfoTextPreset);
		infoCaption1.GetComponent<TMP_Text>().text = "Needed Items";
		infoCaption1.transform.SetParent(recipeInfoContainer.transform, false);
		recipeInfoObjects.Add(infoCaption1);
		foreach(Item item in craftButton.recipe.neededItems)
		{
			GameObject itemInfo = Instantiate(recipeInfoTextPreset);
			itemInfo.GetComponent<TMP_Text>().text = item.count.ToString() + " " + item.name;
			itemInfo.transform.SetParent(recipeInfoContainer.transform, false);
			recipeInfoObjects.Add(itemInfo);
		}

		GameObject infoCaption2 = Instantiate(recipeInfoTextPreset);
		infoCaption2.GetComponent<TMP_Text>().text = "Output Items";
		infoCaption2.transform.SetParent(recipeInfoContainer.transform, false);
		recipeInfoObjects.Add(infoCaption2);
		foreach (Item item in craftButton.recipe.outputItems)
		{
			GameObject itemInfo = Instantiate(recipeInfoTextPreset);
			itemInfo.GetComponent<TMP_Text>().text = item.count.ToString() + " " + item.name;
			itemInfo.transform.SetParent(recipeInfoContainer.transform, false);
			recipeInfoObjects.Add(itemInfo);
		}
	}

	public void ClearRecipeInfo()
	{
		foreach (GameObject infoObject in recipeInfoObjects)
		{
			Destroy(infoObject);
		}
		recipeInfoObjects.Clear();
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

[CreateAssetMenu()]
public class ItemData : ScriptableObject
{
	public Item[] items;
}