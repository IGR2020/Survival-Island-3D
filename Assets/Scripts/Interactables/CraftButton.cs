using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CraftButton : MonoBehaviour
{
    public Image image;
    public Recipe recipe;
	public int index;

    UiHandler uiHandler;
    Crafter crafter;
	private void Start()
	{
        uiHandler = FindFirstObjectByType<UiHandler>();
		crafter = FindFirstObjectByType<Crafter>();

        image.sprite = uiHandler.FindItemSprite(recipe.outputItems[0].name);
	}

	public void OnClick()
	{
		crafter.OnCraftClick(index);
	}
}
