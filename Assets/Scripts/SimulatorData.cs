using UnityEngine;

public class SimulatorData : MonoBehaviour
{
    public float hitDelay;

    public Material hitMaterial;
    public StructureData structureData;
    public GameObject water;
    public float waterSize;
    public float waterHeight;
    public GameObject saplingModel;
    public SaplingData sapaplingData;

    public StructureDataPoint GetStructureDataFromName(string name)
    {
        foreach (var structure in structureData.structures) 
        {
            if (structure.name == name) return structure;
        }

        return new StructureDataPoint();
    }
}
