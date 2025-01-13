using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class FallOffMapGenerator
{
	public static float[,] CreateFallOffMap(int size, float maxHeight = 0.2f)
	{
		float[,] map = new float[size, size];

		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float x = i / (float) size * 2 - 1;
				float y = j / (float)size * 2 - 1;

				float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

				map[i, j] = Mathf.Min(Evaluvate(value), maxHeight);
			} 
		}

		return map;
	}

	public static float[,] ApplyFallOffMap(float[,] heightMap, float[,] fallOffMap)
	{
		for (int x = 0 ; x < heightMap.GetLength(0); x++)
		{
			for (int y = 0; y < heightMap.GetLength(1); y++)
			{
				heightMap[x, y] = Mathf.Clamp01(heightMap[x, y] - fallOffMap[x, y]);
			}
		}

		return heightMap;
	}

	static float Evaluvate(float value)
	{
		float a = 3;
		float b = 2.2f;

		return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
	}
}
