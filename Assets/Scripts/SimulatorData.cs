using UnityEngine;

public class SimulatorData : MonoBehaviour
{
    public float hitDelay;

	public int structuresPerMass = 10;
    public int structureCountVariation = 3;
    public Material massMaterial;
    public Material hitMaterial;
    public StructureData structureData;
    public GameObject water;
    public float waterSize;
    public float waterHeight;
    public GameObject saplingModel;
    public GameObject wheatSeedModel;
    public SaplingData sapaplingData;
    public SaplingData wheatData;

    public StructureDataPoint GetStructureDataFromName(string name)
    {
        foreach (var structure in structureData.structures) 
        {
            if (structure.name == name) return structure;
        }

        return new StructureDataPoint();
    }
}
