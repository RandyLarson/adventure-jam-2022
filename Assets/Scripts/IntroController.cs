using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntroController : MonoBehaviour
{

    public int NumBwCycles = 0;
    public int NumColorCycles = 0;
    public int TransitionBwToColorAtCycleNum = 5;
    public int TransitionToGameAfter = 5;
    public UnityEvent OnComplete;

    void Update()
    {
        if ( NumBwCycles >= TransitionBwToColorAtCycleNum)
        {
            if ( TryGetComponent<Animator>(out var anim))
            {
                anim.SetBool("TransitionToColor", true);
            }
        }
    }

    public void BwCycleComplete()
    {
        NumBwCycles++;
    }

    public void ColorCycleComplete()
    {
        if (NumColorCycles > TransitionToGameAfter)
            OnComplete?.Invoke();
    }

}
