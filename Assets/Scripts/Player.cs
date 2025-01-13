using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
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
	[Range(0f, 0.4f)]
	public float speedToHungerDrain = 0.2f;
	public float hungerToHealthDrain = 1f;
	public List<Item> inventory = new List<Item>();

	PlayerCamera playerCamera;
	Vector3 velocity;
	Vector3 moveDirection;

	//Animator animator;
	CharacterController controller;
	MapGenerator mapGenerator;
	SimulatorData simulatorData;
	Building builder;
	UiHandler uiHandler;
	Structure mostRecentAttackedStructure;

	bool isGroundedLastFrame = false;
	
	bool buildMode = false;
	StructureDataPoint buildStructure;
	Vector3 buildRotationOffset;
	GameObject buildPreview;

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

		if (hunger < 1)
		{
			health -= Time.deltaTime * hungerToHealthDrain;
		}

		if (health < 1)
		{
			SceneLoader.Load(SceneLoader.SceneName.DeathScreen);
		}

		if (buildMode)
		{
			if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, attackRange))
			{
				if (rayInfo.collider.gameObject.name == "Terrain Mass")
				{
					buildPreview.transform.position = rayInfo.point;
					buildPreview.transform.rotation = transform.rotation;
					buildPreview.transform.rotation = builder.SnapBuildRotation(buildPreview.transform);
				}
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

        Item heldItem = uiHandler.GetHeldItem();
		if (heldItem.name == "Empty")
		{
			return;
		}

		bool buildingInteration = false;

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
			if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, attackRange))
			{
				if (rayInfo.collider.gameObject.name == "Terrain Mass")
				{
					Instantiate(simulatorData.saplingModel, rayInfo.point, transform.rotation, rayInfo.collider.gameObject.transform);
					heldItem.count -= 1;
				}
			}
		}

		else if (heldItem.tags.Contains("Build"))
		{
			buildStructure = simulatorData.structureData.playerStructures[(int)heldItem.tagData[0]];
			if (!buildMode) {
				buildMode = false;
				if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit ray, attackRange))
				{
					if (ray.collider.gameObject.name == "Terrain Mass")
					{ 
						buildMode = true; 
						buildPreview = Instantiate(buildStructure.structure, ray.point, transform.rotation, ray.collider.gameObject.transform);
						buildPreview.GetComponent<Structure>().enabled = false;
						buildPreview.GetComponent<Collider>().enabled = false;
					}
				}
				return; 
			}

			buildMode = false;
			Destroy(buildPreview);

			if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit rayInfo, attackRange))
			{
				if (rayInfo.collider.gameObject.name == "Terrain Mass")
				{
					GameObject building = Instantiate(buildStructure.structure, rayInfo.point, transform.rotation, rayInfo.collider.gameObject.transform);
					building.transform.rotation = builder.SnapBuildRotation(building.transform);
					building.GetComponent<Structure>().GetDataFromStructureDataPoint(buildStructure);
					heldItem.count -= 1;
				}
			}
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

		if (Physics.Raycast(transform.position, playerCamera.transform.forward, out RaycastHit hitInfo,attackRange))
		{
			GameObject hitObject = hitInfo.collider.gameObject;
			Structure structure = hitObject.GetComponent<Structure>();

			if (structure == null)
			{
				return;
			}

			mostRecentAttackedStructure = structure;
			structure.Hit(attackStrength, OnWin);
		}
	}

	public void OnWin()
	{
		AddLootTable(mostRecentAttackedStructure.drops);
		uiHandler.UpdateSlots();
	}

	public void AddLootTable(LootTable table)
	{
		for (int i = 0; i < table.drops.Length; i++)
		{
			bool foundMatch = false;
			Item drop = simulatorData.structureData.FindItem(table.drops[i]);
			drop.count = Random.Range(table.dropCountMin, table.dropCountMax);

			for (int j = 0; j < inventory.Count; j++)
			{
				Item item = inventory[j];

				if (drop.name == item.name)
				{
					inventory[j] =  new Item(item.name, item.count + drop.count, item.tags, item.tagData);
					foundMatch = true;
					break;
				}
			}

			if (!foundMatch)
			{
				inventory.Add(drop);
			}

		}
	}
}
