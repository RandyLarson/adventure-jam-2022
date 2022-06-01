using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepOnScreen : MonoBehaviour
{
    private Camera cam;
    void Start()
    {
        cam = FindObjectOfType<Camera>();
    }
    void Update()
    {
        var r = GetComponentInChildren<Renderer>();
        if(r)
        {
            float xOffset = 0;
            float yOffset = 0;
            var minPoint = cam.WorldToScreenPoint(r.bounds.min);
            var maxPoint = cam.WorldToScreenPoint(r.bounds.max);
            if(minPoint.x < 0)
                xOffset = -minPoint.x;
            if(minPoint.y < 0)
                yOffset = -minPoint.y;

            if(maxPoint.x > cam.pixelWidth)
                xOffset = cam.pixelWidth - maxPoint.x;
            
            if(maxPoint.y > cam.pixelHeight)
                yOffset = cam.pixelHeight - maxPoint.y;

            gameObject.transform.Translate(new Vector3(xOffset, yOffset, 0));
        }     
    }
}
