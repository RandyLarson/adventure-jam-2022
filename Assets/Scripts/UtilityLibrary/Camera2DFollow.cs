using System;
using Assets.Scripts.Extensions;
using UnityEngine;

public class Camera2DFollow : MonoBehaviour
{
    public Transform ToFollow;
    public float damping = 1;
    public float lookAheadFactor = 3;
    //public float lookAheadReturnSpeed = 0.5f;
    //public float lookAheadMoveThreshold = 0.1f;
    public Range YRange;
    public Range XRange;
    public Camera Camera;

    private float m_OffsetZ;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    // Use this for initialization
    private void Start()
    {
    }

    public void SetTarget(Transform newTarget)
    {
        ToFollow = newTarget;
        if (ToFollow != null)
        {
            m_LastTargetPosition = ToFollow.position;
            m_OffsetZ = (Camera.transform.position - ToFollow.position).z;
            //Camera.transform.parent = null;
        }
    }

    public void Init(Camera camera, Rect bounds)
    {
        Camera = camera;

        if (Camera != null)
        {
            Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, -10);

            Vector3 topRight = Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            Vector3 bottomLeft = Camera.ViewportToWorldPoint(new Vector3(0, 0, 0));

            float cxViewPort = topRight.x - bottomLeft.x;
            float cyViewPort = topRight.y - bottomLeft.y;

            XRange.min = (bounds.xMin + cxViewPort/2);
            XRange.max = (bounds.xMax - cxViewPort/2);

            YRange.min = (bounds.yMin + cyViewPort/2);
            YRange.max = (bounds.yMax - cyViewPort/2);

            if (XRange.min > XRange.max)
                XRange.max = XRange.min;

            if (YRange.min > YRange.max)
                YRange.max = YRange.min;

            SetTarget(ToFollow);
        }
    }

    private void Update()
    {
        DoFollow();
    }


    private void DoFollow()
    {
        if (ToFollow == null)
        {
            return;
        }

        Vector3 newPos = new Vector3(ToFollow.position.x, ToFollow.position.y, Camera.transform.position.z);
        newPos.x = Mathf.Clamp(newPos.x, XRange.min, XRange.max);
        newPos.y = Mathf.Clamp(newPos.y, YRange.min, YRange.max);

        Camera.transform.position = newPos;
        m_LastTargetPosition = ToFollow.position;
    }
}
