using UnityEngine;

public class DestroyByTime : MonoBehaviour 
{

	[Tooltip("A random range that will be picked from if the random box is checked. Otherwise, the min value is used.")]
	public Range lifetimeRange;
	[Tooltip("If not checked, the min value is used.")]
	public bool randomBeforeLifetime = false;

	private void Start()
	{
		var ttl = randomBeforeLifetime ? Random.Range(lifetimeRange.min, lifetimeRange.max) : lifetimeRange.min;
		Destroy(gameObject, ttl);
	}
}
