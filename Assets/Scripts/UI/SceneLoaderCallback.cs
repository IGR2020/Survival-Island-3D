using UnityEngine;
using UnityEngine.UI;

public class SceneLoaderCallback : MonoBehaviour
{
	public Slider loadingBar;
	public float speed = 2f;

	public void Start()
	{
		loadingBar.value = 0;
	}

	private void Update()
	{
		loadingBar.value = Mathf.Clamp01(Mathf.Lerp(loadingBar.value, 1, Time.deltaTime * speed));
		if (loadingBar.value > 0.8f)
		{
			SceneLoader.Callback();
		}
	}
}
