using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CraftButton : MonoBehaviour
{
    public RawImage image;
    public Recipe recipe;
	public int index;

    UiHandler uiHandler;
    Crafter crafter;
	private void Start()
	{
        uiHandler = FindFirstObjectByType<UiHandler>();
		crafter = FindFirstObjectByType<Crafter>();

        image.texture = uiHandler.FindItemTexture(recipe.outputItems[0].name);
	}

	public void OnClick()
	{
		crafter.OnCraftClick(index);
	}
}
