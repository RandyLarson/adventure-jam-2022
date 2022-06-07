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

    public void Init(Camera camera, Rect bounds, float padding = 0f)
    {
        Camera = camera;

        if (Camera != null)
        {
            Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, -10);

            Vector3 topRight = Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            Vector3 bottomLeft = Camera.ViewportToWorldPoint(new Vector3(0, 0, 0));

            float cxViewPort = topRight.x - bottomLeft.x;
            float cyViewPort = topRight.y - bottomLeft.y;

            XRange.min = (bounds.xMin + cxViewPort / 2) - padding;
            XRange.max = (bounds.xMax - cxViewPort / 2) + padding;

            YRange.min = (bounds.yMin + cyViewPort/2) - padding;
            YRange.max = (bounds.yMax - cyViewPort/2) + padding;

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
        // only update lookahead pos if accelerating or changed direction
        if (ToFollow == null)
        {
            //Debug.LogError(string.Format("Camera `{0}` has nothing to follow.", gameObject.tag.LogValue()));
            return;
        }

        //float xMoveDelta = (ToFollow.position - m_LastTargetPosition).x;

        //bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        //if (updateLookAheadTarget)
        //{
        //	m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
        //}
        //else
        //{
        //	m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        //}

        //Vector3 aheadTargetPos = ToFollow.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
        //Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        Vector3 newPos = new Vector3(ToFollow.position.x, ToFollow.position.y, Camera.transform.position.z);
        if (XRange.min != 0 && XRange.max != 0)
            newPos.x = Mathf.Clamp(newPos.x, XRange.min, XRange.max);

        if (YRange.min != 0 && YRange.max != 0)
            newPos.y = Mathf.Clamp(newPos.y, YRange.min, YRange.max);

        Camera.transform.position = newPos;

        m_LastTargetPosition = ToFollow.position;
    }
}
