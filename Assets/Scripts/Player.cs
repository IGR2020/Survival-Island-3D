using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
	public float gravity;
	public float speed;
	public float acceleration;
	[Range(0f, 2f)]
	public float friction;
	public float attackRange;
	public int attackStrength = 1;
	public float health;
	[HideInInspector()]
	public float maxHealth;
	public float jumpHeight;
	public float minHeightForDamage = 3;
	[HideInInspector()]
	public float hunger = 100;
	[HideInInspector()]
	public float maxHunger = 100;
	[HideInInspector()]
	public float thirst = 100;
	[HideInInspector()]
	public float maxThirst = 100;
	[Range(0f, 0.4f)]
	public float speedToHungerDrain = 0.2f;
	[Range(0f, 1f)]
	public float speedToThirstDrain = 0.2f;
	public float hungerToHealthDrain = 1f;
	public float thirstToHealthDrain = 1f;
	public LayerMask validBuildArea;
	public LayerMask validPlantArea;
	[Range(0f, 1f)]
	public float buildPreviewTransparency;
	public float throwStrength;
	public int maxInventorySize = 7;
	public List<Item> inventory = new List<Item>();

	PlayerCamera playerCamera;
	Vector3 velocity;
	Vector3 moveDirection;

	//Animator animator;
	CharacterController controller;
	MapGenerator mapGenerator;
	SimulatorData simulatorData;
	[HideInInspector()]
	public Building builder;
	UiHandler uiHandler;
	Structure mostRecentAttackedStructure;

	bool isGroundedLastFrame = false;

	[HideInInspector()]
	public bool buildMode = false;
	public Vector3 buildPreviewSize = Vector3.zero;
	StructureDataPoint buildStructure;
	Vector3 buildRotationOffset;
	[HideInInspector()]
	public GameObject buildPreview;

	[HideInInspector()]
	public bool isUsingGui = false;

	private void Start()
	{
		maxHealth = health;

		mapGenerator = FindFirstObjectByType<MapGenerator>();
		uiHandler = FindFirstObjectByType<UiHandler>();
		controller = GetComponent<CharacterController>();
		playerCamera = FindFirstObjectByType<PlayerCamera>();
		simulatorData = FindFirstObjectByType<SimulatorData>();
		builder = FindFirstObjectByType<Building>();
		//animator = GetComponent<Animator>();

		transform.position += Vector3.up * mapGenerator.terrainData.noiseAmplification * mapGenerator.transform.localScale.x;

	}

	private void Update()
	{
		if (!mapGenerator.hasAssignedFallOffMap) {print("waiting"); return; }

		playerCamera.lockRotation = isUsingGui;

		if (!controller.isGrounded)
		{
			velocity += Vector3.down * gravity * Time.deltaTime;
		}
		
		if (!isGroundedLastFrame && controller.isGrounded)
		{
			HasLanded();
		}

		isGroundedLastFrame = controller.isGrounded;

		velocity += moveDirection.x * transform.right * acceleration * Time.deltaTime;
		velocity += moveDirection.y * transform.forward * acceleration * Time.deltaTime;

		velocity -= friction * velocity * Time.deltaTime;

		if (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.z) > speed)
		{
			velocity.x = Mathf.Clamp(velocity.x, -speed, speed);
			velocity.z = Mathf.Clamp(velocity.z, -speed, speed);
		}

		hunger -= (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.z)) * Time.deltaTime * speedToHungerDrain;
		thirst -= (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.z)) * Time.deltaTime * speedToThirstDrain;

		if (hunger < 1)
		{
			health -= Time.deltaTime * hungerToHealthDrain;
		}

		if (thirst < 1)
		{
			health -= Time.deltaTime * thirstToHealthDrain;
		}

		if (health < 1)
		{
			SceneLoader.Load(SceneLoader.SceneName.DeathScreen);
		}

		if (buildMode)
		{
			if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, attackRange, validBuildArea))
			{
				buildPreview.transform.position = rayInfo.point;
				buildPreview.transform.rotation = transform.rotation;
				buildPreview.transform.rotation = builder.SnapBuildRotation(buildPreview.transform);
				buildPreview.transform.position = builder.SnapToGrid(buildPreview.transform.position, buildPreviewSize);
			}
		}

		controller.Move(velocity * Time.deltaTime);
	}

	public void HasLanded()
	{
		health -= Mathf.Max(-velocity.y - minHeightForDamage, 0);
	}

	public void OnJump()
	{
		velocity = Vector3.up * jumpHeight;
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
        if (!context.started) return;

		if (isUsingGui) return;

		bool buildingInteration = false;

		if (!buildMode && Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit ray, attackRange))
		{
			Interactable interacter = ray.collider.gameObject.GetComponent<Interactable>();
			if (interacter == null) {
				interacter = ray.collider.transform.parent.GetComponent<Interactable>();
			}
			if (interacter != null) { 
				if (interacter.buildType == Interactable.BuildType.Crafting) uiHandler.AllowCrating();
				if (interacter.buildType == Interactable.BuildType.Crate) interacter.GetComponent<Storage>().Activate();
				return;
			}
		}

		Item heldItem = uiHandler.GetHeldItem();
		if (heldItem.name == "Empty")
		{
			return;
		}

		if (heldItem.tags.Contains("Food"))
		{
			print("Consuming Food");
			int index = heldItem.tags.IndexOf("Food");
			hunger += heldItem.tagData[index];
			heldItem.count -= 1;
		}

		else if (heldItem.name == "Sapling")
		{
			print("Planting Item");
			if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, attackRange, validPlantArea))
			{
				Instantiate(simulatorData.saplingModel, rayInfo.point, transform.rotation, rayInfo.collider.gameObject.transform);
				heldItem.count -= 1;
			}
		}

		else if (heldItem.tags.Contains("Build"))
		{
			buildStructure = simulatorData.structureData.playerStructures[(int)heldItem.tagData[0]];
			if (!buildMode) {
				buildMode = false;
				if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit castedRay, attackRange, validBuildArea))
				{
					buildMode = true; 
					buildPreview = Instantiate(buildStructure.structure, castedRay.point, transform.rotation, castedRay.collider.gameObject.transform);
					MeshRenderer buildPreviewRenderer = buildPreview.GetComponent<MeshRenderer>();
					if (buildPreviewRenderer != null)
					{
						Color buildPreviewColor = buildPreviewRenderer.material.color;
						buildPreviewColor.a = buildPreviewTransparency;
						buildPreviewRenderer.material.color = buildPreviewColor;
					}
					else
					{
						print("No Mesh Renderer Available");
					}
					BoxCollider buildPreviewCollider = buildPreview.GetComponent<BoxCollider>();
					if (buildPreviewCollider != null)
					{
						buildPreviewSize = buildPreview.GetComponent<BoxCollider>().size;
					}
					else
					{
						buildPreviewSize = Vector3.zero;
					}
					buildPreview.GetComponent<Structure>().enabled = false;
					buildPreview.GetComponent<Collider>().enabled = false;
					uiHandler.SetGrid();
				}
				return; 
			}

			buildMode = false;
			Destroy(buildPreview);

			GameObject building = Instantiate(buildStructure.structure, buildPreview.transform.position, buildPreview.transform.rotation, buildPreview.transform.parent);
			building.GetComponent<Structure>().GetDataFromStructureDataPoint(buildStructure);
			heldItem.count -= 1;
		}

		else
		{
			DropItem();
		}

		if (!buildingInteration)
		{
			if (buildMode)
			{
				Destroy(buildPreview);
			}
			buildMode = false;
		}

		heldItem = Slot.SetEmptyIf0(heldItem);
		uiHandler.SetHeldItem(heldItem);
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (!context.started || !controller.isGrounded) { return; }
		OnJump();
	}

	[ContextMenu("Print Info")]
	public void GetInfo()
	{
		print("Velocity " + velocity.ToString());
		print("Is Grounded " + controller.isGrounded.ToString());
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		moveDirection = context.ReadValue<Vector2>();
	}

	public void OnAttack(InputAction.CallbackContext context) 
	{
		if (!context.canceled)
		{
			return;
		}

		if (isUsingGui) return;

		if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit hitInfo, attackRange))
		{
			GameObject hitObject = hitInfo.collider.gameObject;
			Structure structure = hitObject.GetComponent<Structure>();

			if (structure == null)
			{
				return;
			}
			mostRecentAttackedStructure = structure;
			structure.Hit(attackStrength);
		}
	}

	public void CraftRecipe(Recipe recipe)
	{
		if (!isUsingGui) return;

		foreach (Item need in recipe.neededItems)
		{
			if (!InventoryContains(need))
			{
				return;
			}
		}

		print("Valid Craft -> " + recipe.outputItems[0].name);

		foreach (Item need in recipe.neededItems)
		{
			SubtractItem(need);
		}

		foreach (Item output in recipe.outputItems)
		{
			Item tagedItem = simulatorData.structureData.FindItem(output.name);
			tagedItem.count = output.count;
			tagedItem.name = output.name;
			AddItem(tagedItem);
		}

		uiHandler.UpdateSlots();
	}

	public bool InventoryContains(Item item)
	{
		foreach (Item containedItem in inventory)
		{
			if (containedItem.name == item.name && containedItem.count >= item.count)
			{
				return true;
			}
		}
		return false;
	}

	public void SubtractItem(Item item)
	{
		for (int i = 0; i<inventory.Count; i++)
		{
			Item containedItem = inventory[i];
			if (containedItem.name == item.name)
			{
				containedItem.count -= item.count;
				containedItem = Slot.SetEmptyIf0(containedItem);
			}
			if (containedItem.name != "Empty")
			{
				inventory[i] = containedItem;
			}
			else
			{
				inventory.RemoveAt(i);
			}
		}
	}

	// Returns ture if adding of item was successful
	public bool AddItem(Item item1)
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item2 = inventory[i];

			if (item1.name == item2.name)
			{
				inventory[i] = new Item(item1.name, item1.count + item2.count, item2.tags, item2.tagData);
				return true;
			}
		}

		if (inventory.Count < maxInventorySize)
		{
			inventory.Add(item1);
			return true;
		}
		return false;
	}

	public void DropItem()
	{
		Item heldItem = uiHandler.GetHeldItem();
		if (heldItem.name == "Empty")
		{
			return;
		}

		GameObject thrownItem = Instantiate(uiHandler.presetGroundedItem, transform.position + transform.forward * throwStrength, transform.rotation);
		thrownItem.GetComponent<GroundedItem>().item = heldItem;
		heldItem = new Item("Empty", 0);
		uiHandler.SetHeldItem(heldItem);
	}
}
