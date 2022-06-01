using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotator : MonoBehaviour {

	public float tumble;
	// Use this for initialization
	void Start () {
		var rb = GetComponent<Rigidbody2D>();
		rb.angularVelocity = Random.Range(5,10) * tumble;
	}
}
