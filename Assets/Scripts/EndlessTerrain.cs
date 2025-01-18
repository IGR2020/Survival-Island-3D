using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
	public string terrainMask;
	public static float maxViewDist = 300;
	float sqrViewerMoveDistForUpdate = 30;

	public float automaticReloadTimer = 0;
	public float automaticReloadSpeed = 3;
	public bool autoReload = false;
	public bool firstReload = true;

	public Transform viewer;
	public LODInfo[] detailLevels;

	public static Vector2 viewerPosition;
	public static Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	static SimulatorData simulatorData;
	int massSize;
	int massInView;

	Dictionary<Vector2, TerrainMass> massDict = new Dictionary<Vector2, TerrainMass>();
	static List<TerrainMass> LastUpdateVisibleMass = new List<TerrainMass>();

	void Start()
	{
		mapGenerator = FindFirstObjectByType<MapGenerator>();
		simulatorData = FindFirstObjectByType<SimulatorData>();

		maxViewDist = detailLevels[detailLevels.Length - 1].viewDistance;
		
		massSize = mapGenerator.mapMassSize - 1;
		massInView = Mathf.RoundToInt(maxViewDist) / massSize;
		
		if (!mapGenerator.hasAssignedFallOffMap)
		{
			mapGenerator.ReloadFallOffMap();
		}

		viewerPositionOld = new Vector2(viewer.position.x, viewer.position.z);
	}

	[ContextMenu("Print Terrain Data")]
	public void PrintData()
	{
		int currentMassCordX = Mathf.RoundToInt(viewerPosition.x / massSize);
		int currentMassCordY = Mathf.RoundToInt(viewerPosition.y / massSize);

		for (int yOffset = -massInView; yOffset <= massInView; yOffset++)
		{
			for (int xOffset = -massInView; xOffset <= massInView; xOffset++)
			{
				Vector2 viewedMassCord = new Vector2(currentMassCordX + xOffset, currentMassCordY + yOffset);
				print(viewedMassCord);
				print(massDict[viewedMassCord]);
			}
		}
	}

	[ContextMenu("Reload Map")]
	public void UpdateMapData()
	{
		maxViewDist = detailLevels[detailLevels.Length - 1].viewDistance;

		int currentMassCordX = Mathf.RoundToInt(viewerPosition.x / massSize);
		int currentMassCordY = Mathf.RoundToInt(viewerPosition.y / massSize);

		for (int yOffset = -massInView; yOffset <= massInView; yOffset++)
		{
			for (int xOffset = -massInView; xOffset <= massInView; xOffset++)
			{
				Vector2 viewedMassCord = new Vector2(currentMassCordX + xOffset, currentMassCordY + yOffset);

				if (massDict.ContainsKey(viewedMassCord))
				{
					massDict[viewedMassCord].ReloadMapData();
				}
			}
		}
	}

	[ContextMenu("Print Current Terrain Range")]
	public void PrintTerrainRange()
	{
		print(Noise.maxNoiseHeight);
		print(Noise.minNoiseHeight);
	}

	private void Update()
	{
		if (!mapGenerator.hasAssignedFallOffMap)
		{
			return;
		}

		if (autoReload | firstReload)
		{
			automaticReloadTimer += Time.deltaTime;
			if (automaticReloadTimer > automaticReloadSpeed)
			{
				UpdateMapData();
				firstReload = false;
				automaticReloadTimer = 0;
				UpdateVisibleMass();
			}
		}
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveDistForUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleMass();

		}
	}

	void UpdateVisibleMass()
	{
		for (int i = 0; i < LastUpdateVisibleMass.Count; i++)
		{
			LastUpdateVisibleMass[i].SetVisible(false);
		}
		LastUpdateVisibleMass.Clear();

		int currentMassCordX = Mathf.RoundToInt(viewerPosition.x / massSize);
		int currentMassCordY = Mathf.RoundToInt(viewerPosition.y / massSize);

		for (int yOffset = -massInView; yOffset <= massInView; yOffset++)
		{
			for (int xOffset = -massInView; xOffset <= massInView; xOffset++)
			{
				Vector2 viewedMassCord = new Vector2(currentMassCordX + xOffset, currentMassCordY + yOffset);
			
				if (massDict.ContainsKey(viewedMassCord))
				{
					massDict[viewedMassCord].UpdateMass();
				}
				else
				{
					massDict.Add(viewedMassCord, new TerrainMass(viewedMassCord, massSize, detailLevels, transform, terrainMask));
				}
			}
		}
	}

	public class TerrainMass
	{
		public Vector2 position;
		public Vector3 positionV3;
		public GameObject meshObject;
		Bounds bounds;

		MapData mapData;
		bool mapDataSet = false;

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		MeshCollider meshCollider;
		Island island;

		LODMesh lodMesh;
		LODInfo[] detailLevels;

		public TerrainMass(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, string layer) 
		{
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			positionV3 = new Vector3(position.x, 0, position.y);
			
			meshObject = new GameObject("Terrain Mass");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshRenderer.material = simulatorData.massMaterial;
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			StructureGenerator structureGenerator = meshObject.AddComponent<StructureGenerator>();
			island = meshObject.AddComponent<Island>();

			structureGenerator.AssignConnections();
			Vector3 waterPosition = positionV3 + structureGenerator.ConvertNoiseToMeshHeight(simulatorData.waterHeight) * Vector3.up;
			GameObject water = Instantiate(simulatorData.water, waterPosition, parent.rotation);
			water.transform.localScale = ((mapGenerator.mapMassSize - 1) / simulatorData.waterSize) * parent.localScale;

			meshObject.transform.position = positionV3;
			meshObject.transform.localScale = parent.localScale;
			meshObject.transform.parent = parent;
			meshObject.layer = LayerMask.NameToLayer(layer);
			water.transform.SetParent(meshObject.transform, true);

			SetVisible(false);

			lodMesh = new LODMesh(1, UpdateMesh);
			this.detailLevels = detailLevels;

			mapGenerator.RequestMapData(OnMapDataRecived, position);
		}

		public void ReloadMapData()
		{
			for (int i = 0; i < 6; i++)
			{
				lodMesh.assignedMeshes[i] = false;
			}
			mapGenerator.RequestMapData(OnMapDataRecived, position);
		}

		public void OnMapDataRecived(MapData data)
		{
			mapData = data;
			mapDataSet = true;

			lodMesh.SetLOD(1, mapData);
			meshRenderer.material.mainTexture = TextureGenrator.CreateColorTexture(mapData.noiseMap, mapData.colorMap, mapGenerator.terrainData.filterMode);

			UpdateMass();
		}


		public void UpdateMesh() {
			if (!mapDataSet) { return; }

			if (lodMesh.meshUpdated)
			{
				meshFilter.mesh = lodMesh.GetMesh();
				meshCollider.sharedMesh = lodMesh.GetMesh();
				island.GiveMeshData(lodMesh.GetMeshData());
			}
		}

		public void UpdateMass()
		{
			LastUpdateVisibleMass.Add(this);

			float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
			bool isVisible = viewerDistFromNearestEdge <= maxViewDist;
			SetVisible(isVisible);

			if (!mapDataSet) { return; }

			UpdateMesh();

			for (int i = 0; i < detailLevels.Length; i++)
			{
				if (detailLevels[i].viewDistance > viewerDistFromNearestEdge)
				{
					lodMesh.SetLOD(detailLevels[i].lod, mapData);
					break;
				}
			}
		}

		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}

		public bool IsVisible()
		{
			return meshObject.activeSelf;
		}
	}

	class LODMesh
	{
		System.Action updateCallback;

		public Mesh[] meshes = new Mesh[6];
		public MeshData[] meshDatas = new MeshData[6];
		public bool[] assignedMeshes = new bool[6];

		public bool requestedMeshData = false;
		public bool meshUpdated = false;
		public int currentMesh = 0;
		int lod;

		bool originalUpdate = true;

		public LODMesh(int lod, System.Action callback)
		{
			updateCallback = callback;

			for (int i = 0; i < 6; i++)
			{
				assignedMeshes[i] = false;
			}
			this.lod = Mathf.Clamp(lod, 1, 6);
		}

		public Mesh GetMesh()
		{
			meshUpdated = false;
			return meshes[currentMesh];
		}

		public MeshData GetMeshData()
		{
			return meshDatas[currentMesh];
		}

		public void RecivedMeshData(MeshData meshData)
		{
			meshDatas[meshData.lod - 1] = meshData;
			meshes[meshData.lod - 1] = meshData.CreateMesh();
			currentMesh = meshData.lod - 1;
			assignedMeshes[currentMesh] = true;
			meshUpdated = true;
			requestedMeshData = false;
			updateCallback();
		}

		public void SetLOD(int LOD, MapData mapData)
		{
			lod = Mathf.Clamp(LOD, 1, 6);
			if (originalUpdate)
			{
				originalUpdate = false;
				currentMesh = lod - 1;
				RequestMeshData(mapData);
			}
			if (lod - 1 == currentMesh)
			{
				return;
			}
			if (assignedMeshes[lod - 1])
			{
				currentMesh = lod - 1;
				meshUpdated = true;
			}
			else
			{
				RequestMeshData(mapData);
			}
		}

		public void RequestMeshData(MapData mapData)
		{
			if (requestedMeshData) { return; }
			requestedMeshData = true;
			if (assignedMeshes[lod - 1])
			{
				return;
			}

			mapData.lod = lod;

			mapGenerator.RequestMeshData(RecivedMeshData, mapData);
			
		}
	}

	[System.Serializable]
	public struct LODInfo
	{
		[Range(1, 6)]
		public int lod;
		public float viewDistance;
	}
}