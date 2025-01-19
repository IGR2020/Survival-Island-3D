using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class StructureGenerator : MonoBehaviour
{
	StructureData structureData;
	MapGenerator mapGenerator;

	float mapScale = 1;
	List<Vector3> invalidAreas;
	List<Vector3> placedAreas;

	System.Random structureRandomizer;

	public void ResetAreas()
	{
		invalidAreas = new List<Vector3>();
		placedAreas = new List<Vector3>();
	}

	private void Start()
	{
		AssignConnections();
		structureRandomizer = new System.Random(mapGenerator.noiseData.seed);

		invalidAreas = new List<Vector3>();
		placedAreas = new List<Vector3>();
		mapScale = mapGenerator.transform.localScale.x;
	}

	public void AssignConnections()
	{
		structureData = FindAnyObjectByType<SimulatorData>().structureData;
		mapGenerator = FindFirstObjectByType<MapGenerator>();
	}

	public float ConvertNoiseToMeshHeight(float noiseHeight)
	{
		return mapGenerator.terrainData.heightCurve.Evaluate(noiseHeight) * mapGenerator.terrainData.noiseAmplification;
	}

	public void RecalculateAllSpacings(MeshData meshData, float spacing)
	{
		invalidAreas.Clear();
		foreach (var vertex in meshData.vertices)
		{
			if (placedAreas.Contains(vertex))
			{
				continue;
			}

			CheckAndAddIfInvalidPoint(vertex, spacing);
		}
	}

	public void CheckAndAddIfInvalidPoint(Vector3 vertex, float spacing)
	{
		foreach (var placedVertex in placedAreas)
		{
			if (Vector3.Distance(placedVertex, vertex) <= spacing)
			{
				invalidAreas.Add(vertex);
				break;
			}
		}
	}

	public Vector3 GetValidSummonPoint(MeshData meshData, float minHeight, float maxHeight, float spacing, bool convertToMeshHeight = false, bool randomlyOffsetPositon = false)
	{
		if (convertToMeshHeight)
		{
			minHeight = ConvertNoiseToMeshHeight(minHeight);
			maxHeight = ConvertNoiseToMeshHeight(maxHeight);
		}

		for (int i = 0; i < meshData.vertices.Length; i += Random.Range(mapGenerator.structurePlacementSkippingMin, mapGenerator.structurePlacementSkippingMax))
		{
			Vector3 vertex = meshData.vertices[i];

			if (randomlyOffsetPositon && i > 0 && i < meshData.vertices.Length - 1)
			{
				Vector3 offsetVertex = (Random.value > 0.5) ? meshData.vertices[i + 1] : meshData.vertices[i - 1];
				if (Vector3.Distance(vertex, offsetVertex) < 3) {
					vertex = Vector3.Lerp(vertex, offsetVertex, Random.value);
					CheckAndAddIfInvalidPoint(vertex, spacing);
				} 
			}

			if (invalidAreas.Contains(vertex))
			{
				continue;
			}

			if (placedAreas.Contains(vertex))
			{
				continue;
			}

			if (vertex.y > minHeight && vertex.y < maxHeight)
			{
				placedAreas.Add(vertex);
				return vertex * mapScale + transform.position;
			}
		}

		return Vector3.zero;
	}

	public void SummonStructures(MeshData meshData, int maxSummonCount, Transform parent, bool offsetLocationRandomly = false, float pushStructureDownBy = 0) 
	{
		for (int i = 0; i < maxSummonCount; i++)
		{
			StructureDataPoint[] randomSequenceStructureData = structureRandomizer.Shuffle(structureData.structures);

			foreach (var structure in randomSequenceStructureData)
			{
				RecalculateAllSpacings(meshData, structure.spacing);
				Vector3 summonPoint = GetValidSummonPoint(meshData, structure.minSpawnHeight, structure.maxSpawnHeight, structure.spacing, true, offsetLocationRandomly);
				if (summonPoint == Vector3.zero)
				{
					continue;
				}

				summonPoint.y -= pushStructureDownBy;

				GameObject createdStructure = Instantiate(structure.structure, summonPoint, structure.structure.transform.rotation);
				createdStructure.transform.parent = parent;
				if (structure.useGeneric)
				{
					createdStructure.GetComponent<Structure>().GetDataFromStructureDataPoint(structureData.generic);
				}
				else
				{
					createdStructure.GetComponent<Structure>().GetDataFromStructureDataPoint(structure);
				}
				break;
			}
		}
	}

	[ContextMenu("Print Data")]
	public void PrintData()
	{
		print("Invalid Areas " + invalidAreas.Count.ToString());
	}
}

static class RandomExtensions
{
	public static T[] Shuffle<T>(this System.Random rng, T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = rng.Next(n--);
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}

		return array;
	}
}