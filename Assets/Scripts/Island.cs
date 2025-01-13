using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof(MeshCollider))]
public class Island : MonoBehaviour
{
	public int structureCount = 10;
	public float pushStructureDownBy = 0.04f;
	public bool randomlyOffsetStructureLocation = true;
	public enum StructureSummonMethod {PerFrame1, AllAtOnce}
	public StructureSummonMethod structureSummonMethod = StructureSummonMethod.PerFrame1;

	StructureGenerator structureGenerator;

	MeshData meshData;

	int x = 0;
	bool hasGeneratedStructures = false;
	bool meshDataAssigned = false;

	private void Update()
	{
		if (!hasGeneratedStructures && meshDataAssigned)
		{
			if (structureSummonMethod == StructureSummonMethod.PerFrame1)
			{
				structureGenerator.SummonStructures(meshData, 1, transform, randomlyOffsetStructureLocation, pushStructureDownBy);
				x++;

				if (x == structureCount)
				{
					hasGeneratedStructures = true;
				}
			}
			else
			{
				structureGenerator.SummonStructures(meshData, structureCount, transform, randomlyOffsetStructureLocation, pushStructureDownBy);
				hasGeneratedStructures = true;
			}
		}
	}

	private void Start()
	{
		structureGenerator = GetComponent<StructureGenerator>();
		if (structureGenerator == null) 
		{
			structureGenerator = gameObject.AddComponent<StructureGenerator>();
		}
	}

	[ContextMenu("Summon Structures")]
	public void SummonStructures()
	{
		if (!meshDataAssigned) { return; }
		structureGenerator.SummonStructures(meshData, structureCount, transform, randomlyOffsetStructureLocation, pushStructureDownBy);
	}

	public void GiveMeshData(MeshData meshData)
	{
		this.meshData = meshData;
		meshDataAssigned = true;
	}
}
