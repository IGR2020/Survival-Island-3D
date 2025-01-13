using UnityEngine;

public class Sapling : MonoBehaviour
{
	SimulatorData simulatorData;
	float growTimer = 0;
	float growSpeed;
	string growthVarient;

	private void Start()
	{
		simulatorData = FindFirstObjectByType<SimulatorData>();

		AssignGrowthVars();

		GetComponent<Structure>().GetDataFromStructureDataPoint(simulatorData.structureData.FindPlayerStructure("Sapling"));
	}

	private void Update()
	{
		growTimer += Time.deltaTime;

		if (growTimer > growSpeed) 
		{
			OnGrow();
		}
	}

	public void OnGrow()
	{
		StructureDataPoint structure = simulatorData.GetStructureDataFromName(growthVarient);
		GameObject grownVersion = Instantiate(structure.structure, transform.position, transform.rotation);
		grownVersion.GetComponent<Structure>().GetDataFromStructureDataPoint(structure);
		Destroy(gameObject);
	}

	public void AssignGrowthVars()
	{
		growTimer = 0;
		growSpeed = simulatorData.sapaplingData.growSpeed + (((Random.value * 2) - 1) * simulatorData.sapaplingData.growSpeedVariation);
		growthVarient = simulatorData.sapaplingData.growVarients[Random.Range(0, simulatorData.sapaplingData.growVarients.Length - 1)];
	}
}

[System.Serializable]
public struct SaplingData
{
	public float growSpeed;
	public float growSpeedVariation;
	public string[] growVarients;
}
