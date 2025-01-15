using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent (typeof(Rigidbody))]
public class GroundedItem : MonoBehaviour
{
    public Item item;
    Player player;
	SpriteRenderer spriteRenderer;
	UiHandler uiHandler;

	private void Start()
	{
		player = FindFirstObjectByType<Player>();
		uiHandler = FindFirstObjectByType<UiHandler>();

		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = uiHandler.FindItemSprite(item.name);
	}

	private void Update()
	{
		transform.LookAt(player.transform.position);
	}

	private void OnTriggerEnter(Collider collision)
	{
		print(collision.gameObject.name);
		Player player = collision.gameObject.GetComponent<Player>();
		if (player == null) { return; }
		if (!player.AddItem(item)) return;
		uiHandler.UpdateSlots();
		Destroy(gameObject);
	}
}
