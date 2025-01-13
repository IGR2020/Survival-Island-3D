using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject
{
	public bool useFallOff;
	public float maxFallOffHeight = 0.1f;

	public int seed;

	public int octaves;
	public float lacularity;
	[Range(0f, 1f)]
	public float persistence;

	public float mapScale;
	public Vector2 mapOffset;
}
