using System;
using UnityEngine;

public class Rotation : MonoBehaviour 
{
	public bool IsEnabled = true;
	public float RotationSpeed = 10f;
	public Axis RotationAxis = Axis.Z;
	public bool RotateWrtVelocity = false;

	private Vector3 RotationVector = Vector3.back;
	private Rigidbody2D OurRb { get; set; }

	public void RotateTo(float angle, float inTime)
    {
		IsEnabled = true;

    }

	private void Start()
	{
		OurRb = GetComponent<Rigidbody2D>();
		RotationVector = RotationAxis.AsVector();
	}

	void Update ()
	{
		float workingSpeed = RotationSpeed;

		if ( RotateWrtVelocity && OurRb != null)
        {
			workingSpeed = OurRb.velocity.magnitude * Mathf.Sign(OurRb.velocity.x);
        }

		transform.Rotate(RotationVector, workingSpeed*Time.deltaTime);
	}

    internal void Reset()
    {
		transform.rotation = Quaternion.identity;
    }
}
