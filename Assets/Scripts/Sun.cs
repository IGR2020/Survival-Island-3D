using UnityEngine;

public class Sun : MonoBehaviour
{
	public float SecondsInOneDay;
	float rotateSpeed;

	private void Start()
	{
		rotateSpeed = 1 / SecondsInOneDay * 180;
	}

	private void Update()
	{
		transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
	}

	private void OnValidate()
	{
		rotateSpeed = 1 / SecondsInOneDay * 180;
	}
}
