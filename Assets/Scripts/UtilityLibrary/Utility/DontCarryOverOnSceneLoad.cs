using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontCarryOverOnSceneLoad : MonoBehaviour
{
	private void Start()
	{
		GameObject.Destroy(gameObject);
	}
}
