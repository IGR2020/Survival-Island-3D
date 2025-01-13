using UnityEngine;
using System.Collections.Generic;

public class TextureGenrator : MonoBehaviour
{
	static MapGenerator mapGenerator;

	public int transitionSize = 100;
	public int blendCount = 1;

	public Color[] smoothColorTransition;
	public Texture2D smoothColorTransitionTexture;

	public List<int> textureRange;
	public List<int> textureRefrence;
	public int textureSize = int.MaxValue;

	public MeshRenderer transitionRenderer;

	void Start()
	{
		mapGenerator = FindFirstObjectByType<MapGenerator>();

		GameObject smoothColorTransitionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		smoothColorTransitionPlane.transform.position = transform.position;
		smoothColorTransitionPlane.transform.parent = transform;

		transitionRenderer = smoothColorTransitionPlane.GetComponent<MeshRenderer>();

		smoothColorTransitionTexture = new Texture2D(1, transitionSize);
		smoothColorTransitionTexture.filterMode = FilterMode.Point;

		smoothColorTransition = new Color[transitionSize];

		for (int i = 0; i < mapGenerator.terrainData.regionData.regions.Length; i++) 
		{
			Region region = mapGenerator.terrainData.regionData.regions[i];
			if (region.drawType == Region.RegionDrawType.Texture)
			{
				try
				{
					GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
					newObject.transform.parent = transform;
					MeshRenderer newObjectRenderer = newObject.GetComponent<MeshRenderer>();
					newObjectRenderer.material.mainTexture = region.texture;
					region.textureColorMap = Make2DArray(region.texture.GetPixels(), region.texture.width, region.texture.height);
					if (region.texture.width < textureSize)
					{
						textureSize = region.texture.width;
					}
				}
				catch (System.NullReferenceException)
				{
					region.drawType = Region.RegionDrawType.Color;
				}
				mapGenerator.terrainData.regionData.regions[i] = region;
			}
		}

		CreateSmoothTransitionColor();
	}

	public static T[,] Make2DArray<T>(T[] input, int height, int width)
	{
		T[,] output = new T[height, width];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				output[i, j] = input[i * width + j];
			}
		}
		return output;
	}

	public Region GetRegionFromIndex(int index)
	{
		return mapGenerator.terrainData.regionData.regions[index];
	}

	[ContextMenu("Recreate Transition Color Array")]
	public void CreateSmoothTransitionColor()
	{
		for (int y = 0; y < transitionSize; y++)
		{
			for (int i = 0; i < mapGenerator.terrainData.regionData.regions.Length; i++)
			{
				Region region = mapGenerator.terrainData.regionData.regions[i];
				if (y / (float)transitionSize <= region.height)
				{	
					if (region.drawType == Region.RegionDrawType.Texture)
					{
						try
						{
							if (region.texture == null) { }
							textureRange.Add(y);
							textureRefrence.Add(i);
						}
						catch (System.NullReferenceException)
						{
							
						}
					}
					
					smoothColorTransition[y] = region.color;
					break;
				}
			}
		}

		for (int i = 0; i < blendCount; i++)
		{
			for (int y = 0; y < transitionSize; y++)
			{
				if (y == 0 || y == transitionSize - 1)
				{
					continue;
				}

				Color mixedColor = (smoothColorTransition[y] + smoothColorTransition[y - 1] + smoothColorTransition[y + 1]) / 3;
				smoothColorTransition[y] = mixedColor;
			}
		}

		smoothColorTransitionTexture.SetPixels(smoothColorTransition);
		smoothColorTransitionTexture.Apply();

		transitionRenderer.material.mainTexture = smoothColorTransitionTexture;
	}

	public static Texture2D CreateNoiseTexture(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[height * width];

        for (int y = 0; y < noiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < noiseMap.GetLength(0); x++) 
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }


	public Color[] CreateColorMap(float[,] noiseMap)
	{
		// Non Color Transitionary
		//for (int i = 0; i < regions.Length; i++) {
		//	if (noiseMap[x, y] <= regions[i].height) {
		//		colorMap[y * width + x] = regions[i].color;
		//		break;}}

		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);

		Color[] colorMap = new Color[width * height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int transitionIndex = Mathf.Clamp(Mathf.RoundToInt(noiseMap[x, y] * transitionSize), 0, transitionSize - 1);
				if (textureRange.Contains(transitionIndex))
				{
					int index = textureRange.IndexOf(transitionIndex);
					Region region = GetRegionFromIndex(textureRefrence[index]);
					Color color = region.textureColorMap[Mathf.Min(x, textureSize - 1), Mathf.Min(y, textureSize - 1)];
					colorMap[y * width + x] = color;
				}
				else
				{
					colorMap[y * width + x] = smoothColorTransition[transitionIndex];
				}
			}
		}

		return colorMap;
	}

    public static Texture2D CreateColorTexture(float[,] noiseMap, Color[] colorMap, FilterMode filterMode = FilterMode.Point)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

		Texture2D texture = new Texture2D(width, height);


		texture.filterMode = filterMode;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

	// when the same height curve is accesed by multipule threads it gives odd value so we create a new height curve
	public static MeshData CreateTerrainMesh(float[,] heightMap, float amplification, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading = false, int resolutionDownscale = 1)
	{
		AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

		int width = heightMap.GetLength(0) / resolutionDownscale;
		int height = heightMap.GetLength(1) / resolutionDownscale;
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int meshSimplificationIncrement = levelOfDetail;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData(verticesPerLine, verticesPerLine, useFlatShading);
		int vertexIndex = 0;

		for (int y = 0; y < height; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < width; x += meshSimplificationIncrement)
			{
				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x * resolutionDownscale, y * resolutionDownscale]) * amplification, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		meshData.FinalizeData();

		return meshData;

	}
}

public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	public int lod;

	int triangleIndex;
	bool useFlatShading = false;

	public MeshData(int meshWidth, int meshHeight, bool useFlatShading = false, int lod = 1)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
		this.useFlatShading = useFlatShading;
		this.lod = lod;
	}

	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	Vector3[] CalculateNormals()
	{
		Vector3[] vertexNormals = new Vector3[vertices.Length];
		int triangleCount = triangles.Length / 3;
		
		for (int i = 0; i < triangleCount; i++)
		{
			int normalTriangleIndex = i * 3;
			 
			int vertexIndexA = triangles[normalTriangleIndex];
			int vertexIndexB = triangles[normalTriangleIndex + 1];
			int vertexIndexC = triangles[normalTriangleIndex + 2];

			Vector3 triangleNormal = surfaceNormalFromIndicies(vertexIndexA, vertexIndexB, vertexIndexC);

			vertexNormals[vertexIndexA] += triangleNormal;
			vertexNormals[vertexIndexB] += triangleNormal;
			vertexNormals[vertexIndexC] += triangleNormal;
		}

		for (int i = 0; i < vertexNormals.Length; i++)
		{
			vertexNormals[i].Normalize();
		}

		return vertexNormals;
	}

	Vector3 surfaceNormalFromIndicies(int a, int b, int c)
	{
		Vector3 pointA = vertices[a];
		Vector3 pointB = vertices[b];
		Vector3 pointC = vertices[c];

		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;

		return Vector3.Cross(sideAB, sideAC).normalized;
	}


	public void FlatShading()
	{
		Vector3[] flatShadedVerticies = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++)
		{
			flatShadedVerticies[i] = vertices[triangles[i]];
			flatShadedUvs[i] = uvs[triangles[i]];
			triangles[i] = i;
		}

		vertices = flatShadedVerticies;
		uvs = flatShadedUvs;
	}

	public void FinalizeData()
	{
		if (useFlatShading)
		{
			FlatShading();
		}
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}

}

[System.Serializable]
public struct Region
{
    public string name;
	public enum RegionDrawType {Texture, Color};
	public RegionDrawType drawType;
    public Color color;
	public Texture2D texture;
    public float height;
	[HideInInspector()]
	public Color[,] textureColorMap;
}
