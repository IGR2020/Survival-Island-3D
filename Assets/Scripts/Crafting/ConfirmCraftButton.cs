using UnityEngine;

public class ConfirmCraftButton : MonoBehaviour
{
    Crafter crafter;

	private void Start()
	{
		crafter = FindFirstObjectByType<Crafter>();
	}
	public void OnClick() 
	{
		crafter.ConfirmCraft();
	}
}
