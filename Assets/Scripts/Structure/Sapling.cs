using UnityEngine;

public class Sapling : MonoBehaviour
{
	SimulatorData simulatorData;
	float growTimer = 0;
	float growSpeed;
	string growthVarient;
	public enum PlantType {Sapling, Wheat};
	public PlantType plantType;
	string dataPointName = "Sapling";

	private void Start()
	{
		simulatorData = FindFirstObjectByType<SimulatorData>();

		AssignGrowthVars();

		SetDataPointName();

		GetComponent<Structure>().GetDataFromStructureDataPoint(simulatorData.structureData.FindPlayerStructure(dataPointName));
	}

	private void Update()
	{
		growTimer += Time.deltaTime;

		if (growTimer > growSpeed) 
		{
			OnGrow();
		}
	}

	public void SetDataPointName()
	{

		if (plantType == PlantType.Sapling) dataPointName = "Sapling";
		else if (plantType == PlantType.Wheat) dataPointName = "Wheat";
	}

	public void OnGrow()
	{
		StructureDataPoint structure = simulatorData.GetStructureDataFromName(growthVarient);
		if (structure.structure == null) { structure = simulatorData.structureData.FindPlayerStructure(growthVarient); }
		GameObject grownVersion = Instantiate(structure.structure, transform.position, transform.rotation);
		grownVersion.GetComponent<Structure>().GetDataFromStructureDataPoint(structure);
		Destroy(gameObject);
	}

	public void AssignGrowthVars()
	{
		SetDataPointName();

		SaplingData growData = simulatorData.sapaplingData;
		if (dataPointName == "Sapling") growData = simulatorData.sapaplingData;
		else if (dataPointName == "Wheat") growData = simulatorData.wheatData;

		growTimer = 0;
		growSpeed = growData.growSpeed + (((Random.value * 2) - 1) * growData.growSpeedVariation);
		growthVarient = growData.growVarients[Random.Range(0, growData.growVarients.Length - 1)];
	}
}

[System.Serializable]
public struct SaplingData
{
	public float growSpeed;
	public float growSpeedVariation;
	public string[] growVarients;
}
