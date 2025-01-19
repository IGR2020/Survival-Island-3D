using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Collider))]
// So that the size of the object may be known
[RequireComponent (typeof(BoxCollider))]
public class Structure : MonoBehaviour
{
	public LootTable drops;
	float health = 5;
	float hitDelay = 0.1f;
	public string structureName;

	SimulatorData simulator = null;
	UiHandler uiHandler = null;
	MeshRenderer meshRenderer;
	Material defaultMaterial;

	bool isHit;
	float timeSinceLastHit;
	float maxHealth = 5;

	bool useChildRenderer = false;
	MeshRenderer[] childRenderers;
	Material[] defaultChildMaterials;


	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		simulator = FindFirstObjectByType<SimulatorData>();
		uiHandler = FindFirstObjectByType<UiHandler>();

		hitDelay = simulator.hitDelay;

		if (meshRenderer == null)
		{
			useChildRenderer = true;
			childRenderers = GetComponentsInChildren<MeshRenderer>();
			defaultChildMaterials = new Material[childRenderers.Length];
			for (int i = 0; i < childRenderers.Length; i++)
			{
				defaultChildMaterials[i] = childRenderers[i].material;
			}
		}
		else
		{
			defaultMaterial = meshRenderer.material;
		}
	}

	private void Update()
	{
		IfIsHit();
	}

	public void GetDataFromStructureDataPoint(StructureDataPoint dataPoint)
	{
		name = dataPoint.name;
		health = dataPoint.health;
		maxHealth = dataPoint.health;
		if (simulator == null)
		{
			simulator = FindFirstObjectByType<SimulatorData>();
		}
		drops = simulator.structureData.FindLootTable(dataPoint.lootTable);
	}

	public void Hit(int attackStrength)
	{
		if (isHit)
		{
			return;
		}
		health -= attackStrength;

		if (health < 0) OnDeath();

		timeSinceLastHit = 0;
		isHit = true;
		if (!useChildRenderer)
		{
			meshRenderer.material = simulator.hitMaterial;
		}
		else
		{
			foreach (MeshRenderer child in childRenderers)
			{
				child.material = simulator.hitMaterial;
			}
		}
	}

	public void OnDeath()
	{
		for (int i = 0; i < drops.drops.Length; i++)
		{
			Item drop = simulator.structureData.FindItem(drops.drops[i]);
			drop.count = Random.Range(drops.dropCountMin, drops.dropCountMax);

			GroundedItem item = Instantiate(uiHandler.presetGroundedItem, transform.position + Vector3.up * 3, transform.rotation, transform.parent).GetComponent<GroundedItem>();
			item.item = drop;
		}
			Destroy(gameObject);
	}

	public void IfIsHit()
	{
		if (isHit)
		{
			timeSinceLastHit += Time.deltaTime;
			if (timeSinceLastHit > hitDelay)
			{
				isHit = false;
				if (!useChildRenderer)
				{
					meshRenderer.material = defaultMaterial;
				}
				else
				{
					for (int i = 0; i < childRenderers.Length; i++)
					{
						childRenderers[i].material = defaultChildMaterials[i];
					}
				}
			}
		}
	}
}