using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{ 
	public int mapMassSize = 120;
	public int noiseSize = 120;

	public NoiseData noiseData;
	public TerrainData terrainData;

	public int structurePlacementSkippingMin = 1;
	public int structurePlacementSkippingMax = 1;

	TextureGenrator textureGenrator;

	public float[,] fallOffMap;
	[HideInInspector()]
	public bool hasAssignedFallOffMap = false;

	Queue<MapThreadInfo<MapData>> mapThreadDataInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshThreadDataInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void RequestMeshData(Action<MeshData> callback, MapData mapData)
	{
		ThreadStart threadStart = delegate
		{
			MeshDataThread(callback, mapData);
		};
		new Thread(threadStart).Start();
	}

	void MeshDataThread(Action<MeshData> callback, MapData mapData)
	{
		MeshData meshData = GenerateMeshData(mapData);
		lock (mapThreadDataInfoQueue)
		{
			meshThreadDataInfoQueue.Enqueue(new MapThreadInfo<MeshData>(meshData, callback));
		}
	}

	public void RequestMapData(Action<MapData> callback, Vector2 noiseOffset)
	{
		ThreadStart threadStart = delegate
		{
			noiseOffset *= (noiseSize / mapMassSize);
			MapDataThread(callback, noiseOffset);
		};
		new Thread(threadStart).Start();
	}

	void MapDataThread(Action<MapData> callback, Vector2 noiseOffset)
	{
		MapData mapData = GenerateMapData(noiseOffset);
		lock (mapThreadDataInfoQueue)
		{
			mapThreadDataInfoQueue.Enqueue(new MapThreadInfo<MapData>(mapData, callback));
		}
	}

	void Start()
	{
		textureGenrator = FindAnyObjectByType<TextureGenrator>();

		if (!hasAssignedFallOffMap)
		{
			ReloadFallOffMap();
		}
	}

	[ContextMenu("Reload Fall Off Map")]
	public void ReloadFallOffMap()
	{
		fallOffMap = FallOffMapGenerator.CreateFallOffMap(noiseSize, noiseData.maxFallOffHeight);
		hasAssignedFallOffMap = true;
	}

	[ContextMenu("Print Fall Off Map")]
	public void PrintFallOffMap()
	{
		for (int i = 0; i < fallOffMap.GetLength(0); i++)
		{
			for (int j = 0; j < fallOffMap.GetLength(1); j++)
			{
				print(fallOffMap[i, j]);
			}
		}
	}

	public float[,] CreateNoiseMap(Vector2 noiseOffset)
	{
		float[,] noiseMap = Noise.CreateNoiseMap(noiseSize, noiseSize, noiseData.seed, noiseData.mapScale, noiseData.octaves, noiseData.lacularity, noiseData.persistence, noiseOffset + noiseData.mapOffset);
		noiseMap = Noise.NormalizeGlobalNoiseMap(noiseMap);
		//noiseMap = Noise.NormalizeLocalNoiseMap(noiseMap);

		if (noiseData.useFallOff)
		{
			noiseMap = FallOffMapGenerator.ApplyFallOffMap(noiseMap, fallOffMap);
		}

		return noiseMap;
	}

	public MapData GenerateMapData(Vector2 noiseOffset)
	{
		float[,] noiseMap = CreateNoiseMap(noiseOffset);
		Color[] colorMap = textureGenrator.CreateColorMap(noiseMap);
		MapData mapData = new MapData(noiseMap, colorMap);
		return mapData;
	}

	public MeshData GenerateMeshData(MapData mapData)
	{
		return TextureGenrator.CreateTerrainMesh(mapData.noiseMap, terrainData.noiseAmplification, terrainData.heightCurve, 1, terrainData.useFlatShading, noiseSize / mapMassSize);
	}

	void Update()
	{
		if (mapThreadDataInfoQueue.Count > 0)
		{
			MapThreadInfo<MapData> threadInfo = mapThreadDataInfoQueue.Dequeue();
			threadInfo.callback(threadInfo.paramater);
		}

		if (meshThreadDataInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshThreadDataInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshThreadDataInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.paramater);
			}
		}
	}

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T paramater;

		public MapThreadInfo(T paramater, Action<T> callback)
		{
			this.callback = callback;
			this.paramater = paramater;
		}
	}
}

public struct MapData
{
	public float[,] noiseMap;
	public Color[] colorMap;
	public int lod;

	public MapData(float[,] noiseMap, Color[] colorMap, int lod = 1)
	{
		this.noiseMap = noiseMap;
		this.colorMap = colorMap;
		this.lod = lod;
	}
}