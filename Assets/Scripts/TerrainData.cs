using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject
{
	public bool useFlatShading = false;

	public float noiseAmplification;
	public AnimationCurve heightCurve;

	public float waterHeight;

	public FilterMode filterMode = FilterMode.Point;

	public RegionData regionData;
}
