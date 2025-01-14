using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Structure : MonoBehaviour
{
	public LootTable drops;
	float health = 5;
	float hitDelay = 0.1f;
	public string structureName;

	SimulatorData simulator = null;
	MeshRenderer meshRenderer;
	Material defaultMaterial;

	bool isHit;
	float timeSinceLastHit;
	float maxHealth = 5;

	bool useChildRenderer = false;
	MeshRenderer[] childRenderers;
	Material[] defaultChildMaterials;

	System.Action mostRecentAttacker = null;

	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		if (simulator == null)
		{
			simulator = FindFirstObjectByType<SimulatorData>();
		}

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

	public void Hit(int attackStrength, System.Action callback)
	{
		if (isHit)
		{
			return;
		}
		mostRecentAttacker = callback;
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
		mostRecentAttacker();

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