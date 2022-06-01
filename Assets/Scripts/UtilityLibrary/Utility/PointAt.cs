using UnityEngine;

public class PointAt : MonoBehaviour
{
	public GameObject Target;

    void Update()
    {
		if (Target != null)
		{
			transform.up = Target.transform.position - transform.position;
		}        
    }
}
