using UnityEngine;

public class DiscreteRotator : MonoBehaviour
{
    public bool IsEnabled = false;
    public Axis RotationAxis = Axis.Z;
    public bool RotateBackToStart = true;
    public float EndAngle = 90f;
    public float RotationTime = 5f;
    
    [Tooltip("The object to rotate, or self if left empty.")]
    public GameObject ToRotate;

    private float StartRotationTime = 0;
    private float EndRotationTime = 0;
    private float StartingAngle = 0;

    private GameObject ItemToRotate => ToRotate != null ? ToRotate : gameObject;

    private void Update()
    {
        if (!IsEnabled)
            return;

        float percentComplete = (Time.time - StartRotationTime) / (EndRotationTime - StartRotationTime);

        if (percentComplete >= 1)
        {
            IsEnabled = false;
            return;
        }

        float angle = Mathf.Lerp(0, EndAngle*2, percentComplete);
        if (RotateBackToStart && angle > EndAngle)
            angle = EndAngle - (angle - EndAngle);

        ItemToRotate.transform.rotation = Quaternion.AngleAxis(angle, RotationAxis.AsVector());
    }

    public void DoRotation()
    {
        StartRotationTime = Time.time;
        EndRotationTime = StartRotationTime + RotationTime;
        IsEnabled = true;
        StartingAngle = ItemToRotate.transform.rotation.z;
    }
}
