using UnityEngine;
using System.Collections;

public class DeleteAfterDelay : MonoBehaviour
{
	public float delay = 300.0f;

	void Update()
	{
		delay -= Time.deltaTime;
		if (delay < 0f)
			GameObject.Destroy(this.gameObject);
	}
	
	public void SetDelay(float time)
	{
		delay = time;
	}
	
}
