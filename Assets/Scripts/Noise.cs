using UnityEngine;

public static class Noise
{
	public static float minNoiseHeight = float.MaxValue;
	public static float maxNoiseHeight = float.MinValue;
	public static float[,] CreateNoiseMap(int width, int height, int seed,
		float scale, int octaves, float lacularity, float persistence, Vector2 offset)
	{
		float[,] noiseMap = new float[width, height];
		float halfWidth = width / 2f;
		float halfHeight = height / 2f;

		Vector2[] octaveOffsets = new Vector2[octaves];
		System.Random randInt = new System.Random(seed);

		for (int i = 0; i < octaves; i++)
		{
			octaveOffsets[i] = new Vector2(randInt.Next(-100_000, 100_000) + offset.x,
				randInt.Next(-100_000, 100_000) - offset.y);
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++)
				{
					float xSample = (x - halfWidth + octaveOffsets[i].x) * scale * frequency;
					float ySample = (y - halfHeight + octaveOffsets[i].y) * scale * frequency;

					noiseHeight += (Mathf.PerlinNoise(xSample, ySample) * 2 * amplitude) - 1;

					amplitude *= persistence;
					frequency *= lacularity;
				}

				noiseMap[x, y] = noiseHeight;
			}
		}

		return noiseMap;
	}

	public static float[,] NormalizeLocalNoiseMap(float[,] noiseMap)
	{
		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);

		float minLocalNoiseHeight = float.MaxValue;
		float maxLocalNoiseHeight = float.MinValue;

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
				float noiseHeight = noiseMap[x, y];

				if (noiseHeight < minNoiseHeight)
				{
					minLocalNoiseHeight = noiseHeight;
				}
				if (noiseHeight > maxNoiseHeight)
				{
					maxLocalNoiseHeight = noiseHeight;
				}
			}
		}

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
				noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
			}
		}

		return noiseMap;
	}

	public static float[,] NormalizeGlobalNoiseMap(float[,] noiseMap)
	{
		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
				float noiseHeight = noiseMap[x, y];

				if (noiseHeight < minNoiseHeight)
				{
					minNoiseHeight = noiseHeight;
				}
				if (noiseHeight > maxNoiseHeight)
				{
					maxNoiseHeight = noiseHeight;
				}
			}
		}

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
				noiseMap[x, y] = Mathf.InverseLerp(Mathf.Min(minNoiseHeight, -3.8f), Mathf.Max(-0.5f, maxNoiseHeight), noiseMap[x, y]);
			}
		}

		return noiseMap;
	}
}

