using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickerController : MonoBehaviour
{
    public bool Active = true;
    public float Frequency = .1f;
    public Range FlickerRange = new Range(.9f, 1f);
    public Range FlickerDuration = new Range(.05f, .1f);
    public float OrgIntensity;
    public Light2D LightToControl;

    [ReadOnly]
    public float FlickerEndsAt = float.MaxValue;

    private void Start()
    {
        LightToControl = GetComponent<Light2D>();
        if ( LightToControl != null )
        {
            OrgIntensity = LightToControl.intensity;
        }
    }

    void Update()
    {
        if (LightToControl == null)
            return;

        if (!Active)
        {
            LightToControl.intensity = OrgIntensity;
            return;
        }

        if ( Time.time > FlickerEndsAt)
        {
            LightToControl.intensity = OrgIntensity;
            FlickerEndsAt = float.MaxValue;
            return;
        }

        if ( Random.value < Frequency )
        {
            float deltaI = Random.Range(FlickerRange.min, FlickerRange.max);
            LightToControl.intensity = OrgIntensity + deltaI;
            FlickerEndsAt = Time.time + Random.Range(FlickerDuration.min, FlickerDuration.max);
        }


        
    }
}
